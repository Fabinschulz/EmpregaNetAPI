using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace EmpregaNet.Infra.Configurations;

/// <summary>
/// Classe de extensão responsável pela configuração do ambiente da aplicação Web API.
/// Permite centralizar o carregamento de arquivos de configuração (appsettings) e variáveis de ambiente
/// de acordo com o ambiente atual (Development, Staging, Production, etc).
/// </summary>
public static class WebApiConfiguration
{
    /// <summary>
    /// Configura o <see cref="WebApplicationBuilder"/> para utilizar arquivos de configuração e variáveis de ambiente.
    /// Carrega o <c>appsettings.json</c> padrão, o <c>appsettings.{Environment}.json</c> específico do ambiente
    /// e adiciona variáveis de ambiente ao provedor de configuração.
    /// </summary>
    /// <param name="web">Instância do <see cref="WebApplicationBuilder"/> a ser configurada.</param>
    /// <returns>O próprio <see cref="WebApplicationBuilder"/> configurado, permitindo encadeamento de chamadas.</returns>
    public static WebApplicationBuilder EnvironmentConfig(this WebApplicationBuilder web)
    {
        // Define o diretório base para a busca dos arquivos de configuração
        web.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            // Carrega o arquivo de configuração padrão
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            // Carrega o arquivo de configuração específico do ambiente (ex: appsettings.Development.json)
            .AddJsonFile($"appsettings.{web.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            // Adiciona variáveis de ambiente ao provedor de configuração
            .AddEnvironmentVariables();

        return web;
    }
}
