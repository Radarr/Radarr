using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Radarr.Http
{
    public class ApiInfoController : Controller
    {
        [HttpGet("/api")]
        [Produces("application/json")]
        public ApiInfoResource GetApiInfo()
        {
            return new ApiInfoResource
            {
                Current = "v3",
                Deprecated = new List<string>()
            };
        }
    }
}
