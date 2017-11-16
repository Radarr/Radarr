using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public interface IAlbumCutoffService
    {
        PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec);
    }

    public class AlbumCutoffService : IAlbumCutoffService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IProfileService _profileService;
        private readonly ILanguageProfileService _languageProfileService;
        private readonly Logger _logger;

        public AlbumCutoffService(IAlbumRepository albumRepository, IProfileService profileService, ILanguageProfileService languageProfileService, Logger logger)
        {
            _albumRepository = albumRepository;
            _profileService = profileService;
            _languageProfileService = languageProfileService;
            _logger = logger;
        }

        public PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var languagesBelowCutoff = new List<LanguagesBelowCutoff>();
            var profiles = _profileService.All();
            var languageProfiles = _languageProfileService.All();

            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoffIndex = profile.GetIndex(profile.Cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            foreach (var profile in languageProfiles)
            {
                var languageCutoffIndex = profile.Languages.FindIndex(v => v.Language == profile.Cutoff);
                var belowLanguageCutoff = profile.Languages.Take(languageCutoffIndex).ToList();

                if (belowLanguageCutoff.Any())
                {
                    languagesBelowCutoff.Add(new LanguagesBelowCutoff(profile.Id, belowLanguageCutoff.Select(l => l.Language.Id)));
                }
            }

            return _albumRepository.AlbumsWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, languagesBelowCutoff);
        }
    }
}
