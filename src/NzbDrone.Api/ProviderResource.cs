using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using Radarr.Http.ClientSchema;
using Radarr.Http.REST;

namespace NzbDrone.Api
{
    public class ProviderResource : RestResource
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string InfoLink { get; set; }
        public ProviderMessage Message { get; set; }

        public List<ProviderResource> Presets { get; set; }
    }
}
