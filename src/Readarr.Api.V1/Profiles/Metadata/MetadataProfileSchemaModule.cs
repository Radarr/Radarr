using NzbDrone.Core.Profiles.Metadata;
using Readarr.Http;

namespace Readarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileSchemaModule : ReadarrRestModule<MetadataProfileResource>
    {
        public MetadataProfileSchemaModule()
            : base("/metadataprofile/schema")
        {
            GetResourceSingle = GetAll;
        }

        private MetadataProfileResource GetAll()
        {
            var profile = new MetadataProfile
            {
                AllowedLanguages = "eng, en-US, en-GB"
            };

            return profile.ToResource();
        }
    }
}
