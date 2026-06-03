namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.Json.Nodes;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Add an AutoRest vendor extension (see https://github.com/Azure/autorest/blob/master/docs/extensions/readme.md#x-ms-enum) to inform the AutoRest tool how enums should be modelled when it generates the API client.
    /// </summary>
    public class AutoRestSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            var typeInfo = context.Type.GetTypeInfo();

            if (typeInfo.IsEnum && schema is OpenApiSchema openApiSchema)
            {
                openApiSchema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                openApiSchema.Extensions["x-ms-enum"] = new JsonNodeExtension(JsonValue.Create(typeInfo.Name));
            }
        }
    }
}
