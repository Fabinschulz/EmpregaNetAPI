using System.Reflection;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Domain.Services;

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