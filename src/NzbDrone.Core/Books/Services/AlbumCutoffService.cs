using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public interface IAlbumCutoffService
    {
        PagingSpec<Book> AlbumsWhereCutoffUnmet(PagingSpec<Book> pagingSpec);
    }

    public class AlbumCutoffService : IAlbumCutoffService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IProfileService _profileService;

        public AlbumCutoffService(IAlbumRepository albumRepository, IProfileService profileService)
        {
            _albumRepository = albumRepository;
            _profileService = profileService;
        }

        public PagingSpec<Book> AlbumsWhereCutoffUnmet(PagingSpec<Book> pagingSpec)
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
