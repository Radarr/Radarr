using NzbDrone.Core.Configuration;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Config
{
    public class MetadataProviderConfigResource : RestResource
    {
        public string MetadataSource { get; set; }
        public WriteAudioTagsType WriteAudioTags { get; set; }
        public bool ScrubAudioTags { get; set; }
    }

    public static class MetadataProviderConfigResourceMapper
    {
        public static MetadataProviderConfigResource ToResource(IConfigService model)
        {
            return new MetadataProviderConfigResource
            {
                MetadataSource = model.MetadataSource,
                WriteAudioTags = model.WriteAudioTags,
                ScrubAudioTags = model.ScrubAudioTags,
            };
        }
    }
}
