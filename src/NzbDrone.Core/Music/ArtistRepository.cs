using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        bool ArtistPathExists(string path);
        Artist FindByName(string cleanTitle);
        Artist FindById(int dbId);
        Artist FindById(string spotifyId);
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

        public Artist FindById(string foreignArtistId)
        {
            return Query.Where(s => s.ForeignArtistId == foreignArtistId).SingleOrDefault();
        }

        public Artist FindById(int dbId)
        {
            return Query.Where(s => s.Id == dbId).SingleOrDefault();
        }

        public Artist FindByName(string cleanName)
        {
            cleanName = cleanName.ToLowerInvariant();

            return Query.Where(s => s.CleanName == cleanName)
                        .SingleOrDefault();
        }
    }
}
