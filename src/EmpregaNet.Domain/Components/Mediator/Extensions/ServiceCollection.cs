using System.Reflection;
using EmpregaNet.Domain.Components.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;


/// <summary>
/// Classe de extensão para registro automático do Mediator na coleção de serviços.
/// 
/// ✅ O método AddMediator registra as interfaces principais do Mediator:
///     - IMediator → implementação Mediator
///     - IRequestHandler<,> e INotificationHandler<> → resolve automaticamente todas as classes que implementam essas interfaces.
///
/// ✅ Permite a passagem opcional de filtros para assemblies:
///     - Sem parâmetros → registra de todos os assemblies carregados.
///     - Com array de Assembly → registra apenas os fornecidos.
///     - Com array de string → registra apenas os assemblies cujo nome inicia com algum dos prefixos fornecidos.
///
/// 📌 Exemplo de uso na Startup ou Program:
/// services.AddMediator(); // Registra handlers de todos os assemblies carregados
/// services.AddMediator(typeof(MyApp.SomeClass).Assembly);
/// services.AddMediator("MyApp", "MyApp.Domain"); // Registra assemblies que começam com "MyApp" ou "MyApp.Domain"
///
/// 🚨 Erro lançado se o parâmetro for inválido (não Assembly nem string).
/// </summary>
namespace EmpregaNet.Domain.Components.Mediator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(
    this IServiceCollection services,
    params object[] args)
    {
        var assemblies = ResolveAssemblies(args);

        services.AddSingleton<IMediator, Mediator>();

        RegisterHandlers(services, assemblies, typeof(INotificationHandler<>));
        RegisterHandlers(services, assemblies, typeof(IRequestHandler<,>));
        RegisterPipelineBehaviors(services, assemblies);

        return services;
    }

    /// <summary>
    /// Resolve os assemblies com base nos argumentos fornecidos.
    /// </summary>
    private static Assembly[] ResolveAssemblies(object[] args)
    {
        // Returna todos os assemblies
        if (args == null || args.Length == 0)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName))
                .ToArray();
        }

        // Returna os assemblies fornecidos diretamente
        if (args.All(a => a is Assembly))
            return args.Cast<Assembly>().ToArray();

        // Returna os assemblies que começam com os prefixos fornecidos
        if (args.All(a => a is string))
        {
            var prefixes = args.Cast<string>().ToArray();
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a =>
                    !a.IsDynamic &&
                    !string.IsNullOrWhiteSpace(a.FullName) &&
                    prefixes.Any(p => a.FullName!.StartsWith(p)))
                .ToArray();
        }

        throw new ArgumentException("Invalid parameters for AddSimpleMediator(). Use: no arguments, Assembly[], or prefix strings.");
    }

    /// <summary>
    /// Registra no DI todas as classes que implementam o handler genérico especificado.
    /// </summary>
    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies, Type handlerInterface)
    {
        var types = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerInterface);

            foreach (var iface in interfaces)
            {
                services.AddTransient(iface, type);
            }
        }
    }

    /// <summary>
    /// Registra no DI todos os behaviors que implementam o IPipelineBehavior genérico.
    /// </summary>
    /// <remarks>
    /// Os behaviors são resolvidos dinamicamente com base no tipo de request e response.
    /// </remarks>
    private static void RegisterPipelineBehaviors(IServiceCollection services, Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

            foreach (var iface in interfaces)
            {
                services.AddTransient(iface, type);
            }
        }
    }
}