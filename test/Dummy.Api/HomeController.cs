namespace Dummy.Api
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [Route("v{version:apiVersion}/")]
    [ApiExplorerSettings(GroupName = "Home")]
    [Authorize]
    public class HomeController : ApiController
    {
        /// <summary>
        /// Initial entry point of the API.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HomeResponse), StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(HomeResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public IActionResult GetHome() => Ok(new HomeResponse());

        /// <summary>
        /// Some protected action.
        /// </summary>
        [HttpPost]
        //[Authorize]
        public IActionResult RegisterUser() => Ok("Ok!");
    }

    [DataContract(Name = "Home", Namespace = "")]
    public class HomeResponse
    {
        /// <summary>Current API version.</summary>
        [DataMember(Name = "Version")]
        public int Version { get; set; }

        public HomeResponse() => Version = new Random().Next(10, 200);
    }

    public class HomeResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new HomeResponse();
    }
}
