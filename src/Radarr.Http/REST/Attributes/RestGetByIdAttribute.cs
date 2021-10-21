using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Radarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestGetByIdAttribute : ActionFilterAttribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"OnExecuting {context.Controller.GetType()} {context.ActionDescriptor.DisplayName}");
        }

        public IEnumerable<string> HttpMethods => new[] { "GET" };
        public string Template => "{id:int}";
        public new int? Order => 0;
        public string Name { get; }
    }
}
