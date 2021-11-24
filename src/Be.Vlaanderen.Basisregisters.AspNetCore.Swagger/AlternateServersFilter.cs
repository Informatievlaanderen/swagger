namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AlternateServersFilter : IDocumentFilter
    {
        private readonly IEnumerable<Server> _servers;

        public AlternateServersFilter(IEnumerable<Server> servers)
            => _servers = servers;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var serverArray = new OpenApiArray();
            _servers.ToList().ForEach(x => serverArray.Add(new OpenApiObject
            {
                {"url", new OpenApiString(x.Url) },
                { "description", new OpenApiString(x.Description) }
            }));

            swaggerDoc.Extensions["x-servers"] = serverArray;
        }
    }
}
