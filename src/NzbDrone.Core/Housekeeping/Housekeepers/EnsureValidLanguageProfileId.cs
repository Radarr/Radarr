using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    // For some unknown reason series added through the v2 API can be added without a lanuage profile ID, which breaks things later.
    // This ensures there is a language profile ID and it's valid as a safety net.

    public class EnsureValidLanguageProfileId : IHousekeepingTask
    {
        private readonly IArtistRepository _artistRepository;
        private readonly ILanguageProfileService _languageProfileService;

        public EnsureValidLanguageProfileId(IArtistRepository artistRepository, ILanguageProfileService languageProfileService)
        {
            _artistRepository = artistRepository;
            _languageProfileService = languageProfileService;
        }

        public void Clean()
        {
            var languageProfiles = _languageProfileService.All();
            var firstLangaugeProfile = languageProfiles.First();
            var artists = _artistRepository.All().ToList();
            var artistToUpdate = new List<Artist>();

            artists.ForEach(s =>
            {
                if (s.LanguageProfileId == 0 || languageProfiles.None(l => l.Id == s.LanguageProfileId))
                {
                    s.LanguageProfileId = firstLangaugeProfile.Id;
                    artistToUpdate.Add(s);
                }
            });

            _artistRepository.UpdateMany(artistToUpdate);
        }
    }
}
