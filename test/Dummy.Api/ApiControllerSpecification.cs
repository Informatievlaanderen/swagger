namespace Dummy.Api
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class ApiControllerSpec : IApiControllerSpecification
    {
        private readonly Type ApiControllerType = typeof(ApiController).GetTypeInfo();

        public bool IsSatisfiedBy(ControllerModel controller) =>
            ApiControllerType.IsAssignableFrom(controller.ControllerType);
    }

    public abstract class ApiController : ControllerBase { }
}
