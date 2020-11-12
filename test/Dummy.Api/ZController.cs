namespace Dummy.Api
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [Route("v{version:apiVersion}/x/")]
    [ApiExplorerSettings(GroupName = "X")]
    [Authorize]
    public class XController : ApiController
    {
        /// <summary>
        /// Initial entry point of the API.
        /// </summary>
        /// <remarks>Who really cares?</remarks>
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
}
