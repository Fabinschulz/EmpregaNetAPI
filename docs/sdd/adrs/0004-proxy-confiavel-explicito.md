# ADR 0004: Headers de proxy reverso só de proxies explicitamente confiáveis

## Status
Aceite

## Contexto
Em produção, a API deverá rodar atrás de um proxy reverso/load balancer, que reescreve a conexão TCP para o IP do próprio proxy e comunica o cliente real via headers `X-Forwarded-For`/`X-Forwarded-Proto`. Sem tratamento algum desses headers, dois problemas surgem simultaneamente:

1. O rate limiter por IP (ADR 0002) passa a ver o IP do proxy para todas as requisições — todos os usuários caem no mesmo balde, e o sistema se auto-nega-serviço.
2. Com TLS terminado no proxy, o app enxerga tudo como HTTP puro; `UseHttpsRedirection`/HSTS podem entrar em loop de redirecionamento.

A saída ingênua e comum é configurar `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`. Essa variável de ambiente, no entanto, **limpa as listas de proxies confiáveis** e faz o middleware aceitar `X-Forwarded-For` de **qualquer origem** — reabrindo exatamente o vetor que o hardening (ADR 0002) tentou fechar: um atacante externo forja o header e escapa do rate limit, ou envenena logs/auditoria com um IP falso. Esse comportamento (e o breaking change de segurança introduzido em ASP.NET Core 8.0.17/9.0.6, que passou a ignorar por padrão headers de origem não configurada) está documentado pela Microsoft: https://learn.microsoft.com/aspnet/core/breaking-changes/8/forwarded-headers-unknown-proxies

## Decisão
- `ForwardedHeadersConfig.UseForwardedHeadersIfConfigured()` só registra o middleware `UseForwardedHeaders` quando a seção `ForwardedHeaders:KnownProxies` ou `KnownNetworks` do appsettings estiver preenchida.
- Sem configuração (padrão atual, execução direta/localhost): o middleware **não é registrado** — todo header `X-Forwarded-*` é ignorado, IP e scheme vêm direto da conexão TCP.
- Com configuração (a preencher no deploy): confia **apenas** nos IPs/redes explicitamente listados; qualquer proxy fora da lista tem seus headers ignorados.
- O middleware é o primeiro do pipeline (`SetupApiServices`), antes de HSTS, redirect HTTPS e do rate limiter — para que esses componentes já vejam IP/scheme restaurados corretamente.

## Consequências

**Positivas:**
- Seguro por padrão: em qualquer ambiente sem a seção configurada, não há superfície de spoofing via `X-Forwarded-For`.
- No deploy, basta preencher `KnownProxies`/`KnownNetworks` com a topologia real (IP fixo do proxy ou CIDR da rede interna) para o rate limit e o redirect HTTPS voltarem a funcionar corretamente atrás do proxy.

**Negativas / obrigações futuras:**
- No dia do deploy atrás de proxy/load balancer, é **obrigatório** preencher `ForwardedHeaders:KnownProxies` ou `KnownNetworks` no appsettings de produção — sem isso, o rate limit por IP colapsa (todos os usuários no mesmo balde) e pode haver loop de redirect HTTPS.
- **Proibido** usar `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` como atalho — essa variável limpa as listas de confiança e reabre o spoofing de IP que esta decisão existe para prevenir. A única forma aceita de habilitar é preencher as listas explícitas.
- Em ambientes de nuvem onde o IP do proxy muda com o tempo (ex.: autoscaling), `KnownNetworks` (CIDR da VPC/subnet) é preferível a `KnownProxies` (IP fixo).

## Checklist para o deploy com ALB (AWS)

Topologia planejada: **ALB (TLS/ACM) → EC2:80 → container**. Estes itens são interdependentes — aplicar um sem os outros troca um problema por outro.

| # | Item | Por que é obrigatório |
| - | ---- | --------------------- |
| 1 | `ForwardedHeaders:KnownNetworks` com o **CIDR das subnets do ALB** (não `KnownProxies`) | Os nós do ALB têm IP dinâmico dentro das subnets; uma lista de IPs fixos quebraria sem aviso. Sem isso, o rate limiter vê o IP do ALB para todos e o sistema se auto-nega-serviço. |
| 2 | Health check do target group apontando para **`/health`**, esperando **200** | O ALB remove do balanceamento qualquer alvo que não responda 200 — com uma instância só, isso é indisponibilidade total. `/health` está isento de rate limit (`RateLimiting:ExemptPaths`) justamente para nunca receber 429. |
| 3 | Certificado no **ACM** com TLS terminando no ALB | O cookie de autenticação é emitido com `Secure = true` fora de Development (`AuthCookieService`). Cookie `Secure` não trafega por HTTP: sem TLS, o navegador descarta o cookie e o login "funciona" mas a sessão nunca persiste — sem erro visível no log da API. |
| 4 | `RateLimiting:BypassIps` com o IP de saída do **servidor Next.js** | O SSR do catálogo de vagas chama a API a partir do servidor, não do navegador de cada visitante: todo esse tráfego chega de um IP só e esgotaria o balde sozinho. Note que, com o item 1 aplicado, o IP relevante é o restaurado pelo `ForwardedHeaders`. |
| 5 | Redirecionar HTTP→HTTPS **no listener do ALB** | O `UseHttpsRedirection` da aplicação não substitui isso: sem porta HTTPS conhecida ele apenas registra `Failed to determine the https port for redirect` e não redireciona. A borda é o lugar certo para esse redirect. |

**Nota sobre o health check (bug já corrigido):** `DatabaseCheck` verificava `Database.GetDbConnection().State == Open`. Esse método não abre conexão, e o EF Core devolve a conexão ao pool após cada operação — o estado é `Closed` quase sempre. O resultado era `/health` respondendo **503 com o banco saudável**, algo inofensivo enquanto ninguém consultava a rota, mas que causaria indisponibilidade total assim que o ALB passasse a usá-la como critério. Trocado por `CanConnectAsync`. O `RedisHealthCheck` também tratava latência zero como "indisponível" (lógica invertida: zero é resposta rápida, não ausente) — corrigido.

**Consequência para o rate limit:** com o ALB, a abordagem de borda (AWS WAF) passa a ser viável e o limitador da aplicação deixa de ser a única linha de defesa. Isso não obriga a mudar nada — ver ADR 0002 — mas é o gatilho para reconsiderar, caso o volume justifique.
