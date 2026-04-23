namespace EmpregaNet.Application.Common.Cache;

/// <summary>
/// Chaves de cache HTTP alinhadas entre API e invalidações na Application (evita strings soltas e prefixos divergentes).
/// </summary>
public static class ApplicationCacheKeys
{
    /// <summary>Chaves no padrão do MainController genérico da API (GetAll/GetById com filtros).</summary>
    public static class Entity
    {
        public static string GetAll(string viewModelName, int page, int size, string? orderBy, bool? isDeleted, bool? isActive) =>
            $"{viewModelName}_GetAll_{page}_{size}_{orderBy}_{isDeleted}_{isActive}";

        public static string GetById(string viewModelName, long id) => $"{viewModelName}_GetById_{id}";

        /// <summary>Invalida todas as variações de GetById para um id (filtros na query string).</summary>
        public static string GetByIdPrefix(string viewModelName, long id) =>
            $"{viewModelName}_GetById_{id}";

        public static string GetAllPrefix(string viewModelName) => $"{viewModelName}_GetAll_";
    }

    public static class Users
    {
        public static string Me(long userId) => $"Users_Me_{userId}";

        public static string Me(string userId) => $"Users_Me_{userId}";

        public static string AdminList(int page, int size, string? orderBy, bool? isDeleted) =>
            $"Users_Admin_List_{page}_{size}_{orderBy}_{isDeleted}";

        public static string AdminById(long id) => $"Users_Admin_ById_{id}";

        public const string AdminPrefix = "Users_Admin_";
    }

    public static class Candidates
    {
        public static string GetAll(int page, int size, string? orderBy) =>
            $"Candidates_GetAll_{page}_{size}_{orderBy}";

        public static string GetById(long id) => $"Candidates_GetById_{id}";

        public const string GetAllPrefix = "Candidates_GetAll_";
    }

    public static class JobApplications
    {
        public static string Mine(int page, int size, string? status, string? orderBy) =>
            $"JobApplications_Mine_{page}_{size}_{status}_{orderBy}";

        public static string ByJob(long jobId, int page, int size, string? status, string? orderBy) =>
            $"JobApplications_Job_{jobId}_{page}_{size}_{status}_{orderBy}";

        public const string MinePrefix = "JobApplications_Mine_";
        public const string ByJobPrefix = "JobApplications_Job_";
    }
}
