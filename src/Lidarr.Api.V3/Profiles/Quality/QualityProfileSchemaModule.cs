using System.Linq;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using Lidarr.Http;

namespace Lidarr.Api.V3.Profiles.Quality
{
    public class QualityProfileSchemaModule : LidarrRestModule<QualityProfileResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityProfileSchemaModule(IQualityDefinitionService qualityDefinitionService)
            : base("/qualityprofile/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceSingle = GetSchema;
        }

        private QualityProfileResource GetSchema()
        {
            var items = _qualityDefinitionService.All()
                .OrderBy(v => v.Weight)
                .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = false })
                .ToList();

            var qualityProfile = new Profile();
            qualityProfile.Cutoff = NzbDrone.Core.Qualities.Quality.Unknown;
            qualityProfile.Items = items;

            return qualityProfile.ToResource();
        }
    }
}