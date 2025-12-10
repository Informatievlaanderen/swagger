namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;

    public class ReDocIndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ReDocOptions _options;

        public ReDocIndexMiddleware(RequestDelegate next, ReDocOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            var rqf = httpContext.Request.HttpContext.Features.GetRequiredFeature<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.UICulture;

            switch (httpMethod)
            {
                // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
                case "GET" when Regex.IsMatch(path!, $"^/{_options.RoutePrefix}/?$"):
                    // Use relative redirect to support proxy environments
                    var relativeRedirectPath = path!.EndsWith("/")
                        ? "api-documentation.html"
                        : $"{path.Split('/').Last()}/api-documentation.html";

                    RespondWithRedirect(httpContext.Response, relativeRedirectPath);
                    return;

                case "GET" when Regex.IsMatch(path!, $"^/({_options.RoutePrefix}/)?api-documentation.html", RegexOptions.IgnoreCase):
                    await RespondWithIndexHtml(httpContext.Response, culture);
                    return;

                case "GET" when Regex.IsMatch(path!, $"^/({_options.RoutePrefix}/)?manifest.json", RegexOptions.IgnoreCase):
                    await RespondWithManifest(httpContext.Response, culture);
                    return;

                default:
                    await _next(httpContext);
                    break;
            }
        }

        private static void RespondWithRedirect(HttpResponse response, string redirectPath)
        {
            response.StatusCode = StatusCodes.Status301MovedPermanently;
            response.Headers[HeaderNames.Location] = redirectPath;
        }

        private async Task RespondWithIndexHtml(HttpResponse response, CultureInfo culture)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = MediaTypeNames.Text.Html;
            await RespondWithStream(response, culture, _options.IndexStream);
        }

        private async Task RespondWithManifest(HttpResponse response, CultureInfo culture)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/json";
            await RespondWithStream(response, culture, () => typeof(ReDocOptions).GetTypeInfo().Assembly
                .GetManifestResourceStream("Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc.www.manifest.json")!);
        }

        private async Task RespondWithStream(HttpResponse response, CultureInfo culture, Func<Stream> streamFunc)
        {
            using (var stream = streamFunc())
                await ReplaceTokens(response, culture, stream);
        }

        private async Task ReplaceTokens(HttpResponse response, CultureInfo culture, Stream stream)
        {
            // Inject parameters before writing to response
            using (var streamreader = new StreamReader(stream))
            {
                var responseBuilder = new StringBuilder(streamreader.ReadToEnd());

                foreach (var entry in GetIndexParameters(culture))
                    responseBuilder.Replace(entry.Key, entry.Value);

                await response.WriteAsync(responseBuilder.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexParameters(CultureInfo culture) =>
            new Dictionary<string, string>
            {
                {"%(DocumentTitle)", _options.DocumentTitle},
                {"%(DocumentDescription)", _options.DocumentDescription},
                {"%(ApplicationName)", _options.ApplicationName},
                {"%(HeaderTitle)", _options.HeaderTitle},
                {"%(HeaderLink)", _options.HeaderLink},
                {"%(HeadContent)", _options.HeadContent},
                {"%(FooterVersion)", string.IsNullOrWhiteSpace(_options.FooterVersion) ? "x.x.x" : _options.FooterVersion},
                {"%(SpecUrl)", $"{_options.SpecUrl}?culture={culture.IetfLanguageTag}"},
                {"%(Options)", SerializeToJson(_options.Options)},
                {"%(VersionSelector)", BuildVersionSelectorHtml()}
            };

        private string BuildVersionSelectorHtml()
        {
            var versions = _options.AvailableVersions?.ToList();
            if (versions == null || versions.Count <= 1)
                return string.Empty;

            var optionsHtml = new StringBuilder();
            foreach (var version in versions)
            {
                var selected = version == _options.CurrentVersion ? " selected" : "";
                optionsHtml.Append($"<option value=\"{version}\"{selected}>{version}</option>");
            }

            return $@"
                    <label for=""api-version"">API Version:</label>
                    <select id=""api-version"" onchange=""window.location.href='{_options.VersionRouteBase}/' + (this.value === '{versions.FirstOrDefault()}' ? '' : this.value + '/')"">
                        {optionsHtml}
                    </select>";
        }

        private static string SerializeToJson(object obj) =>
            JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.None
                });
    }
}
