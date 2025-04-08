namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Localization;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    internal static class OperationFilterContextExtensions
    {
        public static IEnumerable<T> GetControllerAndActionAttributes<T>(
            this OperationFilterContext context)
            where T : Attribute
        {
            var customAttributes1 = context.MethodInfo.DeclaringType?.GetTypeInfo().GetCustomAttributes<T>();
            var customAttributes2 = context.MethodInfo.GetCustomAttributes<T>();
            var objList = new List<T>();
            if (customAttributes1 != null)
                objList.AddRange(customAttributes1);
            
            objList.AddRange(customAttributes2);
            return objList;
        }
    }

    /// <summary>
    /// Adds a 401 Unauthorized and 403 Forbidden response to every action which requires authorization.
    /// </summary>
    public class AuthorizationResponseOperationFilter : IOperationFilter
    {
        private readonly AuthorizationResponseOperationFilter<AuthorizeAttribute> _filter;

        public AuthorizationResponseOperationFilter(IStringLocalizer<Localization> stringLocalizer)
            => _filter = new AuthorizationResponseOperationFilter<AuthorizeAttribute>(stringLocalizer);

        public void Apply(OpenApiOperation operation, OperationFilterContext context) => _filter.Apply(operation, context);
    }

    public class AuthorizationResponseOperationFilter<T> : IOperationFilter where T : Attribute
    {
        private readonly IStringLocalizer<Localization> _stringLocalizer;

        public AuthorizationResponseOperationFilter(IStringLocalizer<Localization> stringLocalizer)
            => _stringLocalizer = stringLocalizer;

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.GetControllerAndActionAttributes<AllowAnonymousAttribute>().Any())
                return;

            var actionAttributes = context.GetControllerAndActionAttributes<T>().ToList();
            if (!actionAttributes.Any())
                return;

            var unauthorized = StatusCodes.Status401Unauthorized.ToString();
            var forbidden = StatusCodes.Status403Forbidden.ToString();

            operation.Responses.Add(unauthorized, new OpenApiResponse
            {
                Description = _stringLocalizer["Unauthorized request. This may be because the request to the service has not been properly authenticated."]
            });

            operation.Responses.Add(forbidden, new OpenApiResponse
            {
                Description = _stringLocalizer["Forbidden request. This may be because the credentials are incorrect for the resource requested."]
            });
        }
    }
}
