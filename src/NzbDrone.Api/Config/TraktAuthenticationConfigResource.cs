using Radarr.Http.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class TraktAuthenticationConfigResource : RestResource
    {
	    public string TraktAuthToken { get; set; }
	    public string TraktRefreshToken { get; set; }
	    public int TraktTokenExpiry { get; set; }
    }

    public static class TraktAuthenticationConfigResourceMapper
    {
        public static TraktAuthenticationConfigResource ToResource(IConfigService model)
        {
            return new TraktAuthenticationConfigResource
            {
	            TraktAuthToken = model.TraktAuthToken,
	            TraktRefreshToken = model.TraktRefreshToken,
	            TraktTokenExpiry = model.TraktTokenExpiry,
            };
        }
    }
}
