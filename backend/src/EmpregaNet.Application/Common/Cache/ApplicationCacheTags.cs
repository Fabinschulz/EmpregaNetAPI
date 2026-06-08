namespace EmpregaNet.Application.Common.Cache;

/// <summary>
/// Tags do Output Cache alinhadas com invalidações na Application e API.
/// </summary>
public static class ApplicationCacheTags
{
    public const string AdminUsers = "users:admin";
    public const string Candidates = "candidates";
    public const string JobApplicationsMine = "jobapplications:mine";
    public const string JobApplicationsByJob = "jobapplications:by-job";

    public static string Entity(string viewModelName) => $"entity:{viewModelName}";

    public static string EntityList(string viewModelName) => $"entity:{viewModelName}:list";

    public static string EntityById(string viewModelName, long id) => $"entity:{viewModelName}:id:{id}";

    public static string UserMe(long userId) => $"users:me:{userId}";
}
