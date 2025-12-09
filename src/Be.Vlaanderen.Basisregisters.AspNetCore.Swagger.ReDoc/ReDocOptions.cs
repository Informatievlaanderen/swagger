namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class ReDocOptions
    {
        /// <summary>
        /// Gets or sets a route prefix for accessing the ReDoc page.
        /// </summary>
        public string RoutePrefix { get; set; } = "api-docs";

        /// <summary>
        /// Gets or sets a Stream function for retrieving the ReDoc page.
        /// </summary>
        public Func<Stream> IndexStream { get; set; } = () => typeof(ReDocOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc.www.api-documentation.html")!;

        /// <summary>
        /// Gets or sets a title for the ReDoc page.
        /// This is used in the &lt;title&gt; tag.
        /// </summary>
        public string DocumentTitle { get; set; } = "API Documentation";

        /// <summary>
        /// Gets or sets a description for the ReDoc page.
        /// This is used in the &lt;meta name="description"&gt; tag.
        /// </summary>
        public string DocumentDescription { get; set; } = "An overview of the available API methods.";

        /// <summary>
        /// Gets or sets an application name for the ReDoc page.
        /// This is used in the &lt;apple-mobile-web-app-title&gt; and &lt;application-name&gt; tag.
        /// </summary>
        public string ApplicationName { get; set; } = "API Documentation";

        /// <summary>
        /// Gets or sets a header title for the ReDoc page.
        /// This is visible on the page.
        /// </summary>
        public string HeaderTitle { get; set; } = "API Documentation";

        /// <summary>
        /// Gets or sets a header link for the ReDoc page.
        /// This is visible on the page in conjunction with HeaderTitle.
        /// </summary>
        public string HeaderLink { get; set; } = "/";

        /// <summary>
        /// Gets or sets additional content to place in the head of the ReDoc page.
        /// This is used in the &lt;head&gt; tag.
        /// </summary>
        public string HeadContent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version to display in the footer.
        /// This is visible on the page.
        /// </summary>
        public string FooterVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current API version being displayed.
        /// </summary>
        public string CurrentVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of available API versions for the version selector.
        /// </summary>
        public IEnumerable<string> AvailableVersions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the base route for version navigation (e.g., "/docs").
        /// </summary>
        public string VersionRouteBase { get; set; } = "/docs";

        /// <summary>
        /// Gets or sets the Swagger JSON endpoint. Can be fully-qualified or relative to the ReDoc page.
        /// </summary>
        public string SpecUrl { get; set; } = "v1/swagger.json";

        /// <summary>
        /// Gets or sets an "options" object that is serialized to JSON and passed to Redoc.init
        /// See https://github.com/Rebilly/ReDoc/tree/v1.22.0#advanced-usage for supported options
        /// </summary>
        public object Options { get; set; } = new
        {
            scrollYOffset = "#vlaanderen-top",
            noAutoAuth = true,
            lazyRendering = true,
            menuToggle = true,
            hideLoading = true
        };
    }
}
