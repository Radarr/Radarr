using NzbDrone.Core.Profiles.Qualities;
using Lidarr.Http;

namespace Lidarr.Api.V1.Profiles.Quality
{
    public class QualityProfileSchemaModule : LidarrRestModule<QualityProfileResource>
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
            QualityProfile qualityProfile = _profileService.GetDefaultProfile(string.Empty);


            return qualityProfile.ToResource();
        }
    }
}
