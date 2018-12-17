namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Reflection;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Add an AutoRest vendor extension (see https://github.com/Azure/autorest/blob/master/docs/extensions/readme.md#x-ms-enum) to inform the AutoRest tool how enums should be modelled when it generates the API client.
    /// </summary>
    public class AutoRestSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();

            if (typeInfo.IsEnum)
                schema.Extensions.Add("x-ms-enum", new { name = typeInfo.Name, modelAsString = true });
        }
    }
}
