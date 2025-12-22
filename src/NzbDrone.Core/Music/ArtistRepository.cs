using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IArtistRepository : IBasicRepository<Artist>
    {
        Artist FindByName(string name);
        Artist FindByForeignId(string foreignArtistId);
        List<Artist> GetMonitored();
        bool ArtistPathExists(string path);
    }

    public class ArtistRepository : BasicRepository<Artist>, IArtistRepository
    {
        public ArtistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Artist FindByName(string name)
        {
            return Query(a => a.Name == name).FirstOrDefault();
        }

        public Artist FindByForeignId(string foreignArtistId)
        {
            return Query(a => a.ForeignArtistId == foreignArtistId).FirstOrDefault();
        }

        public List<Artist> GetMonitored()
        {
            return Query(a => a.Monitored);
        }

        public bool ArtistPathExists(string path)
        {
            return Query(a => a.Path == path).Any();
        }
    }
}
