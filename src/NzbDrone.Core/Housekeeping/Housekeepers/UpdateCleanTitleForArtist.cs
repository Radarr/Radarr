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
                var cleanName = s.Name.CleanArtistName();
                if (s.CleanName != cleanName)
                {
                    s.CleanName = cleanName;
                    _artistRepository.Update(s);
                }
            });
        }
    }
}
