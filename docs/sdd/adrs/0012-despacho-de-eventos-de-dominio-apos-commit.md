# ADR 0012: Efeitos colaterais não-transaccionais despachados após o commit

## Status
Aceite

## Contexto
Comandos que implementam `ITransactional` correm **dentro** de uma transacção: `TransactionBehavior` embrulha a execução em `UnityOfWork.ExecuteInTransactionAsync`, que só faz `SaveChangesAsync` + `CommitAsync` depois de o handler devolver.

Isso é o que se quer para escrita em base de dados, e é exactamente o que **não** se quer para efeitos que não sabem participar de uma transacção. O caso que forçou a decisão foi o e-mail de andamento de candidatura: enviado no corpo do handler, uma falha posterior na transacção reverteria a mudança de status e deixaria o candidato com um e-mail a dizer que foi aprovado numa candidatura que continua em análise. **Um e-mail não se desfaz num rollback.** A mesma armadilha vale para qualquer efeito externo: webhook, publicação em fila, notificação push.

Havia três caminhos:

1. **Enviar no handler.** Simples e errado pelo motivo acima.
2. **Outbox persistente.** Correcto e à prova de queda de processo, mas exige tabela, migration, processo de drenagem, política de retry e de veneno, e monitorização — infraestrutura considerável para um efeito colateral cujo pior caso é "um e-mail não chegou".
3. **Fila em memória por requisição, drenada depois do commit.** É o meio-termo: elimina o modo de falha grave (efeito sobre operação revertida) sem introduzir infraestrutura nova.

O mediator interno (`EmpregaNet.Domain.Libs.Mediator`) já tinha `Publish` e `INotificationHandler<T>` implementados e **sem nenhum uso** no repositório. A decisão activa mecanismo existente em vez de acrescentar um barramento paralelo.

## Decisão
- **`IDomainEventQueue`** (Application/Abstractions), implementada por `DomainEventQueue` (Infra/Events) com ciclo de vida **`Scoped`** — uma fila por requisição. Handlers **declaram** factos com `Enqueue`; não executam efeitos externos.
- **`NotificationDispatchBehavior<TRequest,TResponse>`** (Infra/Behaviors) drena a fila depois de `next()` devolver com êxito e publica cada evento via `IMediator.Publish`.
- **A ordem de registo é o mecanismo, e é frágil de propósito reconhecido.** O pipeline é montado de trás para frente sobre `GetServices` (`Mediator.RequestHandlerWrapperImpl`), portanto o **primeiro registado é a camada mais externa**. `NotificationDispatchBehavior` é registado **antes** de `TransactionBehavior`, e é só isso que o coloca fora da transacção. Trocar as duas linhas compila e passa em qualquer teste de caminho feliz. O registo vive num método próprio — `DependencyInjection.AddPipelineBehaviors`, `internal` e visível aos testes — precisamente para que o teste que afirma a ordem (`PipelineBehaviorRegistrationTests.Command_DespachoDeEventos_DeveSerMaisExternoQueATransacao`) exercite **esse** registo em vez de recopiar as linhas: um teste que monte a sua própria `ServiceCollection` verifica apenas que a colecção preserva ordem de inserção, e continuaria verde com a produção invertida.
- **Se a requisição falhar, a fila não é drenada.** A excepção propaga e os eventos morrem com o escopo. Nenhum efeito externo sobre operação revertida.
- **A publicação nunca derruba o comando.** Neste ponto o commit já aconteceu; uma falha de envio é registada em log e a resposta segue com êxito. Cada evento é publicado no seu próprio `try/catch`, para um destinatário inválido não calar os restantes.
- **`Enqueue` tem semântica de conjunto**, por identidade lógica do evento (`IIdentifiableNotification.DeduplicationKey`) quando o evento a declara, e por igualdade estrutural quando não. É defesa em profundidade: a garantia primária de não duplicar vem das invariantes do domínio (`ChangeStatus` recusa transição para o status actual), e a fila impede que uma segunda declaração do mesmo facto, na mesma requisição, se torne um segundo e-mail.
- **Handlers de notificação nunca relançam.** A garantia é declarada no próprio handler, não delegada a quem o invoca.

## Consequências

**Positivas:**
- Desaparece a classe de bug "efeito externo sobre operação revertida", que é irreversível do lado de quem recebe.
- Handlers de comando ficam sem conhecimento de e-mail, template ou transporte: declaram o facto e acabam. O efeito é acrescentado ou removido registando um `INotificationHandler`, sem tocar no comando.
- Activa `Publish`/`INotificationHandler` que já existiam sem uso, em vez de introduzir um segundo barramento.
- O ponto de extensão é único e nomeado: quando aparecer um segundo efeito (push, webhook, analytics), sabe-se onde vive.

**Negativas / cuidados:**
- **Não é durável.** Se o processo cair entre o commit e o `Drain`, os eventos perdem-se sem rasto — o comando ficou gravado e a notificação nunca sai. É a diferença face a um outbox, e é o custo aceite: o gatilho para reconsiderar é perda de notificação com impacto reportado.
- **A correcção depende de duas linhas de registo por ordem.** É a fragilidade central deste desenho. Mitigada por um teste que resolve os behaviors a partir do registo real, por comentário no ponto de registo e por este ADR — não por impossibilidade estrutural.
- **A publicação corre dentro da requisição HTTP**, portanto o tempo de resposta inclui o envio dos e-mails. No encerramento de vaga são N envios (um por candidatura afectada) num único pedido. Não há trabalho em background; se a latência incomodar, o passo seguinte é despachar para fora do pedido — o que reabre a questão da durabilidade.
- **Falha de efeito é invisível para o utilizador, por decisão.** Quem clicou vê sucesso mesmo que nenhum e-mail tenha saído. A única evidência é o log, e não há alerta nem reenvio automático: hoje a recuperação é manual e depende de alguém reclamar.
- **A fila é `Scoped` e sem sincronização.** Um handler que paralelize trabalho que enfileire eventos corromperia a lista. Está documentado na implementação, mas nada no compilador o impede.
