namespace Swashbuckle.AspNetCore.Filters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger;
    using global::Newtonsoft.Json;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.OpenApi.Models;
    using SwaggerGen;

    public class DescriptionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            SetResponseModelDescriptions(operation, context.SchemaRepository, context);
            SetRequestModelDescriptions(context.SchemaRepository, context.ApiDescription);
        }

        private static void SetResponseModelDescriptions(OpenApiOperation operation, SchemaRepository schemaRegistry, OperationFilterContext context)
        {
            var actionAttributes = context.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>().ToList();

            foreach (var attribute in actionAttributes)
            {
                var statusCode = attribute.StatusCode.ToString();

                var response = operation.Responses.FirstOrDefault(r => r.Key == statusCode);

                if (response.Equals(default(KeyValuePair<string, OpenApiResponse>)) == false && response.Value != null)
                    UpdateDescriptions(schemaRegistry, attribute.Type);
            }
        }

        private static void SetRequestModelDescriptions(SchemaRepository schemaRegistry, ApiDescription apiDescription)
        {
            foreach (var parameterDescription in apiDescription.ParameterDescriptions)
                if (parameterDescription.Type != null)
                    UpdateDescriptions(schemaRegistry, parameterDescription.Type);
        }

        private static void UpdateDescriptions(SchemaRepository schemaRegistry, Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                foreach (var genericArgumentType in type.GetGenericArguments())
                    UpdateDescriptions(schemaRegistry, genericArgumentType);

                return;
            }

            if (type.GetTypeInfo().IsArray)
            {
                UpdateDescriptions(schemaRegistry, type.GetElementType()!);
                return;
            }

            var schema = FindSchemaForType(schemaRegistry, type);
            if (schema == null)
                return;

            var propertiesWithDescription = type.GetProperties().Where(prop => prop.IsDefined(typeof(DescriptionAttribute), false)).ToList();
            if (!propertiesWithDescription.Any())
                return;

            foreach (var propertyInfo in propertiesWithDescription)
                UpdatePropertyDescription(propertyInfo, schema);

            var childProperties = type.GetProperties().ToList();
            foreach (var child in childProperties)
                UpdateDescriptions(schemaRegistry, child.PropertyType);
        }

        private static OpenApiSchema? FindSchemaForType(SchemaRepository schemaRegistry, Type type)
        {
            if (schemaRegistry.Schemas.ContainsKey(type.FriendlyId(false)))
                return schemaRegistry.Schemas[type.FriendlyId(false)];

            if (schemaRegistry.Schemas.ContainsKey(type.FriendlyId(true)))
                return schemaRegistry.Schemas[type.FriendlyId(true)];

            return null;
        }

        private static void UpdatePropertyDescription(PropertyInfo prop, OpenApiSchema schema)
        {
            var propName = GetPropertyName(prop);
            foreach (var schemaProperty in schema.Properties)
            {
                if (string.Equals(schemaProperty.Key, propName, StringComparison.OrdinalIgnoreCase))
                {
                    var descriptionAttribute = (DescriptionAttribute)prop.GetCustomAttributes(typeof(DescriptionAttribute), false).First();
                    schemaProperty.Value.Description = descriptionAttribute.Description;
                }
            }
        }

        private static string GetPropertyName(PropertyInfo prop)
        {
            if (prop.IsDefined(typeof(DataMemberAttribute), false))
            {
                var dataMemberAttribute = (DataMemberAttribute)prop.GetCustomAttributes(typeof(DataMemberAttribute), false).First();
                return dataMemberAttribute.Name ?? prop.Name;
            }

            if (prop.IsDefined(typeof(JsonPropertyAttribute), false))
            {
                var jsonPropertyAttribute = (JsonPropertyAttribute)prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).First();
                return jsonPropertyAttribute.PropertyName ?? prop.Name;
            }

            return prop.Name;
        }
    }
}
