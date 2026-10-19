# ADR 0019: Preparação da API e do frontend para várias réplicas (ECS Fargate)

## Status

Aceite. Concretiza a Fase 3 do plano do [ADR 0018](0018-infra-ecs-fargate-com-auto-scaling.md). Nada foi aplicado na
AWS: o que depende de ALB, RDS ou segredos reais está em "Pendências".

## Contexto

Com várias tarefas Fargate atrás de um ALB, três premissas de processo único deixam de valer: o que cada processo
guarda em memória, quem é o cliente que chegou pelo proxy e quem roda migrações e seed no arranque. Cada ponto abaixo
foi provado antes de ser corrigido (testes contra PostgreSQL real e processos reais da API, ver o relatório da fase).

## Decisões

### 1. Data Protection no PostgreSQL, cifrado por certificado

Sem chaves compartilhadas, o token de confirmação de e-mail e o de redefinição de senha (Identity) emitidos por uma
réplica não valem em outra nem após reinício. O key ring passa a viver na tabela `DataProtectionKeys`
(`PersistKeysToDbContext`, migration só aditiva), com `ApplicationName` fixo (`DataProtection:ApplicationName`,
padrão `EmpregaNet`). Em Production a API **recusa subir sem certificado** (`DataProtection:CertificatePfxBase64` e
`...Password`); fora de Production as chaves ficam em texto no banco e a API avisa. Rotação: o certificado anterior
entra em `PreviousCertificatePfxBase64` para ainda abrir chaves antigas. O primeiro key ring é criado sob o bloqueio
de inicialização (item 3), para duas réplicas não criarem dois anéis.

### 2. Proxy e rate limit

- `ForwardedHeaders` só confia em `KnownProxies`/`KnownNetworks` explícitos, com `ForwardLimit = 1`; rede mais ampla que
  /16 (IPv4) ou /48 (IPv6) é recusada na inicialização. Sem IP fixo de tarefa: a rede confiável é a CIDR das subnets do
  ALB. Sem configuração, cabeçalhos `X-Forwarded-*` são ignorados (cabeçalho forjado não muda o IP) e a API avisa fora
  de Development. Continua valendo o [ADR 0004](0004-proxy-confiavel-explicito.md).
- `BypassIps` aceita só IP exato e serve para endereço estável. O SSR deixou de depender dele: o Next envia
  `X-Internal-Caller` (valor de `INTERNAL_API_SECRET`), a API compara em tempo constante (mínimo 32 caracteres) e o
  chamador interno usa balde próprio e limitado (padrão 10× o individual). Sem o segredo configurado, o SSR cai no
  limite por IP como qualquer cliente.

### 3. Inicialização coordenada (migrações, seed, chaves)

Antes de atender, cada réplica espera o banco (backoff 1 s até o teto de 10 s, total 180 s; erro permanente como
senha ou banco inexistente falha na hora), toma um `pg_try_advisory_lock` de sessão em conexão fora do pool e executa em
ordem: migrações, seed, chaves do Data Protection e conta de capacidade. O bloqueio é do servidor: réplica que morre o
perde. Provado com banco real: sem ele, migrações concorrentes falham mesmo com o bloqueio do EF; com ele, quatro
réplicas simultâneas terminam sem erro e sem duplicidade. Em produção a recomendação é separar:
`Startup__RunMigrations=false` nas réplicas (elas **se recusam a subir** com migração pendente) e uma tarefa única
`dotnet EmpregaNet.Api.dll --migrate-only` antes de cada deploy.

### 4. Pool de conexões

`Database:MaxPoolSize` (20), `MinPoolSize`, `ApplicationName`, `ExpectedMaxReplicas` (2) e `ReservedConnections` (10)
entram na connection string, sem sobrescrever valor explícito dela. A inicialização lê `max_connections` e avisa se
`MaxPoolSize × ExpectedMaxReplicas + ReservedConnections` não couber. Com a réplica a mais durante um deploy contínuo,
usar `ExpectedMaxReplicas` = máximo do Auto Scaling + 1.

### 5. Frontend em contêiner

`frontend/Dockerfile` multi-stage (Node 22 Alpine, pnpm pelo `packageManager`, saída `standalone`, usuário `node`),
`HEALTHCHECK` em `/healthz` (sem chamar a API e fora do guard do `proxy.ts`). O navegador usa `/api` no mesmo domínio
(`NEXT_PUBLIC_API_BASE_URL` vazio no build) e o SSR lê `API_INTERNAL_BASE_URL` a cada requisição. **Os rewrites de `/api`
do Next são gravados no build** (provado: o valor em tempo de execução não os altera), então no ECS quem roteia `/api/*`
para a API é o ALB; `API_INTERNAL_BASE_URL` no build só serve para quem quiser o rewrite dentro do contêiner.
`.dockerignore` impede `.env` local de entrar no bundle e `scripts/verify-container.sh`, rodado na CI, falha se houver
URL de desenvolvimento, processo root ou `/healthz` sem 200.

## Consequências

- Com chaves, bloqueio e migrações separadas, escalar de 1 para N réplicas não quebra token, não corrompe schema e
  não duplica seed. Custo: uma tabela, um certificado a guardar e um passo de migração no deploy.
- Sem Redis (ADR 0018), **estes estados continuam por réplica** e multiplicam por N: o limite de requisições por IP, o
  teto diário de e-mails por destinatário, o cache de saída da API e o `'use cache'` do Next. Aceito por ora.
- Cache de saída: a invalidação por tag só alcança a réplica que atendeu a escrita; as outras servem o catálogo público
  antigo até a validade acabar. Requisição autenticada nunca entra nesse cache. A API avisa na subida quando há mais de
  uma réplica prevista, sem Redis e `OutputCache:DefaultExpirationMinutes` > 5; usar no máximo 5. O mesmo vale para
  `revalidateJobCache` (Next): invalida só a tarefa que executou a ação.
- Rollback de release com migração `rename`/`drop` continua sendo para frente: não há como voltar a imagem.

## Pendências (dependem da AWS ou do responsável)

- Certificado do Data Protection no Secrets Manager e injeção como segredo; `INTERNAL_API_SECRET` (mesmo valor no
  frontend e na API); GitHub Environment `Production` e papel OIDC ainda vazios.
- CIDR das subnets do ALB em `ForwardedHeaders__KnownNetworks` (só existe depois do Terraform de rede).
- `max_connections` real do RDS escolhido, para fechar a conta do pool.
- Health check do ECS em `/health/live` com `startPeriod` maior que a espera do banco; ALB roteando `/api/*`.
- Tarefa de migração (`--migrate-only`) no pipeline de deploy.
- Validar login, cookie `Secure` e `proxy.ts` atrás do ALB real (tarefa 3.6) e o encerramento limpo (3.7).

## Alternativas descartadas

- **Redis** para cache, limite e e-mail: custo e serviço novo sem medição que o justifique (ADR 0018).
- **Confiar em `0.0.0.0/0` ou em /8 no proxy** para não depender de CIDR: abre falsificação de IP.
- **Deixar o EF migrar sozinho em cada réplica**: provado insuficiente com banco real.
- **Chaves do Data Protection em EFS ou S3**: serviço a mais; o banco já é compartilhado e tem backup.
