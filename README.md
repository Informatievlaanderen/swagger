# Be.Vlaanderen.Basisregisters.AspNetCore.Swagger

Swagger and ReDoc helpers.

## Goal

> Provide an easy way to configure a project to use Swagger and have a nice API documentation out of the box.

## Quick contributing guide

* Fork and clone locally.
* Build the solution with Visual Studio, `build.cmd` or `build.sh`.
* Create a topic specific branch in git. Add a nice feature in the code. Do not forget to add tests and/or docs.
* Run `build.cmd` or `build.sh` to make sure everything still compiles and all tests are still passing.
* When built, you'll find the binaries in `./dist` which you can then test with locally, to ensure the bug or feature has been successfully fixed/implemented.
* Send a Pull Request.

## Quick start

* Reference [Be.Vlaanderen.Basisregisters.AspNetCore.Swagger](https://www.nuget.org/packages/Be.Vlaanderen.Basisregisters.AspNetCore.Swagger/), [Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc](https://www.nuget.org/packages/Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc/) or both.
* Have a look at the example [Dummy.Api](https://github.com/Informatievlaanderen/swagger/tree/master/test/Dummy.Api) project.
* Browse to the root of your API.

## Swagger

### Features

* Read Swagger Request & Response examples from assembly.
* Describe enums as strings.
* Describe all enums and parameters in camel case.
* Support XML comments as documentation.
* Provide AutoRest support. ([`x-ms-enum`](https://github.com/Azure/autorest/blob/master/docs/extensions/readme.md#x-ms-enum))
* Provide GroupName as tags.
* Add 401 as possible response for endpoints which require authorization.
* Add 403 as possible response for endpoints which require authorization.
* Add `(Auth)` to endpoints which require authorization.

### Usage

```csharp
private static string GetApiLeadingText(ApiVersionDescription description)
    => $"Right now you are reading the documentation for version {description.ApiVersion} of the Example API{string.Format(description.IsDeprecated ? ", **this API version is not supported any more**." : ".")}";

public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // Configure other services here...

    // You most likely want these at least:
    // - AddMvcCore
    // - AddJsonFormatters
    // - AddJsonOptions
    // - AddApiExplorer
    // - AddVersionedApiExplorer
    // - AddApiVersioning

    services
      .AddLocalization()
      .AddSwagger<Startup>(new SwaggerOptions
      {
          ApiInfoFunc = (provider, description) => new Info
          {
              Version = description.ApiVersion.ToString(),
              Title = "Example API",
              Description = GetApiLeadingText(description),
              Contact = new Contact
              {
                  Name = "agentschap Informatie Vlaanderen",
                  Email = "informatie.vlaanderen@vlaanderen.be",
                  Url = "https://vlaanderen.be/informatie-vlaanderen"
              }
          },

          XmlCommentPaths = new [] { typeof(Startup).GetTypeInfo().Assembly.GetName().Name },

          AdditionalHeaderOperationFilters = new List<HeaderOperationFilter>
          {
              new HeaderOperationFilter("apiKey", "Optionel API key for the request.")
          }
      });
}
```

## ReDoc

### Features

* Provide version 1.19.2 of [ReDoc](https://github.com/Rebilly/ReDoc) in an embedded way.
* Provide Flemish Government branding of ReDoc.
* Provide documentation for multiple versions.
* Provide C# client code.
* Provide jQuery client code.
* Provide AngularJS client code.
* Provide Angular client code.

### Usage

```csharp
public void Configure(
    IApplicationBuilder app,
    IApiVersionDescriptionProvider apiVersionProvider)
{
    app
       .UseSwaggerDocumentation(new SwaggerDocumentationOptions
        {
            ApiVersionDescriptionProvider = apiVersionProvider,
            DocumentTitleFunc = groupName => $"Example API {groupName}"
        })

        .UseMvc();
}
```

## License

[Massachusetts Institute of Technology (MIT)](https://choosealicense.com/licenses/mit/)

## Credits

### Languages & Frameworks

* [.NET Core](https://github.com/Microsoft/dotnet/blob/master/LICENSE) - [MIT](https://choosealicense.com/licenses/mit/)
* [.NET Core Runtime](https://github.com/dotnet/coreclr/blob/master/LICENSE.TXT) - _CoreCLR is the runtime for .NET Core. It includes the garbage collector, JIT compiler, primitive data types and low-level classes._ - [MIT](https://choosealicense.com/licenses/mit/)
* [.NET Core APIs](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT) - _CoreFX is the foundational class libraries for .NET Core. It includes types for collections, file systems, console, JSON, XML, async and many others._ - [MIT](https://choosealicense.com/licenses/mit/)
* [.NET Core SDK](https://github.com/dotnet/sdk/blob/master/LICENSE.TXT) - _Core functionality needed to create .NET Core projects, that is shared between Visual Studio and CLI._ - [MIT](https://choosealicense.com/licenses/mit/)
* [.NET Core Docker](https://github.com/dotnet/dotnet-docker/blob/master/LICENSE) - _Base Docker images for working with .NET Core and the .NET Core Tools._ - [MIT](https://choosealicense.com/licenses/mit/)
* [.NET Standard definition](https://github.com/dotnet/standard/blob/master/LICENSE.TXT) - _The principles and definition of the .NET Standard._ - [MIT](https://choosealicense.com/licenses/mit/)
* [Roslyn and C#](https://github.com/dotnet/roslyn/blob/master/License.txt) - _The Roslyn .NET compiler provides C# and Visual Basic languages with rich code analysis APIs._ - [Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)
* [F#](https://github.com/fsharp/fsharp/blob/master/LICENSE) - _The F# Compiler, Core Library & Tools_ - [MIT](https://choosealicense.com/licenses/mit/)
* [F# and .NET Core](https://github.com/dotnet/netcorecli-fsc/blob/master/LICENSE) - _F# and .NET Core SDK working together._ - [MIT](https://choosealicense.com/licenses/mit/)
* [ASP.NET Core framework](https://github.com/aspnet/AspNetCore/blob/master/LICENSE.txt) - _ASP.NET Core is a cross-platform .NET framework for building modern cloud-based web applications on Windows, Mac, or Linux._ - [Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

### Libraries

* [Paket](https://fsprojects.github.io/Paket/license.html) - _A dependency manager for .NET with support for NuGet packages and Git repositories._ - [MIT](https://choosealicense.com/licenses/mit/)
* [FAKE](https://github.com/fsharp/FAKE/blob/release/next/License.txt) - _"FAKE - F# Make" is a cross platform build automation system._ - [MIT](https://choosealicense.com/licenses/mit/)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) - _Json.NET is a popular high-performance JSON framework for .NET_ - [MIT](https://choosealicense.com/licenses/mit/)
* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle/blob/master/LICENSE) - _Seamlessly adds a swagger to WebApi projects!_ - [BSD](https://choosealicense.com/licenses/bsd-3-clause/)
* [Swashbuckle Filters](https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters/blob/master/LICENSE) - _A bunch of useful filters for Swashbuckle.AspNetCore._ - [MIT](https://choosealicense.com/licenses/mit/)
* [ReDoc](https://github.com/Rebilly/ReDoc/blob/master/LICENSE) - _📘 OpenAPI/Swagger-generated API Reference Documentation._ - [MIT](https://choosealicense.com/licenses/mit/)
* [NSwag](https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md) - _The Swagger/OpenAPI toolchain for .NET, ASP.NET Core and TypeScript._ - [MIT](https://choosealicense.com/licenses/mit/)

### Tooling

* [npm](https://github.com/npm/cli/blob/latest/LICENSE) - _A package manager for JavaScript._ - [Artistic License 2.0](https://choosealicense.com/licenses/artistic-2.0/)
* [semantic-release](https://github.com/semantic-release/semantic-release/blob/master/LICENSE) - _Fully automated version management and package publishing._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/changelog](https://github.com/semantic-release/changelog/blob/master/LICENSE) - _Semantic-release plugin to create or update a changelog file._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/commit-analyzer](https://github.com/semantic-release/commit-analyzer/blob/master/LICENSE) - _Semantic-release plugin to analyze commits with conventional-changelog._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/exec](https://github.com/semantic-release/exec/blob/master/LICENSE) - _Semantic-release plugin to execute custom shell commands._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/git](https://github.com/semantic-release/git/blob/master/LICENSE) - _Semantic-release plugin to commit release assets to the project's git repository._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/npm](https://github.com/semantic-release/npm/blob/master/LICENSE) - _Semantic-release plugin to publish a npm package._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/github](https://github.com/semantic-release/github/blob/master/LICENSE) - _Semantic-release plugin to publish a GitHub release._ - [MIT](https://choosealicense.com/licenses/mit/)
* [semantic-release/release-notes-generator](https://github.com/semantic-release/release-notes-generator/blob/master/LICENSE) - _Semantic-release plugin to generate changelog content with conventional-changelog._ - [MIT](https://choosealicense.com/licenses/mit/)
* [commitlint](https://github.com/marionebl/commitlint/blob/master/license.md) - _Lint commit messages._ - [MIT](https://choosealicense.com/licenses/mit/)
* [commitizen/cz-cli](https://github.com/commitizen/cz-cli/blob/master/LICENSE) - _The commitizen command line utility._ - [MIT](https://choosealicense.com/licenses/mit/)
* [commitizen/cz-conventional-changelog](https://github.com/commitizen/cz-conventional-changelog/blob/master/LICENSE) _A commitizen adapter for the angular preset of conventional-changelog._ - [MIT](https://choosealicense.com/licenses/mit/)

### Flemish Government Libraries

* [Be.Vlaanderen.Basisregisters.Build.Pipeline](https://github.com/informatievlaanderen/build-pipeline/blob/master/LICENSE) - _Contains generic files for all Basisregisters pipelines._ - [MIT](https://choosealicense.com/licenses/mit/)
* [Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json](https://github.com/informatievlaanderen/json-serializer-settings/blob/master/LICENSEE) - _Default Json.NET serializer settings._ - [MIT](https://choosealicense.com/licenses/mit/)
