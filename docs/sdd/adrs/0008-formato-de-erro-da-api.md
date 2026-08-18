# ADR 0008: Erro da API com lista tipada de falhas por campo

## Status
Aceite

## Contexto

Todas as respostas de erro da API saem de um único corpo, `DomainError`, escrito pelo
`GlobalExceptionHandler`. O formato anterior era:

```csharp
public required object Details { get; set; }   // { "Errors": string[] }
```

A `ValidationAppException` já carregava `IDictionary<string, string[]>` - campo → mensagens - e os
87 pontos que a lançam já passavam o nome do campo. Mas o handler achatava tudo antes de serializar:

```csharp
errors = validationException.Errors.SelectMany(e => e.Value).ToArray();  // a chave morria aqui
```

O cliente recebia então um `string[]` sem qualquer ligação ao campo de origem, e a única coisa que
podia fazer era concatenar tudo numa frase. O resultado na tela era:

> Ocorreram uma ou mais falhas de validação. Apenas candidatos podem se candidatar para vagas.

Duas informações coladas: um título genérico que não diz nada e a frase que realmente importa.
Num formulário de empresa com quatro campos inválidos o efeito era pior - quatro mensagens num
parágrafo único, nenhuma ao lado do campo que a originou.

Três problemas de contrato agravavam isto:

1. **`Details` era `object`.** O formato não aparecia no Swagger e não havia como um cliente
   tipá-lo. Como `WriteAsJsonAsync` não aplica `DictionaryKeyPolicy`, a chave saía `"Errors"`
   em PascalCase dentro de um payload camelCase - o frontend convivia com `record.Errors ?? record.errors`.
2. **`code` saía como número.** O `JsonStringEnumConverter` estava registado apenas no
   `AddJsonOptions` do MVC; o handler e os callbacks do Identity escrevem com `WriteAsJsonAsync`,
   que usa as opções de `Microsoft.AspNetCore.Http.Json`.
3. **Metade das falhas não pertence a campo nenhum.** "Apenas candidatos podem se candidatar",
   "Vaga encerrada", "Sem permissão" são regras de negócio. Os handlers passavam
   `nameof(_httpCurrentUser.UserId)` como se fossem campos, porque o construtor exigia um nome.

## Decisão

**`DomainError.Errors` é uma lista tipada; `Details` deixa de existir.**

```csharp
public class DomainError
{
    public required int StatusCode { get; set; }
    public DomainErrorEnum Code { get; set; }
    public required string Message { get; set; }
    public IReadOnlyList<DomainErrorItem> Errors { get; set; } = [];
    public required string CorrelationId { get; set; }
    public string? StackTrace { get; set; }   // só em Development
}

public sealed class DomainErrorItem
{
    public string? Field { get; set; }
    public required string Message { get; set; }
    public DomainErrorEnum Code { get; set; }
}
```

- **`Field` é nullable, e o `null` é significativo.** `null` quer dizer "esta falha não pertence a
  um campo": regra de negócio, permissão, estado do registo. O cliente mostra essas no topo do
  formulário; as demais, ao lado do input. Sem esta distinção não há como decidir onde a mensagem
  aparece, e foi exactamente essa a origem do problema.

- **`Field` é o caminho que o cliente enviou, em camelCase.** O `GlobalExceptionHandler` normaliza
  o que vem do FluentValidation e do `nameof`: `entity.Address.ZipCode` passa a `address.zipCode`.
  O segmento `entity` existe apenas no envelope `CreateCommand<T>` e o cliente nunca o enviou -
  deixá-lo no caminho tornaria o campo inendereçável. Índices de coleção são preservados
  (`items[0].name`), porque é assim que o `react-hook-form` endereça elementos de array.

