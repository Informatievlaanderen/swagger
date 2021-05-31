namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Filters;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SwaggerOptions
    {
        /// <summary>
        /// Function which returns global metadata to be included in the Swagger output.
        /// </summary>
        public Func<IApiVersionDescriptionProvider, ApiVersionDescription, OpenApiInfo> ApiInfoFunc { get; set; }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files.
        /// A list of absolute paths to the files that contains XML Comments.
        /// </summary>
        public IEnumerable<string> XmlCommentPaths { get; set; }
            = Array.Empty<string>();

        /// <summary>
        /// Easily add additional header parameters to each request.
        /// </summary>
        public IEnumerable<HeaderOperationFilter> AdditionalHeaderOperationFilters { get; set; }
            = new List<HeaderOperationFilter>();

        /// <summary>
        /// Available servers.
        /// </summary>
        public IEnumerable<Server> Servers { get; set; }
            = new List<Server>();

        public Func<ApiDescription, string> CustomSortFunc { get; set; } = SortByTag.Sort;

        /// <summary>
        /// Hook in additional options at various stages.
        /// </summary>
        public MiddlewareHookOptions MiddlewareHooks { get; } = new();

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
                    x.DescribeAllParametersInCamelCase();

                    foreach (var xmlCommentPath in options.XmlCommentPaths)
                        AddXmlComments<T>(x, xmlCommentPath);

                    var serviceProvider = services.BuildServiceProvider();
                    var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                        x.SwaggerDoc(description.GroupName, options.ApiInfoFunc(provider, description));

                    // Apply [SwaggerRequestExample] & [SwaggerResponseExample]
                    x.ExampleFilters();

                    // Add an AutoRest vendor extension (see https://github.com/Azure/autorest/blob/master/docs/extensions/readme.md#x-ms-enum) to inform the AutoRest tool how enums should be modelled when it generates the API client.
                    x.SchemaFilter<AutoRestSchemaFilter>();

                    // Fix up some Swagger parameter values by discovering them from ModelMetadata and RouteInfo
                    x.OperationFilter<SwaggerDefaultValues>();

                    // Apply [Description] on Response properties
                    x.OperationFilter<DescriptionOperationFilter>();

                    // Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
                    //x.OperationFilter<AddFileParamTypesOperationFilter>(); Marked AddFileParamTypesOperationFilter as Obsolete, because Swashbuckle 4.0 supports IFormFile directly.

                    // Apply [SwaggerResponseHeader] to headers
                    x.OperationFilter<AddResponseHeadersFilter>();

                    // Apply [ApiExplorerSettings(GroupName=...)] property to tags.
                    x.OperationFilter<TagByApiExplorerSettingsOperationFilter>();

                    //x.AddSecurityDefinition("oauth2", new ApiKeyScheme
                    //{
                    //    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    //    In = "header",
                    //    Name = "Authorization",
                    //});

                    //x.AddSecurityDefinition("apiKey", new ApiKeyScheme
                    //{
                    //    Description = "Standard authorization using an API Key.",
                    //    In = "query",
                    //    Name = "apiKey",
                    //});

                    //x.OperationFilter<SecurityRequirementsOperationFilter>(false, "oauth2");
                    //x.OperationFilter<SecurityRequirementsOperationFilter>(false, "apiKey");

                    // Add server support for ReDoc
                    if (options.Servers != null && options.Servers.Any())
                        x.DocumentFilter<AlternateServersFilter>(options.Servers);

                    // Adds a 401 Unauthorized and 403 Forbidden response to every action which requires authorization
                    x.OperationFilter<AuthorizationResponseOperationFilter>();

                    // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                    x.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                    // Add additional global header parameters
                    foreach (var additionalHeader in options.AdditionalHeaderOperationFilters)
                        x.OperationFilter<AddHeaderOperationFilter>(
                            additionalHeader.ParameterName,
                            additionalHeader.Description,
                            additionalHeader.Required);

                    // Order actions
                    if (options.CustomSortFunc != null)
                        x.OrderActionsBy(options.CustomSortFunc);

                    x.DocInclusionPredicate((_, _) => true);

                    options.MiddlewareHooks.AfterSwaggerGen?.Invoke(x);
                })
                .AddSwaggerGenNewtonsoftSupport();

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

    public static class SortByTag
    {
        public static string Sort(ApiDescription desc)
        {
            if (!(desc.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                return string.Empty;

            var apiGroupNames = controllerActionDescriptor
                .ControllerTypeInfo
                .GetCustomAttributes<ApiExplorerSettingsAttribute>(true)
                .Where(x => !x.IgnoreApi)
                .Select(x => x.GroupName)
                .ToList();

            if (apiGroupNames.Count == 0)
                return string.Empty;

            var tags = new StringBuilder();
            foreach (var apiGroupName in apiGroupNames)
                tags.Append(apiGroupName);

            return tags.ToString();
        }
    }
}
