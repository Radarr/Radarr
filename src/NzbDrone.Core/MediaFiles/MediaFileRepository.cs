using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<TrackFile>
    {
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesWithoutMediaInfo();
    }


    public class MediaFileRepository : BasicRepository<TrackFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<TrackFile> GetFilesWithoutMediaInfo()
        {
            return Query.Where(c => c.MediaInfo == null).ToList();
        }

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            return Query.Where(c => c.ArtistId == artistId).ToList();
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return Query.Where(c => c.AlbumId == albumId).ToList();
        }
    }
}