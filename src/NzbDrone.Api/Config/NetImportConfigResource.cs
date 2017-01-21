using NzbDrone.Api.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class NetImportConfigResource : RestResource
    {
    }

    public static class NetImportConfigResourceMapper
    {
        public static NetImportConfigResource ToResource(IConfigService model)
        {
            return new NetImportConfigResource
            {
            };
        }
    }
}
