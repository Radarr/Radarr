using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Lidarr.Http;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Config
{
    public abstract class LidarrConfigModule<TResource> : LidarrRestModule<TResource> where TResource : RestResource, new()
    {
        private readonly IConfigService _configService;

        protected LidarrConfigModule(IConfigService configService)
            : this(new TResource().ResourceName.Replace("config", ""), configService)
        {
        }

        protected LidarrConfigModule(string resource, IConfigService configService) :
            base("config/" + resource.Trim('/'))
        {
            _configService = configService;

            GetResourceSingle = GetConfig;
            GetResourceById = GetConfig;
            UpdateResource = SaveConfig;
        }

        private TResource GetConfig()
        {
            var resource = ToResource(_configService);
            resource.Id = 1;

            return resource;
        }

        protected abstract TResource ToResource(IConfigService model);

        private TResource GetConfig(int id)
        {
            return GetConfig();
        }

        private void SaveConfig(TResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configService.SaveConfigDictionary(dictionary);
        }
    }
}
