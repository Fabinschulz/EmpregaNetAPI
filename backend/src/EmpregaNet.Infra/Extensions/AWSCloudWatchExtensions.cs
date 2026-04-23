using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime;
using AWS.Logger;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Extensions;

public class AWSCloudWatch
{
    public const string SectionName = "AWSCloudWatch";
    public string LogGroup { get; set; } = string.Empty;
    public string LogStreamPrefix { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}

public static class AWSCloudWatchExtensions
{
    public static IServiceCollection SetupAWSCloudWatchLogging(this IServiceCollection services,
                                                               IConfiguration configuration)
    {
        var cloudWatchOptions = configuration.GetSection(AWSCloudWatch.SectionName).Get<AWSCloudWatch>();

        if (cloudWatchOptions == null || string.IsNullOrEmpty(cloudWatchOptions.LogGroup))
        {
            Console.WriteLine("Aviso: Configuração 'AWSCloudWatch' não encontrada ou incompleta. Usando provedores de log padrão.");
            return services;
        }

        var credentials = new BasicAWSCredentials(cloudWatchOptions.AccessKey, cloudWatchOptions.SecretKey);

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();

            loggingBuilder.AddAWSProvider(new AWSLoggerConfig
            {
                Region = cloudWatchOptions.Region,
                LogGroup = cloudWatchOptions.LogGroup,
                LogStreamNamePrefix = cloudWatchOptions.LogStreamPrefix,
                Credentials = credentials,
            });

            loggingBuilder.SetMinimumLevel(LogLevel.Information);
            loggingBuilder.AddConsole();
        });

        return services;
    }
}