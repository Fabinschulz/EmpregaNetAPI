using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmpregaNet.Api.Configuration;

/// <summary>
/// Serialização JSON das respostas dos controllers.
/// </summary>
internal static class ResponseJsonConfig
{
    public static void Configure(MvcNewtonsoftJsonOptions options)
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Se descomentado, ignora propriedades nulas no JSON de resposta
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    }
}
