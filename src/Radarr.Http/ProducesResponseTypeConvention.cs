using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Radarr.Http
{
    // ProducesResponseType can't name the resource type of a generic base controller, so those
    // actions declare the status code alone. MVC fills the body type in from the return type for
    // 200 and 201 but not for the 202 the REST controllers return from updates, so do it here.
    public class ProducesResponseTypeConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            var returnType = action.ActionMethod.ReturnType;

            if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(ActionResult<>))
            {
                return;
            }

            var resourceType = returnType.GetGenericArguments()[0];

            foreach (var attribute in action.Filters.OfType<ProducesResponseTypeAttribute>().Where(a => a.Type == typeof(void)))
            {
                attribute.Type = resourceType;
            }
        }
    }
}
