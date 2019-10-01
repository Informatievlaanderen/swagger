namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    public class HeaderOperationFilter
    {
        public string ParameterName { get; }
        public string Description { get; }
        public bool Required { get; } = false;

        public HeaderOperationFilter(
            string parameterName,
            string description,
            bool required = false)
        {
            ParameterName = parameterName;
            Description = description;
            Required = required;
        }
    }
}
