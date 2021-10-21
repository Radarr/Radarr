using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using Radarr.Http.Frontend.Mappers;

namespace Radarr.Http.Frontend
{
    [Authorize(Policy="UI")]
    [ApiController]
    public class StaticResourceController : Controller
    {
        private readonly string _urlBase;
        private readonly string _loginPath;
        private readonly IEnumerable<IMapHttpRequestsToDisk> _requestMappers;
        private readonly Logger _logger;

        public StaticResourceController(IConfigFileProvider configFileProvider,
            IAppFolderInfo appFolderInfo,
            IEnumerable<IMapHttpRequestsToDisk> requestMappers,
            Logger logger)
        {
            _urlBase = configFileProvider.UrlBase.Trim('/');
            _requestMappers = requestMappers;
            _logger = logger;

            _loginPath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "login.html");
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return PhysicalFile(_loginPath, "text/html");
        }

        [EnableCors("AllowGet")]
        [AllowAnonymous]
        [HttpGet("/content/{**path:regex(^(?!api/).*)}")]
        public IActionResult IndexContent([FromRoute] string path)
        {
            return MapResource("Content/" + path);
        }

        [HttpGet("")]
        [HttpGet("/{**path:regex(^(?!api/).*)}")]
        public IActionResult Index([FromRoute] string path)
        {
            return MapResource(path);
        }

        private IActionResult MapResource(string path)
        {
            path = "/" + (path ?? "");

            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                return mapper.GetResponse(path) ?? NotFound();
            }

            _logger.Warn("Couldn't find handler for {0}", path);

            return NotFound();
        }
    }
}
