using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using Radarr.Http;

namespace NzbDrone.Api.Profiles
{
    public class ProfileSchemaModule : RadarrRestModule<ProfileResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly ICustomFormatService _formatService;

        public ProfileSchemaModule(IQualityDefinitionService qualityDefinitionService, ICustomFormatService formatService)
            : base("/profile/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;
            _formatService = formatService;

            GetResourceAll = GetAll;
        }

        private List<ProfileResource> GetAll()
        {
            var items = _qualityDefinitionService.All()
                .OrderBy(v => v.Weight)
                .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = false })
                .ToList();

            var formatItems = _formatService.All().Select(v => new ProfileFormatItem
            {
                Format = v, Allowed = true
            }).ToList();

            formatItems.Insert(0, new ProfileFormatItem
            {
                Format = CustomFormat.None,
                Allowed = true
            });

            var profile = new Profile();
            profile.Cutoff = Quality.Unknown.Id;
            profile.Items = items;
            profile.FormatCutoff = CustomFormat.None.Id;
            profile.FormatItems = formatItems;
            profile.Language = Language.English;

            return new List<ProfileResource> { profile.ToResource() };
        }
    }
}
