using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface ITrackRepository : IBasicRepository<Track>
    {
        Track FindByForeignId(string foreignTrackId);
        List<Track> FindByAlbumId(int albumId);
        List<Track> GetMonitored();
        bool TrackPathExists(string path);
    }

    public class TrackRepository : BasicRepository<Track>, ITrackRepository
    {
        public TrackRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Track FindByForeignId(string foreignTrackId)
        {
            return Query(t => t.ForeignTrackId == foreignTrackId).FirstOrDefault();
        }

        public List<Track> FindByAlbumId(int albumId)
        {
            return Query(t => t.AlbumId == albumId);
        }

        public List<Track> GetMonitored()
        {
            return Query(t => t.Monitored);
        }

        public bool TrackPathExists(string path)
        {
            return Query(t => t.Path == path).Any();
        }
    }
}
