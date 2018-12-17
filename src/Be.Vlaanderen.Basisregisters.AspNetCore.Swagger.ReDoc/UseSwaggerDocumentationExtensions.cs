namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;

    public static class UseSwaggerDocumentationExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            IApiVersionDescriptionProvider provider,
            Func<string, string> titleFunc)
        {
            app.Map(new PathString("/docs"), apiDocs =>
            {
                apiDocs.UseSwagger(x =>
                {
                    x.RouteTemplate = "{documentName}/docs.json";
                    x.PreSerializeFilters.Add((doc, _) => doc.BasePath = "/");
                });

                var apiVersions = provider.ApiVersionDescriptions.Select(x => x.GroupName).OrderBy(x => x).ToList();
                foreach (var description in apiVersions)
                {
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = titleFunc(description);
                        x.SpecUrl = $"/docs/{description}/docs.json";
                        x.RoutePrefix = $"{description}";
                    });
                }

                if (apiVersions.Count > 0)
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = titleFunc(apiVersions[0]);
                        x.SpecUrl = $"/docs/{apiVersions[0]}/docs.json";
                        x.RoutePrefix = string.Empty;
                    });
            });

            return app;
        }
    }
}
