namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;

    public class SwaggerDocumentationOptions
    {
        /// <summary>
        /// Defines the behavior of a provider that discovers and describes API version information within an application.
        /// </summary>
        public IApiVersionDescriptionProvider ApiVersionDescriptionProvider { get; set; }

        /// <summary>
        /// Sets a title for the ReDoc page.
        /// </summary>
        public Func<string, string> DocumentTitleFunc { get; set; }
    }

    public static class UseSwaggerDocumentationExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            SwaggerDocumentationOptions options)
        {
            if (options.ApiVersionDescriptionProvider == null)
                throw new ArgumentNullException(nameof(options.ApiVersionDescriptionProvider));

            if (options.DocumentTitleFunc == null)
                throw new ArgumentNullException(nameof(options.DocumentTitleFunc));

            app.Map(new PathString("/docs"), apiDocs =>
            {
                apiDocs.UseSwagger(x =>
                {
                    x.RouteTemplate = "{documentName}/docs.json";
                    x.PreSerializeFilters.Add((doc, _) => doc.BasePath = "/");
                });

                var apiVersions = options
                    .ApiVersionDescriptionProvider
                    .ApiVersionDescriptions
                    .Select(x => x.GroupName)
                    .OrderBy(x => x)
                    .ToList();

                foreach (var description in apiVersions)
                {
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = options.DocumentTitleFunc(description);
                        x.SpecUrl = $"/docs/{description}/docs.json";
                        x.RoutePrefix = $"{description}";
                    });
                }

                if (apiVersions.Count > 0)
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = options.DocumentTitleFunc(apiVersions[0]);
                        x.SpecUrl = $"/docs/{apiVersions[0]}/docs.json";
                        x.RoutePrefix = string.Empty;
                    });
            });

            return app;
        }
    }
}
