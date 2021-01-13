namespace Dummy.Api
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.OpenApi.Models;
    using SwaggerOptions = Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.SwaggerOptions;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiControllerSpecification, ApiControllerSpec>());

            services
                .AddLocalization()

                .AddCors(options => options.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()))

                .AddMvcCore(x => x.EnableEndpointRouting = false)
                .AddXmlDataContractSerializerFormatters()

                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)

                .AddNewtonsoftJson(cfg => cfg.SerializerSettings.ConfigureDefaultForApi())

                .AddDataAnnotationsLocalization()

                .AddApiExplorer()

                .Services

                .AddVersionedApiExplorer(cfg =>
                {
                    cfg.GroupNameFormat = "'v'VVV";
                    cfg.SubstituteApiVersionInUrl = true;
                })

                .AddApiVersioning(cfg =>
                {
                    cfg.ReportApiVersions = true;
                    //cfg.UseApiBehavior = false;
                })

                // These settings are required for Swagger
                .AddSwagger<Startup>(new SwaggerOptions
                {
                    ApiInfoFunc = (provider, description) => new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = "Example API",
                        Description = GetApiLeadingText(description),
                        Contact = new OpenApiContact
                        {
                            Name = "agentschap Informatie Vlaanderen",
                            Email = "informatie.vlaanderen@vlaanderen.be",
                            Url = new Uri("https://vlaanderen.be/informatie-vlaanderen")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "European Union Public Licence (EUPL)",
                            Url = new Uri("https://joinup.ec.europa.eu/news/understanding-eupl-v12")
                        }
                    },

                    Servers = new []
                    {
                        new Server("https://api.example.com/", "Production")
                    },

                    XmlCommentPaths = new [] { typeof(Startup).GetTypeInfo().Assembly.GetName().Name },

                    CustomSortFunc = SortByApiOrder.Sort

                    //AdditionalHeaderOperationFilters = new List<HeaderOperationFilter>
                    //{
                    //    new HeaderOperationFilter("apiKey", "Optionele API key voor het verzoek.")
                    //}
                });
        }

        public void Configure(
            IApplicationBuilder app,
            IApiVersionDescriptionProvider apiVersionProvider)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("nl")
            };

            var version = Assembly.GetEntryAssembly().GetName().Version;

            app
                .UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("en"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                })

                .UseCors()

                // These settings are required for ReDoc
                .UseSwaggerDocumentation(new SwaggerDocumentationOptions
                {
                    ApiVersionDescriptionProvider = apiVersionProvider,
                    DocumentTitleFunc = groupName => $"Example API {groupName}",
                    DocumentDescriptionFunc = groupName => $"This contains all the documentation for Example API {groupName}",
                    ApplicationNameFunc = _ => "Example API",
                    HeaderTitleFunc = groupName => "Example API",
                    HeaderLinkFunc = groupName => "/docs/",
                    FooterVersion = $"{version.Minor}.{version.Build}.{version.Revision}",
                    CSharpClient =
                    {
                        ClassName = "ExampleApi",
                        Namespace = "Be.Vlaanderen.Apis"
                    },
                    TypeScriptClient =
                    {
                        ClassName = "ExampleApi",
                    }
                })

                .UseMvc();
        }

        private static string GetApiLeadingText(ApiVersionDescription description)
            => $"Right now you are reading the documentation for version {description.ApiVersion} of the Example API{string.Format(description.IsDeprecated ? ", **this API version is not supported any more**." : ".")}";
    }
}
