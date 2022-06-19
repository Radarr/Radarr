using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using Radarr.Http;
using Radarr.Http.Authentication.Plex;

namespace Radarr.Api.V4.Authentication
{
    [V4ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IPlexTvService _plex;
        private readonly IConfigService _configService;
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationController(IPlexTvService plex, IConfigService configService, IConfigFileProvider configFileProvider)
        {
            _plex = plex;
            _configService = configService;
            _configFileProvider = configFileProvider;
        }

        [HttpGet("plex/resources")]
        public List<PlexTvResource> GetResources(string accessToken)
        {
            return _plex.GetResources(accessToken);
        }

        [HttpGet("cookie")]
        public async Task<IActionResult> GetCookie()
        {
            var authType = _configFileProvider.AuthenticationMethod;

            var claims = new List<Claim>
            {
                new Claim("user", "Anonymous"),
                new Claim("AuthType", authType.ToString())
            };

            if (authType == NzbDrone.Core.Authentication.AuthenticationType.Plex)
            {
                var claimType = _configService.PlexRequireOwner ? PlexConstants.ServerOwnedClaim : PlexConstants.ServerAccessClaim;
                claims.Add(new Claim(claimType, _configService.PlexAuthServer));
            }

            var properties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(authType.ToString(), new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "identifier")), properties);

            return StatusCode((int)HttpStatusCode.OK);
        }
    }
}
