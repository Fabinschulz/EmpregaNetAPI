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

- Chaves e prefixos de invalidação ficam em `Common/Cache/ApplicationCacheKeys.cs` para a **Application** e a **API** usarem os mesmos literais (evita divergência entre `Users_List_` vs `Users_Admin_` e falha ao invalidar).
- O `MainController` usa `ApplicationCacheKeys.Entity.*` para `GetAll`/`GetById` (inclui `isDeleted` / `isActive` na chave quando aplicável).
- Recursos específicos: `Users` (me, admin), `Candidates`, `JobApplications` (listas por job / minhas).
- Após mutação de usuário, invalida-se também o cache de **candidato** (`Candidates_GetById_{id}`) quando os dados exibidos nessa API podem mudar.
