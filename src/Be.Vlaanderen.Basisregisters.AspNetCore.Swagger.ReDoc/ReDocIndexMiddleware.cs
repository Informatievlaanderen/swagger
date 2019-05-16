namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
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

            var rqf = httpContext.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.UICulture;

            switch (httpMethod)
            {
                // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
                case "GET" when Regex.IsMatch(path, $"^/{_options.RoutePrefix}/?$"):
                    // Use relative redirect to support proxy environments
                    var relativeRedirectPath = path.EndsWith("/")
                        ? "api-documentation.html"
                        : $"{path.Split('/').Last()}/api-documentation.html";

                    RespondWithRedirect(httpContext.Response, relativeRedirectPath);
                    return;

                case "GET" when Regex.IsMatch(path, $"/{_options.RoutePrefix}/?api-documentation.html"):
                    await RespondWithIndexHtml(httpContext.Response, culture);
                    return;
            }

            await _next(httpContext);
        }

        private static void RespondWithRedirect(HttpResponse response, string redirectPath)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = redirectPath;
        }

        private async Task RespondWithIndexHtml(HttpResponse response, CultureInfo culture)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var stream = _options.IndexStream())
            {
                // Inject parameters before writing to response
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexParameters(culture))
                    htmlBuilder.Replace(entry.Key, entry.Value);

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexParameters(CultureInfo culture) =>
            new Dictionary<string, string>
            {
                {"%(DocumentTitle)", _options.DocumentTitle},
                {"%(HeadContent)", _options.HeadContent},
                {"%(SpecUrl)", $"{_options.SpecUrl}?culture={culture.IetfLanguageTag}"},
                {"%(Options)", SerializeToJson(_options.Options)}
            };

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
