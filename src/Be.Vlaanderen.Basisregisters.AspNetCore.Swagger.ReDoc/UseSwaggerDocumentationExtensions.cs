namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Versioning.ApiExplorer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;
    using NSwag;
    using NSwag.CodeGeneration;
    using NSwag.CodeGeneration.CSharp;
    using NSwag.CodeGeneration.TypeScript;
    using OpenApiServer = Microsoft.OpenApi.Models.OpenApiServer;

    public class SwaggerDocumentationOptions
    {
        /// <summary>
        /// Defines the behavior of a provider that discovers and describes API version information within an application.
        /// </summary>
        public IApiVersionDescriptionProvider? ApiVersionDescriptionProvider { get; set; }

        /// <summary>
        /// Sets a title for the ReDoc page.
        /// This is used in the &lt;title&gt; tag.
        /// </summary>
        public Func<string, string>? DocumentTitleFunc { get; set; }

        /// <summary>
        /// Sets a description for the ReDoc page.
        /// This is used in the &lt;meta name="description"&gt; tag.
        /// </summary>
        public Func<string, string>? DocumentDescriptionFunc { get; set; }

        /// <summary>
        /// Sets an application name for the ReDoc page.
        /// This is used in the &lt;apple-mobile-web-app-title&gt; and &lt;application-name&gt; tag.
        /// </summary>
        public Func<string, string>? ApplicationNameFunc { get; set; }

        /// <summary>
        /// Sets a header title for the ReDoc page.
        /// This is visible on the page.
        /// </summary>
        public Func<string, string>? HeaderTitleFunc { get; set; }

        /// <summary>
        /// Sets a header link for the ReDoc page.
        /// This is visible on the page in conjunction with HeaderTitle.
        /// </summary>
        public Func<string, string>? HeaderLinkFunc { get; set; }

        /// <summary>
        /// Sets additional content to place in the head of the ReDoc page.
        /// This is used in the &lt;head&gt; tag.
        /// </summary>
        public Func<string, string>? HeadContentFunc { get; set; }

        /// <summary>
        /// Sets a custom RouteTemplate, you can use {documentName} placeholder.
        /// </summary>
        public string? RouteTemplate { get; set; }
        public Func<string, string>? SpecUrlFunc { get; set; }
        public Func<string, string>? RoutePrefixFunc { get; set; }

        /// <summary>
        /// Sets the version to display in the footer.
        /// This is visible on the page.
        /// </summary>
        public string? FooterVersion { get; set; }

        public CSharpClientOptions CSharpClient { get; } = new();

        public class CSharpClientOptions
        {
            public string? ClassName { get; set; }
            public string? Namespace { get; set; }
        }

        public TypeScriptClientOptions TypeScriptClient { get; } = new();

        public class TypeScriptClientOptions
        {
            public string? ClassName { get; set; }
        }
    }

    public static class UseSwaggerDocumentationExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            SwaggerDocumentationOptions options)
        {
            // A bit of a hack to give all the Funcs the default value, but still managing them in one place.
            var defaultValues = new ReDocOptions();

            if (options.ApiVersionDescriptionProvider == null)
                throw new ArgumentNullException(nameof(options.ApiVersionDescriptionProvider));

            options.DocumentTitleFunc ??= _ => defaultValues.DocumentTitle;
            options.DocumentDescriptionFunc ??= _ => defaultValues.DocumentDescription;
            options.ApplicationNameFunc ??= _ => defaultValues.ApplicationName;
            options.HeaderTitleFunc ??= _ => defaultValues.HeaderTitle;
            options.HeaderLinkFunc ??= _ => defaultValues.HeaderLink;
            options.HeadContentFunc ??= _ => defaultValues.HeadContent;

            if (string.IsNullOrWhiteSpace(options.FooterVersion))
                options.FooterVersion = defaultValues.FooterVersion;

            if (string.IsNullOrWhiteSpace(options.CSharpClient.ClassName))
                throw new ArgumentNullException(nameof(options.CSharpClient.ClassName));

            if (string.IsNullOrWhiteSpace(options.CSharpClient.Namespace))
                throw new ArgumentNullException(nameof(options.CSharpClient.Namespace));

            if (string.IsNullOrWhiteSpace(options.TypeScriptClient.ClassName))
                throw new ArgumentNullException(nameof(options.TypeScriptClient.ClassName));

            if (string.IsNullOrWhiteSpace(options.RouteTemplate))
                options.RouteTemplate = "{documentName}/docs.json";

            options.RoutePrefixFunc ??= description => description;
            options.SpecUrlFunc ??= description => $"/docs/{description}/docs.json";

            app
                .MapDocs(options);
                //TODO: remove with next breaking change
                // .MapClient(options, "csharp", GenerateCSharpCode)
                // .MapClient(options, "jquery", GeneratejQueryCode)
                // .MapClient(options, "angular", GenerateAngularCode)
                // .MapClient(options, "angularjs", GenerateAngularJsCode);

            return app;
        }

        private static IApplicationBuilder MapDocs(this IApplicationBuilder app, SwaggerDocumentationOptions options)
        {
            app.Map(new PathString("/docs"), apiDocs =>
            {
                apiDocs.UseSwagger(x =>
                {
                    x.RouteTemplate = options.RouteTemplate;
                    x.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers.Add(new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/" });
                    });
                });

                var apiVersions = GetApiVersions(options).ToList();
                var stringLocalizer = app.ApplicationServices.GetRequiredService<IStringLocalizer<Localization>>();

                for (var i = 0; i < apiVersions.Count; i++)
                {
                    var description = apiVersions[i];
                    var isDefault = i == 0; // First version gets the default (empty) route

                    apiDocs.UseReDoc(x =>
                    {
                        x.DocumentTitle = stringLocalizer[options.DocumentTitleFunc!(description)];
                        x.DocumentDescription = stringLocalizer[options.DocumentDescriptionFunc!(description)];
                        x.ApplicationName = stringLocalizer[options.ApplicationNameFunc!(description)];
                        x.HeaderTitle = stringLocalizer[options.HeaderTitleFunc!(description)];
                        x.HeaderLink = stringLocalizer[options.HeaderLinkFunc!(description)];
                        x.HeadContent = options.HeadContentFunc!(description);
                        x.FooterVersion = options.FooterVersion!;
                        x.SpecUrl = options.SpecUrlFunc!(description);
                        x.RoutePrefix = isDefault ? string.Empty : options.RoutePrefixFunc!(description);
                        // x.CurrentVersion = description;
                        // x.AvailableVersions = apiVersions;
                        // x.VersionRouteBase = $"/{options.DocsRoutePrefix}";
                    });
                }
            });

            return app;
        }

        private static IApplicationBuilder MapClient(
            this IApplicationBuilder app,
            SwaggerDocumentationOptions options,
            string language,
            Func<SwaggerDocumentationOptions, string, OpenApiDocument, string> generateCode)
            => app.Map(new PathString($"/clients/{language}"), apiClients =>
            {
                var apiVersions = GetApiVersions(options);

                foreach (var description in apiVersions)
                {
                    apiClients.Map(new PathString($"/{description}"), apiClient => apiClient.Run(async context =>
                    {
                        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

                        var document = await OpenApiDocument.FromUrlAsync($"{baseUrl}{options.SpecUrlFunc!(description)}");
                        if (document.Servers.Count > 1)
                            document.Servers.Remove(document.Servers.ElementAt(0));

                        var code = generateCode(options, description, document);

                        context.Response.ContentType = "text/plain; charset=utf-8";
                        await context.Response.WriteAsync(code);
                    }));
                }
            });

        private static string GenerateCSharpCode(
            SwaggerDocumentationOptions options,
            string description,
            OpenApiDocument document)
            => new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                ClassName = $"{options.CSharpClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                UseBaseUrl = true,
                CSharpGeneratorSettings =
                {
                    Namespace = options.CSharpClient.Namespace ?? string.Empty,
                }
            }).GenerateFile(ClientGeneratorOutputType.Full);

        private static string GenerateAngularCode(
            SwaggerDocumentationOptions options,
            string description,
            OpenApiDocument document)
            => new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                ClassName = $"{options.TypeScriptClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                Template = TypeScriptTemplate.Angular
            }).GenerateFile();

        private static string GenerateAngularJsCode(
            SwaggerDocumentationOptions options,
            string description,
            OpenApiDocument document)
            => new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                ClassName = $"{options.TypeScriptClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                Template = TypeScriptTemplate.AngularJS
            }).GenerateFile();

        private static string GeneratejQueryCode(
            SwaggerDocumentationOptions options,
            string description,
            OpenApiDocument document)
            => new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                ClassName = $"{options.TypeScriptClient.ClassName}{description.FirstLetterToUpperCaseOrConvertNullToEmptyString()}",
                Template = TypeScriptTemplate.JQueryCallbacks
            }).GenerateFile();

        private static IEnumerable<string> GetApiVersions(SwaggerDocumentationOptions options)
            => options
                .ApiVersionDescriptionProvider!
                .ApiVersionDescriptions
                .Select(x => x.ApiVersion.ToString("'v'V"))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

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
