using Mapper.Interfaces;

namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Configuração central de mapeamento, responsável por registrar e validar mapeamentos.
/// </summary>
public class MapperConfiguration : IMapperConfigurationProvider, IMapperConfigurationExpression
{
    private readonly MappingRegistry _registry = new();

    /// <inheritdoc />
    public Delegate? GetMapping(Type source, Type destination) =>
        _registry.GetMappingDelegate(source, destination);

    /// <inheritdoc />
    public bool HasMapping(Type source, Type destination) =>
        _registry.Exists(source, destination);

    /// <inheritdoc />
    public IEnumerable<(Type Source, Type Destination)> GetAllMappings() =>
        _registry.GetAllMappings();

    /// <inheritdoc />
    public void CreateMap<TSource, TDestination>() =>
        new MappingExpression<TSource, TDestination>(this).Apply();

    /// <inheritdoc />
    public IMappingExpression<TSource, TDestination> CreateMapWithOptions<TSource, TDestination>() =>
        new MappingExpression<TSource, TDestination>(this);

    /// <inheritdoc />
    public void CreateMap(Type source, Type destination) =>
        _registry.RegisterDynamicMapping(source, destination);

    /// <summary>
    /// Valida todas as configurações de mapeamento registradas.
    /// Garante que todas as propriedades simples estejam mapeadas.
    /// </summary>
    /// <exception cref="InvalidOperationException">Se alguma propriedade esperada não estiver presente na origem.</exception>
    public void AssertConfigurationIsValid()
    {
        foreach (var (source, destination) in _registry.GetAllMappings())
        {
            var sourceProps = source.GetProperties();
            var destProps = destination.GetProperties();

            var customMapped = new HashSet<string>(_registry.GetCustomMappedProperties(source, destination));

            foreach (var destProp in destProps)
            {
                // Ignora se há customização ou se não é tipo simples
                if (customMapped.Contains(destProp.Name)) continue;
                if (!IsSimpleType.IsValid(destProp.PropertyType)) continue;

                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name);
                if (sourceProp == null)
                {
                    throw new InvalidOperationException(
                        $"Property '{destProp.Name}' not found in source '{source.Name}' for mapping to '{destination.Name}'.");
                }
            }
        }
    }

    /// <summary>
    /// Registra o mapeamento entre dois tipos.
    /// Opta por expression trees quando possível, senão usa reflection.
    /// </summary>
    internal void RegisterMapping<TSource, TDestination>(Dictionary<string, MemberOptions>? memberOptions = null)
    {
        var customMappedProps = memberOptions?.Keys ?? Enumerable.Empty<string>();

        // Se há membros customizados → usa reflection.
        if (memberOptions != null && memberOptions.Count > 0)
        {
            RegisterWithReflection<TSource, TDestination>(memberOptions);
            return;
        }

        // **Expression tree para mapeamento direto** (melhor performance).
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TSource), "src");
        var bindings = new List<System.Linq.Expressions.MemberBinding>();

        var sourceProps = typeof(TSource).GetProperties();
        var destProps = typeof(TDestination).GetProperties();

        foreach (var destProp in destProps)
        {
            var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name && p.PropertyType == destProp.PropertyType);
            if (sourceProp == null) continue;
            if (!IsSimpleType.IsValid(destProp.PropertyType)) continue;

            var sourceAccess = System.Linq.Expressions.Expression.Property(parameter, sourceProp);
            var binding = System.Linq.Expressions.Expression.Bind(destProp, sourceAccess);
            bindings.Add(binding);
        }

        var body = System.Linq.Expressions.Expression.MemberInit(
            System.Linq.Expressions.Expression.New(typeof(TDestination)),
            bindings
        );

        var lambda = System.Linq.Expressions.Expression.Lambda<Func<TSource, TDestination>>(body, parameter);
        var compiled = lambda.Compile();

        _registry.Register(compiled, customMappedProps);
    }

    /// <summary>
    /// Registra o mapeamento utilizando reflection, necessário quando há custom resolvers.
    /// </summary>
    private void RegisterWithReflection<TSource, TDestination>(Dictionary<string, MemberOptions> memberOptions)
    {
        var customMappedProps = memberOptions?.Keys ?? Enumerable.Empty<string>();

        _registry.Register<TSource, TDestination>(
            src => MapInternal<TSource, TDestination>(src, new HashSet<object>(), memberOptions),
            customMappedProps
        );
    }

    /// <summary>
    /// Mapeamento interno via reflection, suporta nested e coleções.
    /// </summary>
    private TDestination MapInternal<TSource, TDestination>(
        TSource src,
        HashSet<object> visited,
        Dictionary<string, MemberOptions>? memberOptions = null)
    {
        if (src == null) return default!;
        if (visited.Contains(src!)) return default!;  // **Previne ciclos**

        visited.Add(src!);

        var dest = Activator.CreateInstance<TDestination>()!;
        var sourceProps = typeof(TSource).GetProperties();
        var destProps = typeof(TDestination).GetProperties();

        foreach (var destProp in destProps)
        {
            // Se há custom resolver → usa ele.
            if (memberOptions != null && memberOptions.TryGetValue(destProp.Name, out var opt) && opt.CustomResolver != null)
            {
                var value = opt.CustomResolver(src!);
                destProp.SetValue(dest, value);
                continue;
            }

            var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name);
            if (sourceProp == null) continue;

            var sourceValue = sourceProp.GetValue(src);
            if (sourceValue == null)
            {
                destProp.SetValue(dest, null);
                continue;
            }

            var sourceType = sourceProp.PropertyType;
            var destType = destProp.PropertyType;

            // Para tipos simples → atribui diretamente.
            if (IsSimpleType.IsValid(destType))
            {
                destProp.SetValue(dest, sourceValue);
                continue;
            }

            // **Nested mapping para coleções**.
            if (IsEnumerable(sourceType) && IsEnumerable(destType))
            {
                var srcElementType = GetEnumerableElementType(sourceType);
                var destElementType = GetEnumerableElementType(destType);

                if (srcElementType != null && destElementType != null)
                {
                    var mapDelegate = _registry.GetMappingDelegate(srcElementType, destElementType);

                    if (mapDelegate != null)
                    {
                        var func = (Func<object, object>)mapDelegate;
                        var sourceEnumerable = (System.Collections.IEnumerable)sourceValue;

                        var listType = typeof(List<>).MakeGenericType(destElementType);
                        var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

                        foreach (var item in sourceEnumerable)
                        {
                            if (item == null || visited.Contains(item)) continue;

                            visited.Add(item);
                            var mappedItem = func(item);
                            list.Add(mappedItem);
                        }

                        destProp.SetValue(dest, list);
                        continue;
                    }
                }
            }

            // **Nested mapping para objeto simples**
            var nestedMap = _registry.GetMappingDelegate(sourceType, destType);
            if (nestedMap != null)
            {
                var nestedFunc = (Func<object, object>)nestedMap;

                if (!visited.Contains(sourceValue))
                {
                    visited.Add(sourceValue);
                    var nestedDest = nestedFunc(sourceValue);
                    destProp.SetValue(dest, nestedDest);
                }
            }
            else
            {
                // Fallback: atribui direto.
                destProp.SetValue(dest, sourceValue);
            }
        }

        return dest;
    }

    private static bool IsEnumerable(Type type)
    {
        return type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    /// Obtém o tipo de elemento de uma coleção.
    /// </summary>
    private static Type? GetEnumerableElementType(Type enumerableType)
    {
        return enumerableType.IsArray
            ? enumerableType.GetElementType()
            : enumerableType.GetGenericArguments().FirstOrDefault();
    }

    /// <summary>
    /// Classe auxiliar para determinar se um tipo é considerado "simples".
    /// Tipos simples são aqueles que podem ser mapeados diretamente sem processamento adicional.
    /// </summary>
    private static class IsSimpleType
    {
        /// <summary>
        /// Determina se o tipo informado é considerado simples.
        /// </summary>
        /// <param name="type">Tipo a ser verificado.</param>
        /// <returns>
        /// <c>true</c> se o tipo for primitivo, enumeração, string, decimal, DateTime ou Guid (incluindo Nullable destes tipos); caso contrário, <c>false</c>.
        /// </returns>
        public static bool IsValid(Type type)
        {
            // Se for Nullable<T>, obtém o tipo subjacente
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
            }

            // Verifica se é tipo simples
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(Guid);
        }
    }
}
