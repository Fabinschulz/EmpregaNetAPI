# ADR 0013: Estado por tentativa no limite do retry transaccional

## Status
Aceite

## Contexto

`DatabaseConfig` liga o retry do provider no `DbContext` inteiro:

```csharp
npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
```

`UnityOfWork.ExecuteInTransactionAsync` é o **único** sítio do backend que obtém uma execution strategy, e o `TransactionBehavior` é o **único** chamador desse método. Logo, todo comando `ITransactional` — e `ITransactional` está em `CreateCommand`, `UpdateCommand` e `DeleteCommand`, portanto em **todo o CRUD** — corre sob uma estratégia que, ao classificar uma falha como transitória, **reexecuta o delegate inteiro**: `BeginTransaction`, o corpo do handler, `SaveChanges`, `Commit`.

A reexecução usa o **mesmo** `PostgreSqlContext` — é um serviço `Scoped`, resolvido uma vez por requisição — e o EF **não** sobrescreve valores de uma entidade já rastreada. Até esta decisão, nada repunha o `ChangeTracker` entre tentativas. Daí duas consequências, ambas verificadas no código real:

**1. Erro de negócio falso, e barulhento.** A segunda tentativa reencontra as entidades com a mutação da tentativa que foi revertida na base, e as guardas de agregado disparam sobre estado que **nunca** foi commitado:

| Handler | O que o utilizador vê numa operação que não aconteceu |
| ------- | ----------------------------------------------------- |
| `CloseJobHandler` | 400 "A vaga já está encerrada." |
| `ChangeJobApplicationStatusHandler` | "A candidatura já está no status informado." |
| `CancelJobApplicationHandler` | "Esta candidatura não pode mais ser cancelada." (via `CancelByCandidate`, não via `ChangeStatus`) |

**2. Escrita perdida em silêncio, e é a caro.** `BaseRepository.CreateAsync`/`UpdateAsync` — e o `UserManager` — chamam `SaveChangesAsync` eles próprios, dentro da transacção. Um `SaveChanges` com êxito marca os entries como `Unchanged`; o rollback reverte **a base**, não o tracker. Onde não há guarda, o `SaveChangesAsync` final do `UnityOfWork` não emite `UPDATE` para aquela entidade e o comando devolve **200 sem ter escrito nada** — sem log, sem erro, sem sintoma. `DeleteUserHandler` é o caso mais desagradável dos dois mundos: a sua guarda de idempotência (`if (user.IsDeleted) return true;`) transformava a segunda tentativa num **sucesso falso**.

Nada disto apareceu em testes porque a suíte corre sobre `UseInMemoryDatabase`: sem execution strategy, sem transacções reais, e portanto sem segunda tentativa. Foi essa a razão de o defeito passar, e continua a ser a razão pela qual só parte dele é verificável (§ Consequências).

Há um terceiro efeito, de natureza diferente: **efeito externo repetido**. Uma tentativa que chegou a enviar um e-mail e foi anulada deixa o e-mail enviado, e a tentativa seguinte envia o segundo. Nenhuma limpeza de tracker corrige isso — um e-mail não se desfaz. O `RegisterUserHandler` enviava a confirmação de conta no corpo do handler, dentro da transacção.

## Decisão

**1. O `UnityOfWork` limpa o `ChangeTracker`, e só a partir da segunda tentativa.**

```csharp
var attempt = 0;
// ... dentro do delegate da estratégia:
if (attempt++ > 0)
{
    context.ChangeTracker.Clear();
}
```

A condição **é** a decisão, não um detalhe de implementação. Limpar antes da primeira tentativa destacaria entidades rastreadas mais cedo na requisição por código que nada tem a ver com esta transacção — Identity, o `UserManager` resolvido em `JobEmployerAccess`. A semântica desejada é "repor o estado que uma tentativa falhada sujou", não "começar com o tracker vazio", e a diferença custa um contador local.

