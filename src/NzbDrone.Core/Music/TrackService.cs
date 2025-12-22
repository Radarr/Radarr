using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public interface ITrackService
    {
        Track GetTrack(int trackId);
        List<Track> GetTracks(IEnumerable<int> trackIds);
        PagingSpec<Track> Paged(PagingSpec<Track> pagingSpec);
        Track AddTrack(Track newTrack);
        List<Track> AddTracks(List<Track> newTracks);
        Track FindByForeignId(string foreignTrackId);
        List<Track> FindByAlbumId(int albumId);
        void DeleteTrack(int trackId, bool deleteFiles);
        void DeleteTracks(List<int> trackIds, bool deleteFiles);
        List<Track> GetAllTracks();
        List<Track> GetMonitoredTracks();
        Track UpdateTrack(Track track);
        List<Track> UpdateTracks(List<Track> tracks);
        bool TrackPathExists(string path);
    }

    public class TrackService : ITrackService
    {
        private readonly ITrackRepository _trackRepository;

        public TrackService(ITrackRepository trackRepository)
        {
            _trackRepository = trackRepository;
        }

        public Track GetTrack(int trackId)
        {
            return _trackRepository.Get(trackId);
        }

        public List<Track> GetTracks(IEnumerable<int> trackIds)
        {
            return _trackRepository.Get(trackIds).ToList();
        }

        public PagingSpec<Track> Paged(PagingSpec<Track> pagingSpec)
        {
            return _trackRepository.GetPaged(pagingSpec);
        }

        public Track AddTrack(Track newTrack)
        {
            newTrack.Added = DateTime.UtcNow;
            return _trackRepository.Insert(newTrack);
        }

        public List<Track> AddTracks(List<Track> newTracks)
        {
            var now = DateTime.UtcNow;
            foreach (var track in newTracks)
            {
                track.Added = now;
            }

            _trackRepository.InsertMany(newTracks);
            return newTracks;
        }

        public Track FindByForeignId(string foreignTrackId)
        {
            return _trackRepository.FindByForeignId(foreignTrackId);
        }

        public List<Track> FindByAlbumId(int albumId)
        {
            return _trackRepository.FindByAlbumId(albumId);
        }

        public void DeleteTrack(int trackId, bool deleteFiles)
        {
            _trackRepository.Delete(trackId);
        }

        public void DeleteTracks(List<int> trackIds, bool deleteFiles)
        {
            _trackRepository.DeleteMany(trackIds);
        }

        public List<Track> GetAllTracks()
        {
            return _trackRepository.All().ToList();
        }

        public List<Track> GetMonitoredTracks()
        {
            return _trackRepository.GetMonitored();
        }

        public Track UpdateTrack(Track track)
        {
            return _trackRepository.Update(track);
        }

        public List<Track> UpdateTracks(List<Track> tracks)
        {
            _trackRepository.UpdateMany(tracks);
            return tracks;
        }

        public bool TrackPathExists(string path)
        {
            return _trackRepository.TrackPathExists(path);
        }
    }
}
