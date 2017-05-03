using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface ITrackService
    {
        Track GetTrack(int id);
        List<Track> GetTracks(IEnumerable<int> ids);
        Track FindTrack(int artistId, int albumId, int trackNumber);
        Track FindTrackByTitle(int artistId, int albumId, string releaseTitle);
        List<Track> GetTrackByArtist(int artistId);
        List<Track> GetTracksByAblum(int artistId, int albumId);
        List<Track> GetTracksByAblumTitle(int artistId, string albumTitle);
        List<Track> TracksWithFiles(int artistId);
        PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec);
        List<Track> GeTracksByFileId(int trackFileId);
        void UpdateTrack(Track track);
        void SetTrackMonitored(int trackId, bool monitored);
        void UpdateTracks(List<Track> tracks);
        void InsertMany(List<Track> tracks);
        void UpdateMany(List<Track> tracks);
        void DeleteMany(List<Track> tracks);
        void SetTrackMonitoredByAlbum(int artistId, int albumId, bool monitored);
    }

    public class TrackService : ITrackService
    {
        public void DeleteMany(List<Track> tracks)
        {
            throw new NotImplementedException();
        }

        public Track FindTrack(int artistId, int albumId, int trackNumber)
        {
            throw new NotImplementedException();
        }

        public Track FindTrackByTitle(int artistId, int albumId, string releaseTitle)
        {
            throw new NotImplementedException();
        }

        public List<Track> GeTracksByFileId(int trackFileId)
        {
            throw new NotImplementedException();
        }

        public Track GetTrack(int id)
        {
            throw new NotImplementedException();
        }

        public List<Track> GetTrackByArtist(int artistId)
        {
            throw new NotImplementedException();
        }

        public List<Track> GetTracks(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public List<Track> GetTracksByAblum(int artistId, int albumId)
        {
            throw new NotImplementedException();
        }

        public List<Track> GetTracksByAblumTitle(int artistId, string albumTitle)
        {
            throw new NotImplementedException();
        }

        public void InsertMany(List<Track> tracks)
        {
            throw new NotImplementedException();
        }

        public void SetTrackMonitored(int trackId, bool monitored)
        {
            throw new NotImplementedException();
        }

        public void SetTrackMonitoredByAlbum(int artistId, int albumId, bool monitored)
        {
            throw new NotImplementedException();
        }

        public List<Track> TracksWithFiles(int artistId)
        {
            throw new NotImplementedException();
        }

        public PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec)
        {
            throw new NotImplementedException();
        }

        public void UpdateMany(List<Track> tracks)
        {
            throw new NotImplementedException();
        }

        public void UpdateTrack(Track track)
        {
            throw new NotImplementedException();
        }

        public void UpdateTracks(List<Track> tracks)
        {
            throw new NotImplementedException();
        }
    }
}
