using Mapper.Interfaces;

namespace EmpregaNet.Domain.Components.Mapper;

/// <summary>
/// Representa as opções de configuração para um membro (propriedade) durante o mapeamento.
/// Permite definir um resolvedor personalizado para transformar o valor da propriedade.
/// </summary>
public class MemberOptions
{
    /// <summary>
    /// Função customizada para resolver o valor do membro de destino a partir da fonte.
    /// Caso definida, sobrescreve o mapeamento padrão.
    /// </summary>
    public Func<object, object>? CustomResolver { get; set; }
}

/// <summary>
/// Representa uma expressão de configuração de mapeamento entre <typeparamref name="TSource"/> e <typeparamref name="TDestination"/>.
/// Permite configuração fluente, suporte a ReverseMap e customização de membros.
/// </summary>
/// <typeparam name="TSource">Tipo de origem.</typeparam>
/// <typeparam name="TDestination">Tipo de destino.</typeparam>
public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
{
    private readonly MapperConfiguration _config;
    private bool _reverseMap = false;
    private readonly Dictionary<string, MemberOptions> _memberOptions = new();

    /// <summary>
    /// Inicializa a expressão de mapeamento com a configuração fornecida.
    /// </summary>
    /// <param name="config">Instância de <see cref="MapperConfiguration"/> associada.</param>
    public MappingExpression(MapperConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Habilita o mapeamento reverso (de <typeparamref name="TDestination"/> para <typeparamref name="TSource"/>).
    /// </summary>
    /// <returns>Instância atual para encadeamento fluente.</returns>
    public IMappingExpression<TSource, TDestination> ReverseMap()
    {
        _reverseMap = true;
        return this;
    }

    /// <summary>
    /// Configura uma opção customizada para um membro de destino.
    /// </summary>
    /// <param name="memberName">Nome do membro de destino.</param>
    /// <param name="options">Ação de configuração do <see cref="MemberOptions"/>.</param>
    /// <returns>Instância atual para encadeamento fluente.</returns>
    public IMappingExpression<TSource, TDestination> ForMember(string memberName, Action<MemberOptions> options)
    {
        var opts = new MemberOptions();
        options(opts);
        _memberOptions[memberName] = opts;
        return this;
    }

    /// <summary>
    /// Aplica as configurações de mapeamento, registrando no <see cref="MapperConfiguration"/>.
    /// </summary>
    public void Apply()
    {
        // Registra mapeamento principal com membros customizados (se houver)
        _config.RegisterMapping<TSource, TDestination>(_memberOptions);

        // Se ReverseMap estiver ativo → registra também TDestination → TSource
        if (_reverseMap)
        {
            _config.RegisterMapping<TDestination, TSource>();
        }
    }
}
