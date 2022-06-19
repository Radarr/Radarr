using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Radarr.Http.Authentication.Plex
{
    public class PlexOptions : OAuthOptions
    {
        public PlexOptions()
        {
            CallbackPath = new PathString("/signin-plex");
            AuthorizationEndpoint = PlexDefaults.AuthorizationEndpoint;
            TokenEndpoint = PlexDefaults.TokenEndpoint;
            UserInformationEndpoint = PlexDefaults.UserInformationEndpoint;
        }

        public override void Validate()
        {
        }
    }
}
