namespace Dummy.Api
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [Route("v{version:apiVersion}/")]
    [ApiExplorerSettings(GroupName = "Home")]
    [Authorize]
    [Produces(AcceptTypes.Json, AcceptTypes.JsonLd, AcceptTypes.Xml)]
    public class HomeController : ApiController
    {
        /// <summary>
        /// Initial entry point of the API.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HomeResponse), StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(HomeResponseExamples))]
        public IActionResult GetHome() => Ok(new HomeResponse());

        /// <summary>
        /// Some protected action.
        /// </summary>
        [HttpPost]
        //[Authorize]
        public IActionResult RegisterUser() => Ok("Ok!");
    }

    public enum HomeEnum
    {
        Foo,
        Bar
    }

    [DataContract(Name = "Home", Namespace = "")]
    public class HomeResponse
    {
        /// <summary>Current API version.</summary>
        [DataMember(Name = "Version")]
        public int Version { get; set; }

        [DataMember(Name = "Enum")]
        [Description("Bla bla")]
        public HomeEnum E { get; set; }

        [DataMember(Name = "GeometrieMethode")]
        public PositieGeometrieMethode GeometrieMethode { get; set; } = PositieGeometrieMethode.Geinterpoleerd;

        public HomeResponse() => Version = new Random().Next(10, 200);
    }

    public class HomeResponseExamples : IExamplesProvider<HomeResponse>
    {
        public HomeResponse GetExamples() => new HomeResponse();
    }
}
