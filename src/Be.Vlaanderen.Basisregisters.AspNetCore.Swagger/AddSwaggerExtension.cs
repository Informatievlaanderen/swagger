namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.Filters;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class HeaderOperationFilter
    {
        public string ParameterName { get; }
        public string Description { get; }
        public bool Required { get; } = false;

        public HeaderOperationFilter(
            string parameterName,
            string description,
            bool required = false)
        {
            ParameterName = parameterName;
            Description = description;
            Required = required;
        }
    }

    public class SwaggerOptions
    {
        /// <summary>
        /// Function which returns global metadata to be included in the Swagger output.
        /// </summary>
        public Func<IApiVersionDescriptionProvider, ApiVersionDescription, Info> ApiInfoFunc { get; set; }
            = null;

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files.
        /// A list of absolute paths to the files that contains XML Comments.
        /// </summary>
        public IEnumerable<string> XmlCommentPaths { get; set; }
            = new string[0];

        /// <summary>
        /// Easily add additional header parameters to each request.
        /// </summary>
        public IEnumerable<HeaderOperationFilter> AdditionalHeaderOperationFilters { get; set; }
            = new List<HeaderOperationFilter>();

        /// <summary>
        /// Hook in additional options at various stages.
        /// </summary>
        public MiddlewareHookOptions MiddlewareHooks { get; } = new MiddlewareHookOptions();

        public class MiddlewareHookOptions
        {
            public Action<SwaggerGenOptions> AfterSwaggerGen { get; set; }
        }
    }

    /// <summary>
    /// Configure Swagger schema generation.
    /// </summary>
    public static class Swagger
    {
        public static IServiceCollection AddSwagger<T>(
            this IServiceCollection services,
            SwaggerOptions options)
        {
            if (options.ApiInfoFunc == null)
                throw new ArgumentNullException(nameof(options.ApiInfoFunc));

            if (options.XmlCommentPaths == null)
                options.XmlCommentPaths = new string[0];

            services
                .AddSwaggerExamplesFromAssemblyOf<T>()
                .AddSwaggerGen(x =>
                {
                    x.DescribeAllEnumsAsStrings();
                    x.DescribeStringEnumsInCamelCase();
                    x.DescribeAllParametersInCamelCase();

                    foreach (var xmlCommentPath in options.XmlCommentPaths)
                        AddXmlComments<T>(x, xmlCommentPath);

                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                        x.SwaggerDoc(description.GroupName, options.ApiInfoFunc(provider, description));

                    x.ExampleFilters(); // [SwaggerRequestExample] & [SwaggerResponseExample]

                    x.SchemaFilter<AutoRestSchemaFilter>();

                    x.OperationFilter<SwaggerDefaultValues>();
                    x.OperationFilter<DescriptionOperationFilter>(); // [Description] on Response properties
                    x.OperationFilter<AddFileParamTypesOperationFilter>(); // Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
                    x.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]
                    x.OperationFilter<TagByApiExplorerSettingsOperationFilter>();

                    x.AddSecurityDefinition("oauth2", new ApiKeyScheme
                    {
                        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                        In = "header",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                    // add Security information to each operation for OAuth2
                    x.OperationFilter<SecurityRequirementsOperationFilter>();

                    //x.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                    x.OperationFilter<AuthorizationResponseOperationFilter>();
                    x.OperationFilter<AppendAuthorizeToSummaryOperationFilter>(); // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization

                    foreach (var additionalHeader in options.AdditionalHeaderOperationFilters)
                        x.OperationFilter<AddHeaderOperationFilter>(
                            additionalHeader.ParameterName,
                            additionalHeader.Description,
                            additionalHeader.Required);

                    //x.OperationFilter<AddHeaderOperationFilter>("apiKey", "Optionele API key voor het verzoek.");

                    options.MiddlewareHooks.AfterSwaggerGen?.Invoke(x);
                });

            return services;
        }

        private static void AddXmlComments<T>(SwaggerGenOptions swaggerGenOptions, string name)
        {
            var possiblePaths = new[]
            {
                CreateXmlCommentsPath(AppContext.BaseDirectory, name),
                CreateXmlCommentsPath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, name),
                CreateXmlCommentsPath(Path.GetDirectoryName(typeof(T).Assembly.Location), name),
            };

            foreach (var possiblePath in possiblePaths)
            {
                if (!File.Exists(possiblePath))
                    continue;

                swaggerGenOptions.IncludeXmlComments(possiblePath);
                return;
            }

            throw new ApplicationException(
                $"Could not find swagger xml docs. Locations where I searched:\n\t- {string.Join("\n\t-", possiblePaths)}");
        }

        private static string CreateXmlCommentsPath(string directory, string name)
            => Path.Combine(directory, $"{name}.xml");
    }
}
