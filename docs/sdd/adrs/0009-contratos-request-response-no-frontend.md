# ADR 0009: Contratos do frontend nomeados por direção (Request / Response / FormValues)

## Status
Aceite

## Contexto

O frontend chamava de `Dto` tudo que atravessava a fronteira com a API. `JobDto` era resposta,
`RegisterDto` era requisição, `LoginDto` era requisição, `UserDto` era resposta. A palavra não
carregava a direção, e o resultado aparecia nos próprios nomes derivados: `JobsListResponseDto`,
`CompaniesListResponseDto`, `AdminUsersListResponseDto` — o sufixo redundante era o sintoma de que
já se sentia a falta da direção e ela vinha colada por cima.

O backend nunca usou o termo. Uma contagem de sufixos em `backend/src`:

| Sufixo | Ocorrências |
|---|---|
| `Command.cs` | 11 |
| `ViewModel.cs` | 9 |
| `Request.cs` | 2 |
| `Query.cs` | 1 |
| `Dto.cs` | 0 |

Entrada é `Command`/`Query`, saída é `ViewModel`. O vocabulário existia de um lado do fio e não do
outro.

O problema não era só de nome. A ausência da distinção produziu três defeitos concretos:

**1. Regra de entrada aplicada à saída.** `shared/schema/user-schema.ts` era usado em cinco pontos,
todos `parse(res.data)`, e exigia `username` com no mínimo 3 caracteres e e-mail válido. Mas
`UserMapper.ToViewModel` mapeia `Username = user.UserName ?? string.Empty`: string vazia é resposta
legítima. Como o schema estava dentro de `createPaginatedResponseSchema`, um único registro
incompleto derrubava a listagem inteira — não o campo, a página.

**2. Nenhuma camada de requisição.** `jobFormToApiPayload` e `companyFormToApiPayload` devolviam
objeto anônimo, sem nome, sem validação. A camada de API compensava validando o **schema do
formulário**: `createJob(dto: unknown)` chamava `jobFormSchema.parse(dto)`. Uma regra de UX era o
que guardava a fronteira de transporte, e a assinatura pública perdia o tipo.

**3. Contrato adivinhado.** Os schemas de leitura declaravam `z.union([z.string(), z.number()])`
para todo enum, "por precaução". Mas a API registra `StringEnumConverter` (`ResponseJsonConfig`):
enum sempre chega como nome. A união não protegia de nada — escondia o drift que o zod existe para
denunciar. Foi assim que `candidateId` sobreviveu: campo opcional que a API nunca enviou (ela
envia `userId`), parse passando, e a coluna "Candidato" exibindo `-` em 100% das linhas sem erro.

## Decisão

Três categorias, nomeadas pela direção, com o termo `Dto` extinto do frontend.

| Papel | Tipo | Schema | Estrito? |
|---|---|---|---|
| Sai pelo fio | `JobRequest` | `jobRequestSchema` | sim — espelha o `Command` e seu validador |
| Entra pelo fio | `JobResponse` | `jobResponseSchema` | forma sim, regra de negócio não |
| Vive no formulário | `JobFormValues` | `jobFormSchema` | sim — regra de UX, mensagens ao usuário |

**Layout de arquivos**, por feature:

```
features/<feature>/
  service/
    <x>-request-schema.ts    o que sai
    <x>-response-schema.ts   o que entra
    <x>-api.ts               transporte: parse nas duas direções
    <x>-keys.ts, <x>-queries.ts
  <componente-dono>/
    <x>-form-schema.ts       schema de UX + defaults + mappers
  domain/                    vocabulário que atravessa as três camadas
```

**Dependência em uma direção só:**

```
<x>-form-schema.ts  ──importa tipo──▶  <x>-request-schema.ts / <x>-response-schema.ts
<x>-api.ts          ──importa──────▶  <x>-request-schema.ts / <x>-response-schema.ts
```

A camada de API **nunca** importa schema de formulário. A tradução entre os dois vocabulários mora
no form-schema, em `xFormValuesFromResponse` e `xFormToRequest`, e é aplicada na mutation.