**2. O `UnityOfWork` repõe o estado de que é dono, e nada mais.** Não conhece `IDomainEventQueue`, `IMediator` nem tipos de notificação, e não passará a conhecer. Repor a fila de eventos daqui faria a camada de persistência depender de notificações para corrigir um problema de persistência.

**3. A regra que torna o retry defensável não é o `Clear()` — é esta:**

> **Um handler `ITransactional` não executa efeito externo que um rollback não desfaça.**

Efeito externo é e-mail, webhook, publicação em fila, notificação push, invalidação de cache, chamada HTTP com escrita. O handler **declara** o facto em `IDomainEventQueue`; quem executa é um `INotificationHandler`, publicado pelo `NotificationDispatchBehavior` **depois** do commit. Isto **estende o [ADR 0012](0012-despacho-de-eventos-de-dominio-apos-commit.md)**: o que lá era o desenho de "e-mail de andamento de candidatura" passa a ser a regra para **qualquer** efeito externo em comando transaccional. O 0012 continua válido; este ADR generaliza-lhe o âmbito e acrescenta a segunda razão para a regra — o 0012 argumentava com o rollback, e o retry acrescenta a repetição.

**4. `RegisterUserHandler` deixa de enviar o e-mail de confirmação.** Passa a enfileirar `UserRegistered`; `UserRegisteredEmailHandler` gera o token e envia, já depois do commit. O token é gerado **no consumidor**, contra o utilizador lido da base pelo id do evento — não viaja no evento, porque um token de confirmação é credencial.

**5. `verifySucceeded` continua `null`, deliberadamente.** Se o `COMMIT` tiver êxito e o reconhecimento se perder na rede, a estratégia reexecuta e a tentativa seguinte lê estado **já commitado**: a guarda dispara com a mensagem agora **verdadeira** ("a vaga já está encerrada" — está mesmo), e o utilizador vê um erro sobre uma operação que ficou gravada. Verificar o desfecho exigiria um predicado por comando, e o `TransactionBehavior` é genérico em `TRequest` — não tem como o fornecer. É limite conhecido e está escrito no `<remarks>` do método, para não ser redescoberto como bug.

### Alternativas rejeitadas

| Alternativa | Porque não |
| ----------- | ---------- |
| **Desligar `EnableRetryOnFailure`** | É global ao `DbContext`, e a maioria do tráfego é **leitura idempotente fora de transacção** (feed de vagas, detalhe, OData), onde o retry é a cura de manual para *deadlock* e queda transitória de conexão. Desligá-lo trocaria um defeito raro de escrita por fragilidade permanente de leitura. Retry por operação exigiria configuração paralela que o provider não expõe assim. |
| **`DbContext` novo por tentativa** | É a correcção estruturalmente certa e a mais cara: o `DbContext` é `Scoped`, e o handler, os repositórios e o `UserManager` recebem-no por construtor. Um contexto por tentativa exige um *scope* de DI por tentativa e a reinvocação do pipeline dentro dele — reescrever `TransactionBehavior` e o ciclo de vida de metade da Infra. Contra 25 linhas e um contador hoje, e sem consumidor que peça mais isolamento. Fica como caminho conhecido se o modo de falha voltar por outra via. |
| **Não fazer nada** | O modo de falha mais grave é **silencioso**: 200 sem escrita, sem log. Não há sintoma que leve alguém a investigar, e o dado perdido não se recupera depois. |
| **Repor a `IDomainEventQueue` no `UnityOfWork`** | Acoplaria persistência a notificações para resolver um dano que hoje está contido (abaixo). |

### Adiado, com gatilho

**A `IDomainEventQueue` não é reposta por tentativa.** Uma tentativa anulada deixa o evento enfileirado, e o commit da tentativa seguinte publica-o.

