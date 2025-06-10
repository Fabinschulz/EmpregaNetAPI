using EmpregaNet.Domain.Components.Mapper.Interfaces;

namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Extensões para facilitar a projeção de consultas <see cref="IQueryable{T}"/> utilizando o <see cref="IConfigurationProvider"/>.
/// </summary>
public static class QueryableExtensions
{

    /// <summary>
    /// Projeta uma lista enumerável de <typeparamref name="TSource"/> para uma lista de <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="source">Lista de origem.</param>
    /// <param name="configuration">Configuração de mapeamento.</param>
    /// <returns>Lista de <typeparamref name="TDestination"/>.</returns>
    public static List<TDestination> ProjectToList<TSource, TDestination>(
        this IEnumerable<TSource> source,
        IConfigurationProvider configuration)
    {
        return source.AsQueryable()
                     .ProjectTo<TSource, TDestination>(configuration)
                     .ToList();
    }

    /// <summary>
    /// Projeta os elementos de <typeparamref name="TSource"/> para <typeparamref name="TDestination"/> utilizando o mapeamento configurado.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="source">Consulta de origem.</param>
    /// <param name="config">Configuração de mapeamento.</param>
    /// <returns>Consulta projetada para <typeparamref name="TDestination"/>.</returns>
    /// <exception cref="InvalidOperationException">Se não houver mapeamento registrado.</exception>
    private static IQueryable<TDestination> ProjectTo<TSource, TDestination>(
        this IQueryable<TSource> source,
        IConfigurationProvider config)
    {
        var mapFunc = config.GetMapping(typeof(TSource), typeof(TDestination));

        if (mapFunc == null)
            throw new InvalidOperationException($"No mapping found for {typeof(TSource)} → {typeof(TDestination)}");

        // Casting do delegate armazenado no registro
        var func = (Func<TSource, TDestination>)mapFunc;

        // Projeta cada elemento da consulta
        return source.Select(x => func(x));
    }
}
