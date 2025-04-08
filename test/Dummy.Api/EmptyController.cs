namespace Dummy.Api
{
    using System.Reflection;
    using Asp.Versioning;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Example API v{Assembly.GetEntryAssembly()!.GetName().Version}.");
    }

    public class EmptyResponseExamples : IExamplesProvider<object>
    {
        public object GetExamples() => new { };
    }
}
