using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IMusicFileRepository : IBasicRepository<MusicFile>
    {
        List<MusicFile> GetFilesByTrackId(int trackId);
        List<MusicFile> GetFilesByAlbumId(int albumId);
        MusicFile GetByRelativePath(string relativePath);
    }

    public class MusicFileRepository : BasicRepository<MusicFile>, IMusicFileRepository
    {
        public MusicFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<MusicFile> GetFilesByTrackId(int trackId)
        {
            return Query(f => f.TrackId == trackId);
        }

        public List<MusicFile> GetFilesByAlbumId(int albumId)
        {
            return Query(f => f.AlbumId == albumId);
        }

        public MusicFile GetByRelativePath(string relativePath)
        {
            return Query(f => f.RelativePath == relativePath).FirstOrDefault();
        }
    }
}
