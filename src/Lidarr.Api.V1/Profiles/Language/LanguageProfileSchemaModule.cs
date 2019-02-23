using System.Linq;
using NzbDrone.Core.Profiles.Languages;
using Lidarr.Http;

namespace Lidarr.Api.V1.Profiles.Language
{
    public class LanguageProfileSchemaModule : LidarrRestModule<LanguageProfileResource>
    {
        private readonly LanguageProfileService _languageProfileService;

        public LanguageProfileSchemaModule(LanguageProfileService languageProfileService)
            : base("/languageprofile/schema")
        {
            _languageProfileService = languageProfileService;
            GetResourceSingle = GetAll;
        }

        private LanguageProfileResource GetAll()
        {
            var profile = _languageProfileService.GetDefaultProfile(string.Empty);
            return profile.ToResource();
        }
    }
}
