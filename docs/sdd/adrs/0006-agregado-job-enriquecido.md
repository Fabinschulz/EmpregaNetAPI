# ADR 0006: Agregado `Job` enriquecido e separação entre vínculo e modalidade

## Contexto

O agregado `Job` tinha sete campos: `CompanyId`, `Title`, `Description`, `Salary`, `JobType`,
`PublishedAt`, `IsActive`. Nenhum conceito de localização, modalidade de trabalho, senioridade,
área profissional, faixa salarial, tecnologias ou benefícios.

Duas consequências práticas apareceram ao especificar o feed de vagas:

1. **Os filtros pedidos não têm o que filtrar.** Localização, modalidade, senioridade, área,
   tecnologias e benefícios — a maior parte do que um candidato usa para triar — simplesmente não
   existem no modelo.
2. **`JobTypeEnum` acumulava dois conceitos.** Sete membros descrevem vínculo ou jornada
   (`FullTime`, `PartTime`, `Internship`, `Freelancer`, `Temporary`, `Trainee`, `Volunteer`) e um
   descreve **modalidade de trabalho** (`Remote`). Como é um enum de escolha única, uma vaga não
   conseguia ser "CLT" e "remota" ao mesmo tempo: escolher `Remote` apagava o vínculo, e escolher o
   vínculo apagava a informação de que era remota. Faltavam ainda `CLT` e `PJ`, que no mercado
   brasileiro são a primeira pergunta do candidato.

Havia três caminhos:

1. **Derivar da empresa.** Usar `Company.Address` como localização da vaga. Barato, mas errado: a
   vaga pode ser em cidade diferente da sede, e o filtro geográfico passaria a exigir join em toda
   consulta do feed, sem coluna indexável em `Jobs`.
2. **Tabelas de catálogo.** Modelar tecnologias e benefícios como entidades com tabelas de junção.
   Correto em teoria, mas hoje são conjuntos fechados de strings sem gestão pelo utilizador — duas
   entidades, dois repositórios, seeds e dois joins para nada.
3. **Enriquecer o agregado.** Levar para `Job` os atributos que são propriedades da vaga.

## Decisão

- **`Job` recebe:** `Summary`, `SalaryMin`, `SalaryMax`, `SalaryDisclosed`, `WorkModel`,
  `Seniority`, `Area`, `Location` (value object) e as coleções `Technologies` e `Benefits`.
- **`Salary` vira `SalaryMin`**, com `SalaryMax` opcional e `SalaryDisclosed` para "a combinar".
  Valor único não expressa faixa, e faixa é o que o mercado publica.
- **`JobTypeEnum` passa a descrever só vínculo/jornada.** `Remote` sai para o novo `WorkModelEnum`
  (`OnSite`, `Hybrid`, `Remote`); entram `Clt` e `Pj`.
- **O índice 8 do enum fica reservado, não reutilizado.** Era `Remote`. Uma linha que escape ao
  data-move — réplica, backup restaurado, ambiente esquecido — passaria a significar `Clt` em
  silêncio. Um buraco no enum não custa nada.
- **`Location` é owned type na tabela `Jobs`**, não derivado da empresa. A migration faz backfill a
  partir do endereço da empresa para que nenhuma vaga existente saia dos filtros geográficos.
- **`Technologies` e `Benefits` são `text[]` nativo do Postgres** com índice GIN, e o vocabulário é
  uma lista curada em constante na Application. Sem tabela de catálogo enquanto não houver
  necessidade real de gestão.

## Consequências

**Positivas:**
- Os filtros do feed passam a ter colunas reais e indexadas por trás. Nenhum filtro é encenação.
- Uma vaga pode finalmente ser "CLT" **e** "remota" — o modelo deixa de forçar uma escolha falsa
  entre dois eixos independentes.
- Sobreposição de arrays (`&&`) com GIN resolve "vagas com React ou .NET" sem join.
- A faixa salarial permite ordenação e filtro por interseção de intervalos.

**Negativas / obrigações futuras:**
- **A migration move dados em produção** (`Salary` → `SalaryMin`, `JobType = Remote` → `WorkModel`,
  backfill de cidade/UF). Exige revisão humana antes de aplicar.
- **O contrato de escrita quebra.** `CreateJobCommand`/`UpdateJobCommand` trocam `Salary` pelos três
  campos novos. O consumidor é único e interno (o formulário de recrutamento), atualizado junto.
- **O formulário de publicação passa a ser parte obrigatória de qualquer entrega que dependa dos
  campos novos.** Campo que ninguém preenche é filtro que não filtra.
- Vagas anteriores ficam sem área, senioridade, tecnologias e benefícios. Os campos são opcionais e
  a vaga continua listável, mas some dos filtros correspondentes até ser editada.
- Promover `Technologies`/`Benefits` a tabela no futuro será uma migration de normalização, não uma
  troca de tipo.

## Referências

- `backend/src/EmpregaNet.Domain/Entities/Job.cs`, `JobLocation.cs`
- `backend/src/EmpregaNet.Domain/Enums/JobType.cs`, `WorkModel.cs`, `Seniority.cs`, `JobArea.cs`
- `backend/src/EmpregaNet.Infra/Persistence/Repositories/Job/JobConfiguration.cs`
- [`docs/features/emp-feed-vagas/design.md`](../../features/emp-feed-vagas/design.md)
- [ADR 0007](0007-endpoint-de-feed-dedicado.md) — endpoint que consome estes campos
