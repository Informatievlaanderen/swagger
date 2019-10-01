namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AlternateServersFilter : IDocumentFilter
    {
        private readonly IEnumerable<Server> _servers;

        public AlternateServersFilter(IEnumerable<Server> servers)
            => _servers = servers;

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions["x-servers"] = _servers.Select(x => new {url = x.Url, description = x.Description});
            swaggerDoc.Extensions["servers"] = _servers.Select(x => new {url = x.Url, description = x.Description});
        }
    }
}
