using NzbDrone.Core.Profiles;
using Radarr.Http;

namespace Radarr.Api.V3.Profiles.Quality
{
    public class QualityProfileSchemaModule : RadarrRestModule<QualityProfileResource>
    {
        private readonly IProfileService _profileService;

        public QualityProfileSchemaModule(IProfileService profileService)
            : base("/qualityprofile/schema")
        {
            _profileService = profileService;

            GetResourceSingle = GetSchema;
        }

        private QualityProfileResource GetSchema()
        {
            var qualityProfile = _profileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
