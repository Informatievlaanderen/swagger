namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Linq;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Fix up some Swagger parameter values by discovering them from ModelMetadata and RouteInfo.
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters) //https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1147#issuecomment-490515950
            {
                var description = context
                    .ApiDescription
                    .ParameterDescriptions
                    .First(p => string.Equals(p.Name, parameter.Name, StringComparison.InvariantCultureIgnoreCase));

                if (parameter.Description == null)
                    parameter.Description = description.ModelMetadata?.Description;

                // https://github.com/RicoSuter/NSwag/issues/2569
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
                //if (parameter.Default == null)
                //    parameter.Default = description.RouteInfo?.DefaultValue;

                if (description.RouteInfo != null)
                    parameter.Required |= !description.RouteInfo.IsOptional;
            }
        }
    }
}
