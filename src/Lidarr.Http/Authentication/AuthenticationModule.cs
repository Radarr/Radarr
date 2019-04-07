using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Lidarr.Http.Authentication
{
    public class AuthenticationModule : NancyModule
    {
        private readonly IUserService _userService;
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationModule(IUserService userService, IConfigFileProvider configFileProvider)
        {
            _userService = userService;
            _configFileProvider = configFileProvider;
            Post["/login"] = x => Login(this.Bind<LoginResource>());
            Get["/logout"] = x => Logout();
        }

        private Response Login(LoginResource resource)
        {
            var username = resource.Username;
            var password = resource.Password;

            if (username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
            {
                return LoginFailed();
            }

            var user = _userService.FindUser(username, password);

            if (user == null)
            {
                return LoginFailed();
            }

            DateTime? expiry = null;

            if (resource.RememberMe)
            {
                expiry = DateTime.UtcNow.AddDays(7);
            }

            return this.LoginAndRedirect(user.Identifier, expiry, _configFileProvider.UrlBase + "/");
        }

        private Response Logout()
        {
            return this.LogoutAndRedirect(_configFileProvider.UrlBase + "/");
        }

        private Response LoginFailed()
        {
            var returnUrl = (string)Request.Query.returnUrl;
            return Context.GetRedirect($"~/login?returnUrl={returnUrl}&loginFailed=true");
        }
    }
}
