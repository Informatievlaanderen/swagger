namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Add an Authorization bearer token field to every action which requires authorization.
    /// </summary>
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

            if (!isAuthorized || allowAnonymous)
                return;

            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            // TODO: Nadat OpenID Connect is geimplementeerd, vervangen door https://github.com/domaindrivendev/Swashbuckle.AspNetCore#add-security-definitions-and-requirements

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "Authorization",
                In = "header",
                Description = "bearer token",
                Required = true,
                Type = "string"
            });
        }
    }
}
