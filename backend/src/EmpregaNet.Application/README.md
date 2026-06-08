# Camada Application — convenções

Cada **feature** (aggregate / bounded context) segue a mesma árvore:

```
{Feature}/
  Commands/           # Comandos MediatR (mutações)
    {Verb}/           # Create, Update, Delete, Close, …
    Validators.cs     # Contratos compartilhados (ex.: IJobCommand + JobDataValidator)
  Queries/            # Queries MediatR (leituras)
    Validator.cs      # Validadores de queries do módulo (um arquivo por pasta)
  ViewModel/          # DTOs de saída / leitura
  Factories/          # (opcional) montagem de entidades
  UseCase/            # (opcional) serviços de domínio da feature
```

- **Namespaces** alinhados à pasta: `EmpregaNet.Application.{Feature}.Commands`, `.Queries`, etc.
- **Companies** e **Jobs** usam `Commands` no plural (pasta e namespace).
- **Users** agrupa por perfil: `Commands/Register`, `Commands/Login`, `Commands/Admin`, `Commands/Profile`.
- **Validação**: FluentValidation registrada por assembly; comandos com validador por pasta quando há regras específicas (`Create/Validator.cs`).

Controllers na API devem espelhar o recurso e delegar ao MediatR.

## Cache HTTP (leituras)

- Respostas GET opt-in via `[OutputCache(PolicyName = ...)]`; política base `NoCache()` desabilita cache no restante da API.
- Políticas nomeadas em `EmpregaNet.Api/Configuration/OutputCacheConfig.cs` e implementações em `Configuration/OutputCache/`.
- Tags de invalidação em `Common/Cache/ApplicationCacheTags.cs`; mutações invalidam via `IOutputCacheManager`.
- Leituras autenticadas genéricas usam `AuthenticatedRead` + `Tags` no atributo; entidades do `MainController` usam `EntityRead` (tags dinâmicas por ViewModel).
- Redis opcional (`Redis:Enabled=true`) ativa store distribuído do Output Cache para scale-out.
