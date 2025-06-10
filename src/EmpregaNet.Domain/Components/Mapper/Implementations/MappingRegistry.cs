namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Registro central de todas as funções de mapeamento configuradas.
/// Mantém também informações sobre propriedades customizadas.
/// </summary>
public class MappingRegistry
{
    // Armazena os delegates de mapeamento por combinação (Source, Destination)
    private readonly Dictionary<(Type, Type), Delegate> _mappings = new();

    // Armazena os nomes das propriedades customizadas que não devem ser mapeadas automaticamente
    private readonly Dictionary<(Type, Type), HashSet<string>> _customMappedProperties = new();

    /// <summary>
    /// Registra uma função de mapeamento entre <typeparamref name="TSource"/> e <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="mapFunc">Delegate de função de mapeamento.</param>
    /// <param name="customMembers">Propriedades customizadas que não devem ser validadas automaticamente.</param>
    public void Register<TSource, TDestination>(
        Func<TSource, TDestination> mapFunc,
        IEnumerable<string>? customMembers = null)
    {
        var key = (typeof(TSource), typeof(TDestination));
        _mappings[key] = mapFunc;

        // Se houver propriedades customizadas → armazena para validação posterior
        if (customMembers != null)
        {
            _customMappedProperties[key] = new HashSet<string>(customMembers);
        }
    }

    /// <summary>
    /// Verifica se existe um mapeamento registrado entre os tipos especificados.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    /// <returns>True se existir; caso contrário, false.</returns>
    public bool Exists(Type source, Type destination) =>
        _mappings.ContainsKey((source, destination));

    /// <summary>
    /// Obtém as propriedades customizadas configuradas para o mapeamento entre os tipos.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    /// <returns>Enumerable de nomes de propriedades customizadas.</returns>
    public IEnumerable<string> GetCustomMappedProperties(Type source, Type destination)
    {
        var key = (source, destination);
        return _customMappedProperties.TryGetValue(key, out var set) ? set : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Obtém o delegate de mapeamento registrado entre os tipos especificados.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    /// <returns>Delegate de mapeamento, ou null se não encontrado.</returns>
    public Delegate? GetMappingDelegate(Type source, Type destination) =>
        _mappings.TryGetValue((source, destination), out var del) ? del : null;

    /// <summary>
    /// Obtém todas as combinações de tipos registrados para mapeamento.
    /// </summary>
    /// <returns>Enumerable de tuplas (Source, Destination).</returns>
    public IEnumerable<(Type Source, Type Destination)> GetAllMappings() =>
        _mappings.Keys;

    /// <summary>
    /// Registra um mapeamento dinâmico utilizando reflection.
    /// Este mapeamento copia propriedades com o mesmo nome e tipo.
    /// </summary>
    /// <param name="sourceType">Tipo de origem.</param>
    /// <param name="destinationType">Tipo de destino.</param>
    public void RegisterDynamicMapping(Type sourceType, Type destinationType)
    {
        // Cria um delegate de mapeamento utilizando reflection.
        _mappings[(sourceType, destinationType)] = new Func<object, object>(source =>
        {
            // Cria uma nova instância do destino.
            var destination = Activator.CreateInstance(destinationType)!;

            var sourceProps = sourceType.GetProperties();
            var destProps = destinationType.GetProperties();

            foreach (var destProp in destProps)
            {
                // Mapeia propriedades com mesmo nome e tipo.
                var sourceProp = Array.Find(sourceProps, p => p.Name == destProp.Name && p.PropertyType == destProp.PropertyType);
                if (sourceProp != null)
                {
                    destProp.SetValue(destination, sourceProp.GetValue(source));
                }
            }

            return destination;
        });
    }
}