- **O dano está contido por duas coisas que já existem:** a `DeduplicationKey` inclui o `JobApplicationId`, portanto a segunda declaração do mesmo facto não vira segundo e-mail; e `JobApplicationStatusChangedEmailHandler` re-resolve o payload por id depois do commit, com guarda de projecção nula. O pior caso conhecido degrada para **uma linha de log**, não para e-mail errado.
- **Gatilho de retorno:** o primeiro `INotification` cujo consumidor **não** re-resolva o payload por id a partir da base, ou a primeira publicação não idempotente. Nesse dia, a fila precisa de saber esquecer o que uma tentativa falhada declarou.

## Consequências

**Positivas:**
- Desaparecem duas classes de defeito de uma vez: o erro de negócio sobre estado nunca commitado e o 200 sem escrita. A segunda era invisível.
- O retry deixa de ser uma configuração cuja consequência ninguém tinha lido. A partir daqui, `EnableRetryOnFailure` tem uma decisão escrita por trás.
- A regra do ponto 3 dá um critério para revisão de código que não depende de reconstruir o raciocínio: `IEmailSender`, `HttpClient` ou `IOutputCacheManager` no corpo de um handler `ITransactional` é defeito, sem discussão caso a caso.
- O envio da confirmação de conta ganhou o mesmo tratamento que o resto: um consumidor, testável, que não relança e cujo caminho de recuperação (`resend-confirmation`) já existia.

**Negativas / cuidados:**
- **A parte grave não é verificável com o stack de testes actual.** `InMemoryIdentityFixture` usa `UseInMemoryDatabase`: sem execution strategy e com transacção no-op, o *rollback* não é reproduzido e a escrita perdida em silêncio não é observável. O que os testes fixam é **quando** o tracker é reposto — `UnityOfWorkTests` injecta uma estratégia com uma repetição pelo ponto de extensão do próprio EF (`IExecutionStrategyFactory` via `ReplaceService`), sem nenhuma abstracção nova em produção, e a condição foi confirmada por **mutação nos dois sentidos** (nunca limpar: falha 1 teste; limpar sempre: falham 2). O comportamento transaccional real exige base real (Testcontainers), que este projecto não tem — a decisão sobre isso está com o humano.
- **O `Clear()` na segunda tentativa descarta também estado legítimo.** Entidades que código anterior ao `TransactionBehavior` deixou rastreadas com mudanças pendentes são perdidas na reexecução. Hoje isso não morde porque essas mudanças, quando existem, são gravadas pelo próprio `SaveChanges` da transacção e portanto refeitas pela tentativa seguinte; mas é dependência de facto, não de garantia. Mutação feita fora do handler e nunca salva desapareceria em silêncio na retry.
- **A regra do ponto 3 é disciplina, não impedimento.** Nada no compilador recusa um `IEmailSender` injectado num handler `ITransactional`. A varredura foi feita neste diff e o que ficou está registado abaixo; o próximo handler é responsabilidade de quem revê.
- **Sobrevive código que ainda chama `SaveChangesAsync` dentro da transacção** (`BaseRepository.CreateAsync`/`UpdateAsync`, `UserManager`, `RefreshTokenService`). O `Clear()` neutraliza a consequência, não a causa: o padrão continua a produzir *round-trips* por entidade e a tornar o `UnityOfWork` responsável por limpar o que outros sujaram. Reduzir isso é refactor próprio, sem gatilho hoje.
- **Duas coisas conhecidas continuam por corrigir**, por serem de baixa severidade e fora do âmbito deste diff: `DeleteUserHandler` e `UpdateAdminUserHandler` invalidam *output cache* dentro da transacção (uma purga sobre operação revertida é auto-curável — o pedido seguinte repovoa com o dado verdadeiro — e repetida é idempotente); e `LoginWithGoogleHandler` valida o `id_token` contra a Google dentro da transacção (leitura idempotente, sem efeito no exterior).
- **O contador de tentativas é local à chamada.** Chamadas aninhadas a `ExecuteInTransactionAsync` contariam em separado — hoje não existem, porque só o `TransactionBehavior` chama, e uma vez por requisição.
