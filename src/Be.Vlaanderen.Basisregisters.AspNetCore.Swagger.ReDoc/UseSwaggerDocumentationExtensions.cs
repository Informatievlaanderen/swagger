namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;
    using NSwag;
    using NSwag.CodeGeneration.CSharp;
    using NSwag.CodeGeneration.TypeScript;

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

        public CSharpClientOptions CSharpClient { get; } = new CSharpClientOptions();

        public class CSharpClientOptions
        {
            public string ClassName { get; set; }
            public string Namespace { get; set; }
        }

        public TypeScriptClientOptions TypeScriptClient { get; } = new TypeScriptClientOptions();

        public class TypeScriptClientOptions
        {
            public string ClassName { get; set; }
        }
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

            if (string.IsNullOrWhiteSpace(options.CSharpClient.ClassName))
                throw new ArgumentNullException(nameof(options.CSharpClient.ClassName));

            if (string.IsNullOrWhiteSpace(options.CSharpClient.Namespace))
                throw new ArgumentNullException(nameof(options.CSharpClient.Namespace));

            if (string.IsNullOrWhiteSpace(options.TypeScriptClient.ClassName))
                throw new ArgumentNullException(nameof(options.TypeScriptClient.ClassName));

            app
                .MapDocs(options)
                .MapCSharpClients(options)
                .MapAngularClients(options);

            return app;
        }

        private static IApplicationBuilder MapDocs(this IApplicationBuilder app, SwaggerDocumentationOptions options)
        {
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

                var stringLocalizer = app.ApplicationServices.GetRequiredService<IStringLocalizer<Localization>>();

                foreach (var description in apiVersions)
                {
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = stringLocalizer[options.DocumentTitleFunc(description)];
                        x.SpecUrl = $"/docs/{description}/docs.json";
                        x.RoutePrefix = $"{description}";
                    });
                }

                if (apiVersions.Count > 0)
                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = stringLocalizer[options.DocumentTitleFunc(apiVersions[0])];
                        x.SpecUrl = $"/docs/{apiVersions[0]}/docs.json";
                        x.RoutePrefix = string.Empty;
                    });
            });

            return app;
        }

        private static IApplicationBuilder MapCSharpClients(this IApplicationBuilder app, SwaggerDocumentationOptions options)
        {
            app.Map(new PathString("/clients/csharp"), apiClients =>
            {
                var apiVersions = options
                    .ApiVersionDescriptionProvider
                    .ApiVersionDescriptions
                    .Select(x => x.GroupName)
                    .OrderBy(x => x)
                    .ToList();

                // Hello Client: v1, Swagger: /docs/v1/docs.json, Prefix: v1
                foreach (var description in apiVersions)
                {
                    apiClients.Map(new PathString($"/{description}"), apiClient => apiClient.Run(async context =>
                    {
                        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                        var document = await SwaggerDocument.FromUrlAsync($"{baseUrl}/docs/{description}/docs.json");

                        var settings = new SwaggerToCSharpClientGeneratorSettings
                        {
                            ClassName = $"{options.CSharpClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                            CSharpGeneratorSettings =
                            {
                                Namespace = options.CSharpClient.Namespace,
                            }
                        };

                        var generator = new SwaggerToCSharpClientGenerator(document, settings);
                        var code = generator.GenerateFile();

                        await context.Response.WriteAsync(code);
                    }));
                }
            });

            return app;
        }

        private static IApplicationBuilder MapAngularClients(this IApplicationBuilder app, SwaggerDocumentationOptions options)
        {
            app.Map(new PathString("/clients/angular"), apiClients =>
            {
                var apiVersions = options
                    .ApiVersionDescriptionProvider
                    .ApiVersionDescriptions
                    .Select(x => x.GroupName)
                    .OrderBy(x => x)
                    .ToList();

                foreach (var description in apiVersions)
                {
                    apiClients.Map(new PathString($"/{description}"), apiClient => apiClient.Run(async context =>
                    {
                        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                        var document = await SwaggerDocument.FromUrlAsync($"{baseUrl}/docs/{description}/docs.json");

                        var settings = new SwaggerToTypeScriptClientGeneratorSettings
                        {
                            ClassName = $"{options.TypeScriptClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                            Template = TypeScriptTemplate.Angular
                        };

                        var generator = new SwaggerToTypeScriptClientGenerator(document, settings);
                        var code = generator.GenerateFile();

                        await context.Response.WriteAsync(code);
                    }));
                }
            });

            return app;
        }

        private static string FirstLetterToUpperCaseOrConvertNullToEmptyString(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
