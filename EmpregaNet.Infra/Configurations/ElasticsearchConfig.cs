using Nest;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Elastic.Apm;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EmpregaNet.Infra.Configurations
{
    public class ElasticsearchSettings
    {
        public string Uri { get; set; } = string.Empty;
        public string DefaultIndex { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
    }

    public static class ElasticsearchConfig
    {
        public static void AddElasticsearch(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IElasticClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;

            if (string.IsNullOrEmpty(settings.Uri) ||
                string.IsNullOrEmpty(settings.Username) ||
                string.IsNullOrEmpty(settings.Password))
            {
                throw new ArgumentNullException("ElasticsearchSettings", "Configurações do Elasticsearch incompletas.");
            }

            var connectionSettings = new ConnectionSettings(new Uri(settings.Uri))
                .BasicAuthentication(settings.Username, settings.Password)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                .EnableHttpCompression()
                .EnableApiVersioningHeader()
                .DefaultIndex(settings.DefaultIndex);

            return new ElasticClient(connectionSettings);
        });

        }

        public static void UseElasticApm(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElasticApm();
            services.Configure<AgentComponents>(configuration.GetSection("ElasticApm"));
        }
    }
}


