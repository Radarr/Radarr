using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Profiles;
using Radarr.Http;

namespace Radarr.Api.V4.Profiles.Quality
{
    [V4ApiController("qualityprofile/schema")]
    public class QualityProfileSchemaController : Controller
    {
        private readonly IProfileService _profileService;

        public QualityProfileSchemaController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public QualityProfileResource GetSchema()
        {
            var qualityProfile = _profileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