- **`Message` é uma frase, não um cabeçalho fixo.** Havendo exactamente uma falha, `Message` é essa
  própria falha - tenha ela campo ou não. Só com várias é que passa a um título curto ("Corrija os
  campos destacados"). A regra não olha ao campo porque nem toda requisição vem de um formulário:
  um botão "Candidatar-se" não tem campos para destacar, e a instrução seria impossível de seguir.

- **Regra de negócio lança por `ValidationAppException.ForBusinessRule`**, que grava a falha sob
  chave vazia e sai como `field: null`. O construtor de três argumentos exigia um nome de campo, o
  que levava os handlers a passar o nome de variáveis internas (`UserId`, `currentUser`) - e o
  cliente acabava a destacar um input sem culpa, ou a ler um título de formulário fora de um.

- **`code` serializa por nome.** `JsonStringEnumConverter` registado também em
  `ConfigureHttpJsonOptions`, que é o que `WriteAsJsonAsync` usa.

- **Os 87 `throw` não mudam.** A assinatura de `ValidationAppException` fica igual; a informação
  sempre existiu, só não chegava ao fio.

**No cliente, quem distribui as falhas é o `FormProvider`.** Ao falhar um submit, ele aplica
`setError(field)` para cada falha cujo caminho existe no formulário; o resto continua a chegar
pela mensagem do topo que a página já exibe. Nenhum componente de campo precisou mudar - a camada
`FormField` já lê os erros do `react-hook-form`.

## Consequências

**Positivas**

- O erro do servidor aparece no campo que o causou, com o mesmo visual do erro de schema.
- O contrato é tipado e visível no Swagger; o `record.Errors ?? record.errors` desaparece.
- `code` legível permite ao cliente reagir a casos específicos sem depender do texto da mensagem.
- Erros de campo e de formulário deixam de ser indistinguíveis.

**Negativas e riscos aceites**

- **Mudança quebrante do corpo de erro.** Qualquer consumidor que leia `details.Errors` para de
  funcionar. Hoje o único consumidor é o frontend deste repositório, actualizado no mesmo commit.
  O BFF não intermedia erros (ver [ADR 0001](0001-auth-por-cookies-httponly.md) e o facto de o
  frontend chamar a API directamente).
- **Podem voltar a aparecer campos falsos.** Os 19 `throw` que passavam nomes de variáveis internas
  foram revistos: os de identidade, permissão e estado do registo passaram a `ForBusinessRule`, e os
  que eram mesmo campos passaram a referenciar a propriedade do comando (`nameof(Command.Cnpj)` em
  vez de `nameof(cnpjCleaned)`). Nada impede, porém, que um handler novo volte a inventar um nome.
  O cliente degrada - um caminho que o formulário não conhece não é aplicado a nenhum input - mas o
  `field` no payload continuaria a mentir. Um analisador ou uma revisão de PR é o que resta como
  guarda.

- **`UpdateMyProfileCommand.UserName` foi renomeado para `Username`.** A propriedade era a única
  divergente - `RegisterUserCommand.Username` e `UserViewModel.Username` já usavam a outra forma -
  e obrigava o frontend a traduzir `username` para `userName` ao enviar, o que tornava impossível
  ligar o erro ao campo. A ligação do Newtonsoft é insensível a maiúsculas, logo um cliente antigo
  a enviar `userName` continua a funcionar.
- **`StackTrace` é agora uma propriedade de topo.** Continua limitado a `Development`, mas está
  mais visível no contrato do que quando vivia dentro de um `object` opaco.

## Alternativas consideradas

- **Manter `Details` e apenas tipá-lo.** Deixaria a assimetria de casing das chaves de dicionário e
  não resolveria a ausência do campo - o problema não era o invólucro, era a chave descartada.
- **Adoptar `ProblemDetails` (RFC 7807) com a extensão `errors` do `ValidationProblemDetails`.**
  É o padrão do ASP.NET e teria sido a escolha inicial natural. Foi descartado agora porque
  `ValidationProblemDetails.Errors` é `IDictionary<string, string[]>` - a mesma estrutura que não
  consegue representar uma falha sem campo, que é metade dos nossos casos - e porque migrar o
  `DomainErrorEnum` e o `correlationId` para extensões arbitrárias custaria mais do que o formato
  próprio, já consumido de ponta a ponta.
- **Devolver 422 em vez de 400 para falhas de validação.** Sem ganho prático para este cliente e
  quebraria o tratamento de status existente.
