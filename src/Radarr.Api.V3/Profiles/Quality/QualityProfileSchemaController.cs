using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Profiles.Qualities;
using Radarr.Http;

namespace Radarr.Api.V3.Profiles.Quality
{
    [V3ApiController("qualityprofile/schema")]
    public class QualityProfileSchemaController : Controller
    {
        private readonly IQualityProfileService _qualityProfileService;

        public QualityProfileSchemaController(IQualityProfileService qualityProfileService)
        {
            _qualityProfileService = qualityProfileService;
        }

        [HttpGet]
        public QualityProfileResource GetSchema()
        {
            var qualityProfile = _qualityProfileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
