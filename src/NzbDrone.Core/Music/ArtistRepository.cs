using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        bool ArtistPathExists(string path);
        Artist FindByName(string cleanTitle);
        Artist FindByItunesId(int iTunesId);
    }

    public class ArtistRepository : BasicRepository<Artist>, IArtistRepository
    {
        public ArtistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
       

        public bool ArtistPathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Artist FindByItunesId(int iTunesId)
        {
            return Query.Where(s => s.ItunesId == iTunesId).SingleOrDefault();
        }

        public Artist FindByName(string cleanName)
        {
            cleanName = cleanName.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanName)
                        .SingleOrDefault();
        }
    }
}
