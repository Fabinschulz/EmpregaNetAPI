using System.Reflection;
using Elastic.Apm.SerilogEnricher;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace EmpregaNet.Infra.Configurations
{
    public static class SerilogConfig
    {
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true).Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithElasticApmCorrelationInfo()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Host.UseSerilog(Log.Logger, true);

            return builder;
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment)
        {
            var settings = new ElasticsearchSettings
            {
                Uri = configuration["ElasticsearchSettings:Uri"]!,
                Username = configuration["ElasticsearchSettings:Username"]!,
                Password = configuration["ElasticsearchSettings:Password"]!,
            };

            if (string.IsNullOrEmpty(settings.Uri) || string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
                throw new ArgumentNullException(nameof(settings.Uri), "Elasticsearch não configurado corretamente.");

            return new ElasticsearchSinkOptions(new Uri(settings.Uri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logs-{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                FailureCallback = (logEvent, exception) => Log.Error(exception, $"Falha ao enviar log para Elasticsearch: {logEvent.MessageTemplate}"),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback,
                ModifyConnectionSettings = conn =>
                {
                    var modified = conn;

                    if (!string.IsNullOrEmpty(settings.Username) && !string.IsNullOrEmpty(settings.Password))
                    {
                        modified = modified.BasicAuthentication(settings.Username, settings.Password);
                    }

                    return modified;
                }
            };
        }
    }
}
