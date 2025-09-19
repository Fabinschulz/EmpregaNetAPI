using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EmpregaNet.Application.Utils;

/// <summary>
/// Filtro de schema para o Swagger que adiciona à descrição dos enums os valores possíveis e suas descrições extraídas dos comentários XML.
/// Exibe uma lista HTML com os nomes e descrições dos membros do enum na documentação gerada.
/// </summary>
public class DescribeEnumMembers : ISchemaFilter
{
    /// <summary>
    /// Documento XML contendo os comentários extraídos do código (geralmente o arquivo de documentação XML do projeto).
    /// </summary>
    private readonly XDocument mXmlComments;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DescribeEnumMembers"/> com o documento XML de comentários.
    /// </summary>
    /// <param name="argXmlComments">Documento XML de comentários do código.</param>
    public DescribeEnumMembers(XDocument argXmlComments)
    {
        mXmlComments = argXmlComments;
    }

    /// <summary>
    /// Aplica o filtro ao schema do Swagger, adicionando à descrição do enum os valores possíveis e suas descrições.
    /// </summary>
    /// <param name="argSchema">Schema OpenAPI a ser modificado.</param>
    /// <param name="argContext">Contexto do filtro, contendo informações do tipo.</param>
    public void Apply(OpenApiSchema argSchema, SchemaFilterContext argContext)
    {
        var EnumType = argContext.Type;

        // Só processa se o tipo for Enum
        if (!EnumType.IsEnum) return;

        // Inicia a descrição com o texto já existente
        var sb = new StringBuilder(argSchema.Description);

        sb.AppendLine("<p>Valores Possíveis:</p>");
        sb.AppendLine("<ul>");

        // Para cada membro do enum, busca a descrição no XML e adiciona à lista
        foreach (var enumMemberName in Enum.GetNames(EnumType))
        {
            var FullEnumMemberName = $"F:{EnumType.FullName}.{enumMemberName}";

            // Busca a descrição do membro no XML de comentários
            var EnumMemberDescription = mXmlComments.XPathEvaluate($"normalize-space(//member[@name = '{FullEnumMemberName}']/summary/text())") as string;

            if (string.IsNullOrEmpty(EnumMemberDescription))
            {
                // Se não encontrar descrição, interrompe o processamento
                return;
            }
            else
            {
                sb.AppendLine($"<li><b>{enumMemberName}</b>: {EnumMemberDescription}</li>");
            }
        }

        sb.AppendLine("</ul>");

        // Atualiza a descrição do schema
        argSchema.Description = sb.ToString();
    }
}

/// <summary>
/// Filtro de schema para o Swagger que permite ignorar determinados valores de enums na documentação,
/// caso estejam decorados com o atributo <c>IgnoreEnumAttribute</c>.
/// Útil para ocultar valores de enumeração que não devem ser expostos na API.
/// </summary>
public class IgnoreEnumSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Documento XML contendo os comentários extraídos do código (não utilizado diretamente neste filtro, mas mantido para compatibilidade).
    /// </summary>
    private readonly XDocument mXmlComments;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="IgnoreEnumSchemaFilter"/> com o documento XML de comentários.
    /// </summary>
    /// <param name="argXmlComments">Documento XML de comentários do código.</param>
    public IgnoreEnumSchemaFilter(XDocument argXmlComments)
    {
        mXmlComments = argXmlComments;
    }

    public class IgnoreEnumAttribute : Attribute { }

    /// <summary>
    /// Aplica o filtro ao schema do Swagger, removendo valores de enum que estejam decorados com <c>IgnoreEnumAttribute</c>.
    /// </summary>
    /// <param name="schema">Schema OpenAPI a ser modificado.</param>
    /// <param name="context">Contexto do filtro, contendo informações do tipo.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Só processa se o tipo for Enum
        if (context.Type.IsEnum)
        {
            var enumOpenApiStrings = new List<IOpenApiAny>();

            // Para cada valor do enum, verifica se deve ser incluído na documentação
            foreach (var enumValue in Enum.GetValues(context.Type))
            {
                var member = context.Type.GetMember(enumValue.ToString()!)[0];
                // Só inclui se NÃO estiver decorado com IgnoreEnumAttribute
                if (!member.GetCustomAttributes<IgnoreEnumAttribute>().Any())
                {
                    enumOpenApiStrings.Add(new OpenApiString(enumValue.ToString()));
                }
            }

            // Atualiza a lista de valores do enum no schema
            schema.Enum = enumOpenApiStrings;
        }
    }
}
