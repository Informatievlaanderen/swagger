namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Adds a 401 Unauthorized response to every action which requires authorization.
    /// </summary>
    public class AuthorizationResponseOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

            if (!isAuthorized || allowAnonymous)
                return;

            operation.Responses.Add(
                StatusCodes.Status401Unauthorized.ToString(),
                new Response { Description = "Als u onvoldoende rechten heeft." });
        }
    }
}
