using System.Security.Claims;
using EmpregaNet.Application.Common.Cache;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace EmpregaNet.Api.Configuration.OutputCache;

/// <summary>Utilitários compartilhados pelas políticas de Output Cache.</summary>
internal static class OutputCachePolicyHelpers
{
    public static bool IsReadMethod(HttpContext httpContext)
        => HttpMethods.IsGet(httpContext.Request.Method)
           || HttpMethods.IsHead(httpContext.Request.Method);

    public static bool IsAuthenticated(HttpContext httpContext)
        => httpContext.User.Identity?.IsAuthenticated == true;

    public static string? ResolveUserId(HttpContext httpContext)
        => httpContext.User.FindFirstValue("userId")
           ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public static void ApplyAuthenticatedVaryRules(OutputCacheContext context)
    {
        context.CacheVaryByRules.QueryKeys = "*";
        context.CacheVaryByRules.VaryByValues["userId"] =
            ResolveUserId(context.HttpContext) ?? "anonymous";
    }

    public static string? ResolveViewModelName(HttpContext httpContext)
    {
        var descriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var controllerType = descriptor?.ControllerTypeInfo;
        if (controllerType is null)
            return null;

        for (var type = controllerType.AsType(); type is not null; type = type.BaseType)
        {
            if (!type.IsGenericType)
                continue;

            if (type.GetGenericTypeDefinition() == typeof(Controllers.Core.MainController<,,>))
                return type.GetGenericArguments()[2].Name;
        }

        return null;
    }

    public static void ApplyEntityTags(OutputCacheContext context, string viewModelName, bool isList, long? id = null)
    {
        context.Tags.Add(ApplicationCacheTags.Entity(viewModelName));

        if (isList)
            context.Tags.Add(ApplicationCacheTags.EntityList(viewModelName));
        else if (id.HasValue)
            context.Tags.Add(ApplicationCacheTags.EntityById(viewModelName, id.Value));
    }

    public static (bool IsList, long? Id) ResolveEntityRoute(HttpContext httpContext)
    {
        var routeValues = httpContext.GetRouteData().Values;
        var isList = !routeValues.ContainsKey("id");

        long? id = routeValues.TryGetValue("id", out var routeId)
                   && long.TryParse(routeId?.ToString(), out var parsedId)
            ? parsedId
            : null;

        return (isList, id);
    }
}

/// <summary>
/// Base das políticas: aplica TTL configurável e regras padrão do ASP.NET Core na resposta (200, sem cookies).
/// </summary>
internal abstract class OutputCachePolicyBase(IOptions<OutputCacheOptions> options) : IOutputCachePolicy
{
    protected TimeSpan Expiration { get; } =
        TimeSpan.FromMinutes(Math.Max(1, options.Value.DefaultExpirationMinutes));

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        if (!ShouldCacheRequest(context))
        {
            context.EnableOutputCaching = false;
            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = Expiration;

        ConfigureCache(context);
        return ValueTask.CompletedTask;
    }

    protected abstract bool ShouldCacheRequest(OutputCacheContext context);

    protected abstract void ConfigureCache(OutputCacheContext context);

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;

        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            context.AllowCacheStorage = false;

        if (response.StatusCode != StatusCodes.Status200OK)
            context.AllowCacheStorage = false;

        return ValueTask.CompletedTask;
    }
}

/// <summary>Catálogo público: GET/HEAD anónimo; tags de entidade para invalidação.</summary>
internal sealed class PublicCatalogOutputCachePolicy(IOptions<OutputCacheOptions> options)
    : OutputCachePolicyBase(options)
{
    protected override bool ShouldCacheRequest(OutputCacheContext context)
        => OutputCachePolicyHelpers.IsReadMethod(context.HttpContext);

    protected override void ConfigureCache(OutputCacheContext context)
    {
        context.CacheVaryByRules.QueryKeys = "*";

        var viewModelName = OutputCachePolicyHelpers.ResolveViewModelName(context.HttpContext);
        if (string.IsNullOrEmpty(viewModelName))
            return;

        var (isList, id) = OutputCachePolicyHelpers.ResolveEntityRoute(context.HttpContext);
        OutputCachePolicyHelpers.ApplyEntityTags(context, viewModelName, isList, id);
    }
}

/// <summary>CRUD genérico autenticado: vary por utilizador + query; tags de entidade dinâmicas.</summary>
internal sealed class EntityReadOutputCachePolicy(IOptions<OutputCacheOptions> options)
    : OutputCachePolicyBase(options)
{
    protected override bool ShouldCacheRequest(OutputCacheContext context)
        => OutputCachePolicyHelpers.IsReadMethod(context.HttpContext)
           && OutputCachePolicyHelpers.IsAuthenticated(context.HttpContext);

    protected override void ConfigureCache(OutputCacheContext context)
    {
        OutputCachePolicyHelpers.ApplyAuthenticatedVaryRules(context);

        var viewModelName = OutputCachePolicyHelpers.ResolveViewModelName(context.HttpContext);
        if (string.IsNullOrEmpty(viewModelName))
            return;

        var (isList, id) = OutputCachePolicyHelpers.ResolveEntityRoute(context.HttpContext);
        OutputCachePolicyHelpers.ApplyEntityTags(context, viewModelName, isList, id);
    }
}

/// <summary>
/// Leitura autenticada genérica. Tags de invalidação vêm do atributo <c>[OutputCache(Tags = ...)]</c> no endpoint.
/// </summary>
internal sealed class AuthenticatedReadOutputCachePolicy(IOptions<OutputCacheOptions> options)
    : OutputCachePolicyBase(options)
{
    protected override bool ShouldCacheRequest(OutputCacheContext context)
        => OutputCachePolicyHelpers.IsReadMethod(context.HttpContext)
           && OutputCachePolicyHelpers.IsAuthenticated(context.HttpContext);

    protected override void ConfigureCache(OutputCacheContext context)
        => OutputCachePolicyHelpers.ApplyAuthenticatedVaryRules(context);
}

/// <summary>Perfil do utilizador autenticado: vary por userId; tag dinâmica para invalidação de /users/me.</summary>
internal sealed class UserProfileReadOutputCachePolicy(IOptions<OutputCacheOptions> options)
    : OutputCachePolicyBase(options)
{
    protected override bool ShouldCacheRequest(OutputCacheContext context)
    {
        if (!OutputCachePolicyHelpers.IsReadMethod(context.HttpContext)
            || !OutputCachePolicyHelpers.IsAuthenticated(context.HttpContext))
            return false;

        return !string.IsNullOrEmpty(OutputCachePolicyHelpers.ResolveUserId(context.HttpContext));
    }

    protected override void ConfigureCache(OutputCacheContext context)
    {
        var userId = OutputCachePolicyHelpers.ResolveUserId(context.HttpContext)!;
        context.CacheVaryByRules.VaryByValues["userId"] = userId;

        if (long.TryParse(userId, out var parsedUserId))
            context.Tags.Add(ApplicationCacheTags.UserMe(parsedUserId));
    }
}
