# ADR 0005: `User` e `Role` herdam de ASP.NET Core Identity dentro do Domain

## Status
Aceite

## Contexto

`EmpregaNet.Domain` declara `User : IdentityUser<long>` e `Role : IdentityRole<long>`, o que obriga
o projeto a referenciar `Microsoft.AspNetCore.Identity.EntityFrameworkCore`. Isso contraria a regra
escrita em `CLAUDE.md` — *"Domain não referencia EF nem ASP.NET"* — e a divergência ficou por muito
tempo entre a regra declarada e o código real, sem nenhum registro de qual das duas estava certa.

Uma auditoria da documentação levantou a questão. As alternativas eram duas:

1. **Refatorar**: entidades de domínio puras (`User`, `Role` sem herança), com entidades de Identity
   separadas na Infra e projeção entre as duas.
2. **Aceitar e registrar**: manter a herança e corrigir a regra, que é que estava desatualizada.

O custo real da opção 1 neste código: `UserManager<User>`, `SignInManager<User>` e
`RoleManager<Role>` atravessam `JwtBuilder`, todos os handlers de autenticação, o
`IdentityDataSeeder`, o `CandidateRoleAssignment`, o `UserTypeRoleSync`, as configurações Fluent API
e a fixture `InMemoryIdentityFixture` usada por toda a suíte de integração. A projeção teria de ser
bidirecional (o Identity precisa da entidade concreta para persistir) e passaria a existir em cada
um desses pontos.

O benefício seria teórico: trocar o provedor de identidade. Não há indício de que isso vá acontecer,
e o Identity é justamente a peça que fornece hash de senha, lockout por conta (ADR 0002) e tokens de
confirmação/reset — funcionalidade que o projeto não pretende reimplementar.

## Decisão

- `User` e `Role` **continuam** herdando de `IdentityUser<long>` / `IdentityRole<long>` no Domain, e
  `EmpregaNet.Domain` **mantém** a referência a `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- A regra em `CLAUDE.md` passa a declarar a exceção explicitamente, em vez de afirmar algo que o
  código contradiz.
- A exceção é **restrita a Identity**. Continua proibido no Domain: `Microsoft.EntityFrameworkCore`
  (`DbContext`, `DbSet`, migrations), `Microsoft.AspNetCore.Mvc` e qualquer tipo de HTTP
  (`HttpContext`, `ControllerBase`, `IActionResult`).

## Consequências

**Positivas:**
- A regra escrita volta a descrever o código. Quem chega para de encontrar uma contradição logo no
  primeiro arquivo de contexto.
- Nenhuma camada de projeção para manter em ~8 pontos do código.
- O limite fica mais útil por ser mais preciso: uma referência a EF ou MVC no Domain continua sendo
  motivo de rejeição em revisão, e agora sem ambiguidade sobre o caso do Identity.

**Negativas / obrigações futuras:**
- Trocar o provedor de identidade passa a ser um refactor grande, não uma troca de implementação.
  É o custo aceito conscientemente.
- Lógica de negócio de usuário deve continuar em `Application`, não em `User`. A entidade herda de
  um tipo de framework; enchê-la de regra de domínio ampliaria o acoplamento em vez de contê-lo.
- Ao adicionar campo novo em `User`, prefira propriedades simples e mapeamento por Fluent API na
  Infra — nada de atributos de EF no Domain.

## Referências

- `backend/src/EmpregaNet.Domain/Entities/User.cs`, `Role.cs`
- `backend/src/EmpregaNet.Domain/EmpregaNet.Domain.csproj`
- `backend/src/EmpregaNet.Infra/Extensions/IdentityExtensions.cs`
- [ADR 0002](0002-rate-limit-unico-por-ip.md) — lockout do Identity como proteção de brute-force
