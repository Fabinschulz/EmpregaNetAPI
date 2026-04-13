using EmpregaNet.Tests.Support;

namespace EmpregaNet.Tests.Integration;

/// <summary>
/// Uma instância partilhada de <see cref="InMemoryIdentityFixture"/> por coleção e sem paralelismo
/// entre testes de integração (evita condições de corrida no mesmo ServiceProvider / InMemory DB).
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<InMemoryIdentityFixture>
{
}
