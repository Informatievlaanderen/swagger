namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Linq;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class CleanUpTagsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var usedTagNames = swaggerDoc.Paths
                .SelectMany(path => path.Value.Operations.Values)
                .SelectMany(operation => operation.Tags ?? Enumerable.Empty<OpenApiTagReference>())
                .Select(tag => tag.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);

            swaggerDoc.Tags = usedTagNames
                .Select(name => new OpenApiTag
                {
                    Name = name
                })
                .ToHashSet();
        }
    }
}
