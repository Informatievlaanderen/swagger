namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Nodes;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AlternateServersFilter : IDocumentFilter
    {
        private readonly IEnumerable<Server> _servers;

        public AlternateServersFilter(IEnumerable<Server> servers)
            => _servers = servers;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var serverArray = new JsonArray(_servers
                .Select(x => new JsonObject
                {
                    ["url"] = JsonValue.Create(x.Url),
                    ["description"] = JsonValue.Create(x.Description),
                })
                .ToArray());

            swaggerDoc.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            swaggerDoc.Extensions["x-servers"] = new JsonNodeExtension(serverArray);
        }
    }
}