**Primitivas de campo compartilhadas** ficam em `shared/` e são compostas nos dois lados —
`shared/auth/password-schema.ts` exporta `newPasswordSchema` (espelho de
`PasswordPolicyRules.ApplyNewPassword`) e `existingPasswordSchema`, usados tanto pelos schemas de
requisição quanto pelos de formulário de cadastro, troca e redefinição.

**Rigor por categoria:**

- Requisição: espelha o validador do servidor. Mensagens descrevem quebra de contrato, não erro de
  preenchimento — quando disparam, o usuário já passou pelo formulário e o defeito é do mapeamento.
- Resposta: exige presença do que o ViewModel garante (`required`, tipos-valor), e nada de regra de
  negócio. Datas de auditoria seguem permissivas: `BaseViewModel` devolve pt-BR com string vazia,
  `UserViewModel` devolve ISO com `null`, e `formatDate` absorve as duas.
- Formulário: regra de UX e mensagem em pt-BR para o usuário.

## Consequências

O contrato apertado transformou drift silencioso em erro de compilação e em teste vermelho, que é o
ponto — mas cobra o preço na hora da migração:

- `candidateId` virou erro de tipo em `recrutamento/vagas/candidates`, e o defeito foi corrigido dos
  dois lados: a API passou a resolver o candidato do lado da leitura, via
  `JobApplicationProjection`, e o contrato do cliente declara `candidate` **obrigatório**. Ver a
  seção "Candidato na resposta de candidatura" abaixo.
- Payloads de teste que descreviam respostas impossíveis (enum como índice, vaga sem `workShift`)
  falharam. Foram corrigidos para o que a API sabe produzir, e os cenários que existiam para
  documentar a tolerância viraram cenários que documentam a recusa.
- O cadastro passou a exigir senha forte no cliente. `registerFormSchema` pedia só 8 caracteres,
  mas `RegisterUserCommandValidator` aplica `ApplyNewPassword` como os demais fluxos: a senha fraca
  era aceita na tela e recusada pelo 400.
- `updateAdminUser` enviava `userType: null` quando o select estava vazio, contra um
  `UpdateAdminUserCommand(string UserType)` não-anulável. O schema de requisição passa a barrar.

Arquivos que mudavam por cinco motivos diferentes deixaram de existir: `jobs-feed-schema.ts` (333
linhas: resposta + estado de filtro + codec de URL + contagem + params de API) virou três módulos,
e os quatro `<x>-schema.ts` de feature viraram request + response + form + filtro.

## Candidato na resposta de candidatura

O `candidateId` inexistente era sintoma de um problema de modelagem, não de nomenclatura:
`JobApplication` é raiz de agregado e referencia o candidato apenas por `UserId`. Navegar da
entidade até `User` fecharia o buraco na tela e abriria outro, atravessando a fronteira do agregado.

A saída foi a que o próprio repositório já usava para os dados de empresa no feed: uma **projeção de
leitura**. `JobApplicationProjection` resolve a candidatura junto do candidato num único `SELECT`, o
agregado continua fechado, e `JobApplicationMapper` passou a mapear **só a partir da projeção** —
não existe mais um mapeador de entidade, então não há como produzir um ViewModel sem o nome.

Duas decisões dentro disso:

- **`LEFT JOIN`, não `INNER`.** Com `INNER`, uma linha órfã faria a candidatura sumir da listagem
  sem erro nenhum — a mesma classe de falha que se está corrigindo, só que pior. Com `LEFT`, ela
  aparece com nome vazio, que é visível.
- **Candidato excluído continua sendo devolvido**, marcado com `isDeleted`: o histórico do processo
  seletivo precisa dele, e como sinalizar isso é decisão da tela.

Todas as rotas passaram pela projeção, inclusive a resposta da troca de status — que relê a
candidatura após a escrita em vez de mapear a entidade. Um campo presente em algumas rotas e ausente
em outras seria o defeito original de volta, em escala menor.

Não houve mudança de esquema: nenhuma migração envolvida.

Relacionado: [ADR 0008](0008-formato-de-erro-da-api.md), que trata do outro lado da mesma fronteira.
