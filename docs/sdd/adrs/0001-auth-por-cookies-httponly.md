# ADR 0001: Autenticação via cookies httpOnly, sem token acessível por JavaScript

## Status
Aceite

## Contexto
O fluxo original guardava o access token e o refresh token no cliente (sessionStorage e/ou cookies legíveis por JS), para permitir hidratação da sessão sem depender do backend a cada carregamento de página.

Essa abordagem expõe as duas credenciais a qualquer script que rode na página — inclusive via XSS numa dependência de terceiros, num componente de rich text, ou numa lib de analytics comprometida. Quem rouba o token rouba a sessão inteira, sem precisar de mais nada.

O backend já emite os tokens via `AuthCookieService` e o middleware JWT (`OnMessageReceived`) já sabe ler o access token tanto do header `Authorization` quanto de um cookie httpOnly — a infraestrutura para eliminar o token do lado do cliente já existia, só não era usada.

## Decisão
- O **access token** e o **refresh token** passam a viver exclusivamente em cookies `HttpOnly`, `Secure` (fora de Development) e `SameSite=Lax`, emitidos pelo backend (`AuthCookieService.AppendLoginCookies`).
- O frontend nunca lê nem guarda o valor do token. `axios` autentica via `withCredentials: true`; o cookie viaja sozinho.
- Para hidratar a UI sem round-trip ao servidor a cada carregamento, o cliente guarda apenas **metadados de exibição não sensíveis** (`roles`, `username`, `email`) em `localStorage`, lidos via `useSyncExternalStore`. Esses metadados nunca autorizam nada sozinhos — a decisão de autorização real é sempre do servidor, a partir do cookie.
- Logout revoga o refresh token no servidor (`POST /api/users/logout`) e limpa os cookies.

## Consequências

**Positivas:**
- Um XSS que consiga rodar JS na página **não consegue mais roubar a sessão**: não há token para ler. Na pior hipótese, lê roles/nome/e-mail — dados de exibição, sem poder de autorização.
- Hidratação da sessão continua instantânea e sem custo de requisição (decisão explícita para não gerar carga desnecessária no servidor a cada load).
- Logout entre abas é sincronizado automaticamente (evento `storage`).

**Negativas / obrigações futuras:**
- Em produção, se a API e o frontend ficarem em subdomínios distintos, o cookie precisa de um `Domain` compartilhado explícito — sem isso o cookie não é enviado entre eles e o login "não funciona" silenciosamente.
- CORS precisa manter `AllowCredentials()` com origens explícitas (nunca wildcard) — é o que permite o cookie viajar cross-origin com segurança.
- **Proibido a partir de agora**: reintroduzir o token (access ou refresh) em `localStorage`, `sessionStorage` ou cookie não-`HttpOnly`, mesmo que pareça resolver um problema de UX pontual.
