using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
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
        private readonly Logger _logger;

        public AlbumCutoffService(IAlbumRepository albumRepository, IProfileService profileService, Logger logger)
        {
            _albumRepository = albumRepository;
            _profileService = profileService;
            _logger = logger;
        }

        public PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _profileService.All();

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

            return _albumRepository.AlbumsWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }
    }
}
