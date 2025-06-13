using Mapper.Interfaces;

namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Implementação padrão do mapeador de objetos utilizando a <see cref="IConfigurationProvider"/>.
/// </summary>
public class MapperObj : IMapper
{
    private readonly IConfigurationProvider _configuration;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="MapperObj"/>.
    /// </summary>
    /// <param name="configuration">Configuração de mapeamento utilizada.</param>
    public MapperObj(IConfigurationProvider configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Realiza o mapeamento de uma instância de <typeparamref name="TSource"/> para <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="source">Instância de origem.</param>
    /// <returns>Instância mapeada de <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="InvalidOperationException">Se não houver mapeamento configurado.</exception>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        // Obtém função de mapeamento a partir da configuração
        var mapFunc = _configuration.GetMapping(typeof(TSource), typeof(TDestination));

        if (mapFunc == null)
            throw new InvalidOperationException($"Mapping not found: {typeof(TSource)} → {typeof(TDestination)}");

        // Faz o casting seguro para a função esperada
        var func = (Func<TSource, TDestination>)mapFunc;

        // Executa a função de mapeamento
        return func(source);
    }

    /// <summary>
    /// Realiza o mapeamento de uma lista de instâncias de <typeparamref name="TSource"/> para <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="sourceList">Lista de instâncias de origem.</param>
    /// <returns>Enumerável de instâncias mapeadas de <typeparamref name="TDestination"/>.</returns>
    public IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList)
    {
        // Itera e mapeia cada elemento utilizando o método Map
        foreach (var item in sourceList)
        {
            yield return Map<TSource, TDestination>(item);
        }
    }
}
