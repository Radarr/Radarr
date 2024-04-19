using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.REST.Filters;

public class LogDatabaseDisabledActionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var configFileProvider = context.HttpContext.RequestServices.GetService<IConfigFileProvider>();
        if (!configFileProvider.LogDbEnabled)
        {
            context.Result = new NotFoundResult();
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
