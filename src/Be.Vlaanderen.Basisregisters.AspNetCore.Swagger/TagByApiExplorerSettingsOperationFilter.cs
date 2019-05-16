namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Apply [ApiExplorerSettings(GroupName=...)] property to tags.
    /// </summary>
    public class TagByApiExplorerSettingsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
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

            var tags = operation.Tags?.Select(x => x).ToList() ?? new List<string>();

            tags.Remove(controllerActionDescriptor.ControllerName);

            foreach (var apiGroupName in apiGroupNames)
                if (!tags.Contains(apiGroupName))
                    tags.Add(apiGroupName);

            operation.Tags = tags;
        }
    }
}
