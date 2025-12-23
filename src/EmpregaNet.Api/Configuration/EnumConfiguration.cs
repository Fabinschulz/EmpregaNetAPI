using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

/// <summary>
/// Filtro de schema para o Swagger que adiciona à descrição dos enums os valores possíveis e suas descrições extraídas dos comentários XML.
/// Exibe uma lista HTML com os nomes e descrições dos membros do enum na documentação gerada.
/// </summary>
public sealed class DescribeEnumMembers : ISchemaFilter
{
    private readonly XDocument _xmlComments;

    public DescribeEnumMembers(XDocument xmlComments)
    {
        _xmlComments = xmlComments;
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        var sb = new StringBuilder(schema.Description);

        sb.AppendLine("<p>Valores possíveis:</p><ul>");

        foreach (var name in Enum.GetNames(context.Type))
        {
            var memberName = $"F:{context.Type.FullName}.{name}";
            var description = _xmlComments.XPathEvaluate(
                $"normalize-space(//member[@name='{memberName}']/summary/text())"
            ) as string;

            if (!string.IsNullOrWhiteSpace(description))
            {
                sb.AppendLine($"<li><b>{name}</b>: {description}</li>");
            }
        }

        sb.AppendLine("</ul>");
        schema.Description = sb.ToString();
    }
}

/// <summary>
/// Filtro de schema para o Swagger que permite ignorar determinados valores de enums na documentação,
/// caso estejam decorados com o atributo <c>IgnoreEnumAttribute</c>.
/// Útil para ocultar valores de enumeração que não devem ser expostos na API.
/// </summary>
public sealed class IgnoreEnumSchemaFilter : ISchemaFilter
{
    public sealed class IgnoreEnumAttribute : Attribute { }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum || schema.Enum == null)
            return;

        var allowedValues = Enum.GetValues(context.Type)
            .Cast<object>()
            .Where(v =>
            {
                var member = context.Type.GetMember(v.ToString()!)[0];
                return !member.IsDefined(typeof(IgnoreEnumAttribute), false);
            })
            .Select(v => v.ToString()!)
            .ToHashSet();

        var filteredEnum = schema.Enum.Where(e => allowedValues.Contains(e.ToString())).ToList();
        schema.Enum.Clear();
        foreach (var item in filteredEnum)
        {
            schema.Enum.Add(item);
        }
    }
}


