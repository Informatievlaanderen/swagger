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

    /// <summary>
    /// Configure Swagger schema generation.
    /// </summary>
    public static class Swagger
    {
        public static IServiceCollection AddSwagger<T>(
            this IServiceCollection services,
            Func<IApiVersionDescriptionProvider, ApiVersionDescription, Info> apiInfoFunc,
            IEnumerable<string> xmlCommentPaths)
        {
            services
                .AddSwaggerExamplesFromAssemblyOf<T>()
                .AddSwaggerGen(x =>
                {
                    x.DescribeAllEnumsAsStrings();
                    x.DescribeStringEnumsInCamelCase();
                    x.DescribeAllParametersInCamelCase();

                    foreach (var xmlCommentPath in xmlCommentPaths)
                        AddXmlComments<T>(x, xmlCommentPath);

                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                        x.SwaggerDoc(description.GroupName, apiInfoFunc(provider, description));

                    x.ExampleFilters(); // [SwaggerRequestExample] & [SwaggerResponseExample]

                    x.SchemaFilter<AutoRestSchemaFilter>();

                    x.OperationFilter<SwaggerDefaultValues>();
                    x.OperationFilter<DescriptionOperationFilter>(); // [Description] on Response properties
                    x.OperationFilter<AddFileParamTypesOperationFilter>(); // Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
                    x.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]
                    x.OperationFilter<TagByApiExplorerSettingsOperationFilter>();

                    x.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                    x.OperationFilter<AuthorizationResponseOperationFilter>();
                    x.OperationFilter<AppendAuthorizeToSummaryOperationFilter>(); // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                    x.OperationFilter<AddHeaderOperationFilter>("apiKey", "Optionele API key voor het verzoek.");
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
