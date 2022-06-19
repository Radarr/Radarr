using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Authentication
{
    [AllowAnonymous]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationController(IAuthenticationService authService, IConfigFileProvider configFileProvider)
        {
            _authService = authService;
            _configFileProvider = configFileProvider;
        }

        [HttpPost("login")]
        public Task LoginLogin([FromForm] LoginResource resource, [FromQuery] string returnUrl = "/")
        {
            if (_configFileProvider.AuthenticationMethod == AuthenticationType.Forms)
            {
                return LoginForms(resource, returnUrl);
            }

            return LoginSso(resource, returnUrl);
        }

        private async Task LoginForms(LoginResource resource, string returnUrl)
        {
            var user = _authService.Login(HttpContext.Request, resource.Username, resource.Password);

            if (user == null)
            {
                await HttpContext.ForbidAsync(AuthenticationType.Forms.ToString());
                return;
            }

            var claims = new List<Claim>
            {
                new Claim("user", user.Username),
                new Claim("identifier", user.Identifier.ToString()),
                new Claim("AuthType", AuthenticationType.Forms.ToString())
            };

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = resource.RememberMe == "on",
                RedirectUri = returnUrl
            };

            await HttpContext.SignInAsync(AuthenticationType.Forms.ToString(), new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "identifier")), authProperties);
        }

        private async Task LoginSso(LoginResource resource, string returnUrl = "/")
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = resource.RememberMe == "on",
                RedirectUri = returnUrl
            };

            await HttpContext.ChallengeAsync(_configFileProvider.AuthenticationMethod.GetChallengeScheme(), authProperties);
        }

        [HttpGet("logout")]
        public async Task Logout()
        {
            _authService.Logout(HttpContext);

            var authType = _configFileProvider.AuthenticationMethod;
            await HttpContext.SignOutAsync(authType.ToString());

            if (authType == AuthenticationType.Oidc)
            {
                await HttpContext.SignOutAsync(authType.GetChallengeScheme());
            }
        }
    }
}
