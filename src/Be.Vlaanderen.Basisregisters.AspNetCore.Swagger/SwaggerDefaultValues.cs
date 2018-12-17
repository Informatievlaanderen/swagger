namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    using System;
    using System.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters.OfType<NonBodyParameter>())
            {
                var description = context
                    .ApiDescription
                    .ParameterDescriptions
                    .First(p => string.Equals(p.Name, parameter.Name, StringComparison.InvariantCultureIgnoreCase));

                if (parameter.Description == null)
                    parameter.Description = description.ModelMetadata?.Description;

                if (parameter.Default == null)
                    parameter.Default = description.RouteInfo?.DefaultValue;

                if (description.RouteInfo != null)
                    parameter.Required |= !description.RouteInfo.IsOptional;
            }
        }
    }
}
