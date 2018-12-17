namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.FileProviders;

    public static class ReDocBuilderExtensions
    {
        private const string EmbeddedFilesNamespace = "Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc.www";

        public static IApplicationBuilder UseReDoc(
            this IApplicationBuilder app,
            Action<ReDocOptions> setupAction)
        {
            var options = new ReDocOptions();
            setupAction?.Invoke(options);

            app
                //.UseDefaultFiles()
                .UseMiddleware<ReDocIndexMiddleware>(options)
                .UseFileServer(new FileServerOptions
                {
                    EnableDefaultFiles = true,
                    EnableDirectoryBrowsing = true,

                    DefaultFilesOptions =
                    {
                        DefaultFileNames = new List<string> { "api-documentation.html" }
                    },

                    RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                    FileProvider = new EmbeddedFileProvider(typeof(ReDocBuilderExtensions).GetTypeInfo().Assembly, EmbeddedFilesNamespace),
                });

            return app;
        }
    }
}
