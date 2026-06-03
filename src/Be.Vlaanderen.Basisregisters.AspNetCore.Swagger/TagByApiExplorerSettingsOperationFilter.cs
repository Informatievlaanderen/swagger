namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Apply [ApiExplorerSettings(GroupName=...)] property to tags.
    /// </summary>
    public class TagByApiExplorerSettingsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                return;

            var apiGroupNames = controllerActionDescriptor
                .ControllerTypeInfo
                .GetCustomAttributes<ApiExplorerSettingsAttribute>(true)
                .Where(x => !x.IgnoreApi)
                .Select(x => x.GroupName)
                .ToList();

            if (apiGroupNames.Count == 0)
                return;

            var tags = operation.Tags?.Select(x => x).ToList() ?? new List<OpenApiTagReference>();
            var controllerName = controllerActionDescriptor.ControllerName;
            var controllerClassName = controllerActionDescriptor.ControllerTypeInfo.Name;

            tags.RemoveAll(x =>
                string.Equals(x.Name, controllerName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name, controllerClassName, StringComparison.OrdinalIgnoreCase));

            foreach (var apiGroupName in apiGroupNames)
                if (tags.All(x => !string.Equals(x.Name, apiGroupName, StringComparison.Ordinal)))
                    tags.Add(new OpenApiTagReference(apiGroupName));

            operation.Tags = tags.ToHashSet();
        }
    }
}
