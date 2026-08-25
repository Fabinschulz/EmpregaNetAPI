# ADR 0010: Analytics em endpoints por seção, com escopo resolvido no servidor

## Contexto

O painel de métricas precisa responder a perguntas de naturezas diferentes sobre o mesmo recorte:
indicadores acumulados e do período, séries temporais, funil, distribuições categóricas, ranking de
vagas e leituras derivadas. Três pressões apareceram ao desenhar a superfície HTTP:

1. **Granularidade da resposta.** Um endpoint por métrica produziria dezenas de rotas quase idênticas
   e a tela faria dezenas de requisições. Um endpoint único devolveria tudo numa resposta cujo tempo
   é o da consulta mais lenta, e cuja falha apaga a tela inteira — inclusive as partes que tinham
   dados.

2. **Quem decide de que empresa são os números.** O painel serve `Admin`, `Recruiter` e `Manager`.
   Os dois últimos só podem ver a empresa a que estão vinculados (`User.EmployerCompanyId`). Se o
   `companyId` da query fosse a instrução, bastaria trocar um número na URL para ler a operação de
   outra empresa.

3. **O que fazer com o que o domínio não mede.** O produto pede visualizações de vaga, contratações e
   vagas próximas do vencimento. Nada disso existe: não há contador de exibição, não há status de
   contratação (`Finished` não distingue contratado de encerrado) e a vaga não tem data de expiração.

## Decisão

- **Cinco endpoints, um por seção da tela**, sob `GET /api/dashboard/`: `overview`, `trends`,
  `distribution`, `jobs`, `insights`. O corte é por seção, não por métrica: cada resposta traz o
  conjunto coerente daquela seção, com o mesmo `meta`.

- **Escopo resolvido no servidor.** `IDashboardScopeAccess` traduz o papel do utilizador autenticado
  num `DashboardScope`; `companyId` na query é **pedido, não instrução**. Não-administrador que pede
  outra empresa é **recusado**, não silenciosamente reduzido ao próprio escopo — devolver dados
  válidos para a pergunta errada é pior que um erro. Empresa inexistente também falha, em vez de
  produzir um painel de zeros indistinguível de empresa sem movimento.

- **Métricas de plataforma não descem ao escopo de empresa.** Total de utilizadores e de empresas não
  são calculados nem devolvidos quando o escopo é uma empresa.

- **Indicadores como lista de descritores**, não campos fixos: cada item traz `key`, `label`,
  `value`, `unit`, `previousValue`, `changePercent`, `trend`, `hint` e `isPeriodScoped`. O conjunto
  muda com o escopo, e com campos fixos a tela precisaria de um `if` por indicador para distinguir
  "zero" de "não se aplica ao seu perfil".

- **Lacunas de domínio viajam na resposta**, em `meta.unavailable`, com métrica, rótulo e motivo. A
  tela mostra a lacuna como nota; nunca zero.

- **Intervalo semiaberto em UTC**, `[from, toExclusive)`, com as fronteiras calculadas no fuso de
  Brasília uma única vez, na Application. O par "00:00:00 até 23:59:59" perde o último segundo do dia
  e obriga cada consulta a repetir a mesma gambiarra.

- **Repositório de leitura separado** (`IDashboardAnalyticsRepository`), que não herda
  `IBaseRepository<T>` e nunca devolve entidade: só escalares e linhas de `GROUP BY`.

- **Cache por tempo, não por tag.** Política `DashboardRead`, TTL de 300 s configurável, vary por
  utilizador e query. Qualquer candidatura ou vaga nova altera algum número do painel: invalidar por
  evento esvaziaria o cache continuamente. `meta.generatedAt` expõe a idade real do dado.

## Consequências

**Positivas:**
- Os cartões do topo aparecem sem esperar as agregações mais largas; cada seção falha, recarrega e é
  cacheada por conta própria.
- A regra de visibilidade vive num único caso de uso, auditável, com teste de integração sobre o
  Identity real. Nenhum handler reescreve o predicado de escopo.
- O contrato é auditável de ponta a ponta: cada número tem `hint` dizendo o que conta, e o que não é
  medido está declarado em vez de aparecer como zero.
- Nenhuma migration foi necessária: os índices existentes do feed e das candidaturas sustentam as
  agregações.

**Negativas / obrigações futuras:**
- São **cinco requisições** por carga do painel. Aceitável: correm em paralelo, cada uma é cacheada e
  a mais crítica é a mais leve. Uma sexta seção deve nascer como sexto endpoint, não inflar um
  existente.
- `overview` faz nove agregações. Cabe no TTL de cache atual; se a base crescer, medir antes de
  acrescentar índice em `JobApplications(AppliedAt)`.
- Três números são **aproximações declaradas**: vagas encerradas (usa `UpdatedAt`, que qualquer edição
  move), contas inativas (usa e-mail não confirmado) e a etapa de aprovação do funil (soma `Approved`
  e `Finished`, porque o status não tem histórico). Sair da aproximação exige `Job.ClosedAt`,
  `User.LastLoginAt` e histórico de transições da candidatura.
- O agrupamento por dia local assume **um deslocamento por janela**, válido enquanto o Brasil não
  observar horário de verão. Se voltar, os dias de transição atribuem uma hora ao balde vizinho —
  erro de fronteira, não de total.

## Referências

- `backend/src/EmpregaNet.Api/Controllers/Dashboard/DashboardController.cs`
- `backend/src/EmpregaNet.Application/Dashboard/UseCase/DashboardScopeAccess.cs`
- `backend/src/EmpregaNet.Application/Dashboard/UseCase/DashboardDomainGaps.cs`
- `backend/src/EmpregaNet.Infra/Persistence/Repositories/Dashboard/DashboardAnalyticsRepository.cs`
- [`docs/features/emp-dashboard-analytics/design.md`](../../features/emp-dashboard-analytics/design.md)
- [ADR 0007](0007-endpoint-de-feed-dedicado.md) — mesma decisão de não contaminar o contrato genérico
- [ADR 0008](0008-formato-de-erro-da-api.md) — formato de erro que estas rotas devolvem
