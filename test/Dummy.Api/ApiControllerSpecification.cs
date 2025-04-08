namespace Dummy.Api
{
    using System;
    using System.Net.Mime;
    using System.Reflection;
    using Asp.Versioning.ApplicationModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class ApiControllerSpec : IApiControllerSpecification
    {
        private readonly Type _apiControllerType = typeof(ApiController).GetTypeInfo();

        public bool IsSatisfiedBy(ControllerModel controller) =>
            _apiControllerType.IsAssignableFrom(controller.ControllerType);
    }

    public abstract class ApiController : ControllerBase { }

    public static class AcceptTypes
    {
        public const string Any = "*/*";
        public const string Json = MediaTypeNames.Application.Json;
        public const string JsonLd = "application/ld+json";
        public const string Xml = MediaTypeNames.Application.Xml;
        public const string Atom = "application/atom+xml";
    }
}
