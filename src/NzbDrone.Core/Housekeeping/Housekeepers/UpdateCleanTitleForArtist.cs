using System.Linq;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForArtist : IHousekeepingTask
    {
        private readonly IArtistRepository _artistRepository;

        public UpdateCleanTitleForArtist(IArtistRepository artistRepository)
        {
            _artistRepository = artistRepository;
        }

        public void Clean()
        {
            var artists = _artistRepository.All().ToList();

            artists.ForEach(s =>
            {
                s.CleanName = s.CleanName.CleanArtistName();
                _artistRepository.Update(s);
            });
        }
    }
}
