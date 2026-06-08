using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Cache;

public sealed class OutputCacheManager : IOutputCacheManager
{
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly ILogger<OutputCacheManager> _logger;

    public OutputCacheManager(IOutputCacheStore outputCacheStore, ILogger<OutputCacheManager> logger)
    {
        _outputCacheStore = outputCacheStore;
        _logger = logger;
    }

    public Task InvalidateEntityAsync(string viewModelName, long id = default, CancellationToken cancellationToken = default)
    {
        var tags = new List<string>
        {
            ApplicationCacheTags.Entity(viewModelName),
            ApplicationCacheTags.EntityList(viewModelName)
        };

        if (id != default)
            tags.Add(ApplicationCacheTags.EntityById(viewModelName, id));

        return EvictTagsAsync(cancellationToken, [.. tags]);
    }

    public async Task InvalidateAdminUsersAsync(long userId = default, CancellationToken cancellationToken = default)
    {
        await EvictTagAsync(ApplicationCacheTags.AdminUsers, cancellationToken);

        if (userId == default)
            return;

        await InvalidateUserMeAsync(userId, cancellationToken);
        await InvalidateCandidatesAsync(userId, cancellationToken);
    }

    public Task InvalidateCandidatesAsync(long candidateId = default, CancellationToken cancellationToken = default)
        => EvictTagAsync(ApplicationCacheTags.Candidates, cancellationToken);

    public Task InvalidateUserMeAsync(long userId, CancellationToken cancellationToken = default)
        => EvictTagAsync(ApplicationCacheTags.UserMe(userId), cancellationToken);

    public Task InvalidateJobApplicationsAsync(CancellationToken cancellationToken = default)
        => EvictTagsAsync(
            cancellationToken,
            ApplicationCacheTags.JobApplicationsMine,
            ApplicationCacheTags.JobApplicationsByJob);

    private Task EvictTagsAsync(CancellationToken cancellationToken, params string[] tags)
    {
        var tasks = tags.Select(tag => EvictTagAsync(tag, cancellationToken));
        return Task.WhenAll(tasks);
    }

    private async Task EvictTagAsync(string tag, CancellationToken cancellationToken)
    {
        try
        {
            await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);
            _logger.LogDebug("Output Cache invalidado pela tag {Tag}.", tag);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao invalidar Output Cache pela tag {Tag}.", tag);
        }
    }
}
