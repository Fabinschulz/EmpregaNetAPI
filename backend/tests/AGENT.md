# Guia de testes (EmpregaNet.Tests)

Este documento define o **padrão** para implementar testes de unidade e integração no repositório, alinhado a **Clean Code**, **SOLID** e boa legibilidade.

---

## 1. Objetivos

- Testar **regra de negócio** e **comportamento observável**, não implementação trivial (get/set, mapeamentos óbvios).
- Manter testes **rápidos, determinísticos** e **independentes** (sem ordem de execução implícita).
- Preferir **poucos mocks bem escolhidos** a “over-mocking” da stack inteira.

---

## 2. Stack e projetos

| Pacote / recurso | Uso |
|------------------|-----|
| **xUnit** | Framework de testes (`[Fact]`, `[Theory]`). |
| **FluentAssertions** | Asserções legíveis (`result.Should().BeTrue()`, `act.Should().ThrowAsync<>()`). |
| **Moq** | Substituir **portas** (interfaces) e serviços externos; evitar mockar DTOs ou value objects. |

**Referências de projeto**

- Testes que exercitam handlers/validators da aplicação: `EmpregaNet.Application` + `EmpregaNet.Domain`.
- Integração (API + DB in-memory ou Testcontainers): referenciar `EmpregaNet.Api` / `EmpregaNet.Infra` **apenas** quando necessário; documentar no próprio ficheiro de teste.

---

## 3. Organização de pastas (sugestão)

```
backend/tests/
  AGENT.MD
  Unit/
    Application/
      Users/           — handlers/validators de utilizadores
      Jobs/            — regras de emprego, etc.
    Domain/            — entidades e serviços de domínio puros (se existirem)
  Integration/
    Api/               — WebApplicationFactory, health, fluxos E2E curtos
```

Mantenha o **namespace** alinhado ao caminho: `EmpregaNet.Tests.Unit.Application.Users`.

---

## 4. Padrão AAA (Arrange, Act, Assert)

```csharp
[Fact]
public void Metodo_Condicao_ResultadoEsperado()
{
    // Arrange — preparar SUT, dados e mocks (Given)
    var sut = new MinhaValidator();
    var cmd = new MeuCommand(...);

    // Act — uma única operação sob teste (When)
    var result = sut.Validate(cmd);

    // Assert — verificar resultado ou efeitos (Then)
    result.IsValid.Should().BeTrue();
}
```

Para assíncronos, prefira **Act** explícito com FluentAssertions:

```csharp
var act = async () => await sut.Handle(cmd, CancellationToken.None);
await act.Should().ThrowAsync<ValidationAppException>();
```

---

## 5. BDD (Given / When / Then)

Opcional e recomendado para fluxos mais longos:

- Comentários `// Given`, `// When`, `// **Then**` no corpo do teste, **ou**
- Métodos auxiliares `GivenUtilizadorAtivo()` / `WhenExecutaLogin()` com nomes que leem como especificação (sem exagerar na indireção).

Evite duplicar a mesma narrativa no nome do teste **e** em três níveis de comentários — escolha um estilo por classe.

---

## 6. Nomeação dos testes

Formato obrigatório:

`Metodo_Condicao_ResultadoEsperado`

Exemplos:

- `Validate_EmailInvalido_DeveFalhar`
- `Handle_RotateAsyncRetornaNull_DeveLancarValidationAppException`
- `ConfirmEmailAsync_TokenExpirado_DeveRetornarFalha` (quando existir handler testado)

Use **português** consistente com o domínio do projeto (mensagens de validação, nomes de propriedades).

---

## 7. O que testar (prioridades)

| Prioridade | Exemplo |
|------------|---------|
| Alta | Validators FluentValidation (regras de entrada e políticas DRY como `ApplyNewPassword`). |
| Alta | Handlers com **lógica condicional** e dependências como **interfaces** (`IRefreshTokenService`, `IJwtBuilder`). |
| Média | Serviços de aplicação que orquestram repositórios mockados (contratos claros). |
| Integração | Fluxos que cruzam EF + Identity + middleware (poucos, caros de manter). |

**Evitar**

- Testar apenas construtores ou propriedades auto-implementadas.
- Mockar `DbContext` linha a linha — preferir repositório in-memory ou integração.
- Mockar `UserManager<T>` completo — custoso; preferir integração com Identity in-memory ou extrair uma porta (`IUserAccountService`) se o volume de testes justificar.

---

## 8. Moq — boas práticas

```csharp
_refresh.Setup(x => x.RotateAsync("token", It.IsAny<CancellationToken>()))
    .ReturnsAsync((User u, string t) => (u, "novo"));

_refresh.Verify(x => x.RotateAsync("token", It.IsAny<CancellationToken>()), Times.Once);
```

- Use `It.IsAny<CancellationToken>()` quando o handler não define comportamento por token.
- Verifique **interações relevantes** (`Times.Never` / `Times.Once`), não cada chamada interna.

---

## 9. Template mínimo (copiar/colar)

```csharp
namespace EmpregaNet.Tests.Unit.Application.Feature;

public sealed class MeuHandlerTests
{
    private readonly Mock<IDependencia> _dep = new();

    [Fact]
    public async Task Handle_EntradaInvalida_DeveLancarValidationAppException()
    {
        // Given
        _dep.Setup(x => x.Consultar(It.IsAny<int>())).ReturnsAsync((MeuAggregate?)null);
        var sut = new MeuHandler(_dep.Object);

        // When
        var act = async () => await sut.Handle(new MeuCommand(0), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
```

---

## 10. Execução

Na raiz do repositório:

```bash
dotnet test backend/tests/tests.csproj
```

Com cobertura (se configurado com coverlet):

```bash
dotnet test backend/tests/tests.csproj --collect:"XPlat Code Coverage"
```

---

## 11. Manutenção deste ficheiro

Ao introduzir **novo padrão** (ex.: `WebApplicationFactory`, fixtures globais), atualize esta secção em vez de espalhar READMEs por pastas.

---
