using NzbDrone.Api.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class NetImportConfigResource : RestResource
    {
        public int NetImportSyncInterval { get; set; }
	public string TraktAuthToken { get; set; }
	public string TraktRefreshToken { get; set; }
	public string TraktTokenCreatedAt { get; set; }
	public string TraktTokenExpiresIn { get; set; }
    }

    public static class NetImportConfigResourceMapper
    {
        public static NetImportConfigResource ToResource(IConfigService model)
        {
            return new NetImportConfigResource
            {
                NetImportSyncInterval = model.NetImportSyncInterval,
		TraktAuthToken = model.TraktAuthToken,
		TraktRefreshToken = model.TraktRefreshToken,
		TraktTokenCreatedAt = model.TraktTokenCreatedAt,
		TraktTokenExpiresIn = model.TraktTokenExpiresIn,
            };
        }
    }
}
