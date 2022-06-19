using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using Radarr.Http;

namespace Radarr.Api.V4.Authentication
{
    [V4ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IPlexTvService _plex;

        public AuthenticationController(IPlexTvService plex)
        {
            _plex = plex;
        }

        [HttpGet("plex/resources")]
        public List<PlexTvResource> GetResources(string accessToken)
        {
            return _plex.GetResources(accessToken);
        }
    }
}
