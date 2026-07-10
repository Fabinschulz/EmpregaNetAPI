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
