using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaItems;

namespace NzbDrone.Core.Music
{
    public interface ITrackService : IBaseMediaService<Track>
    {
        Track GetTrack(int trackId);
        List<Track> GetTracks(IEnumerable<int> trackIds);
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

    public class TrackService : BaseMediaService<Track>, ITrackService
    {
        private readonly ITrackRepository _trackRepository;

        public TrackService(ITrackRepository trackRepository)
        {
            _trackRepository = trackRepository;
        }

        protected override IBasicRepository<Track> Repository => _trackRepository;

        public Track GetTrack(int trackId) => Get(trackId);
        public List<Track> GetTracks(IEnumerable<int> trackIds) => Get(trackIds);
        public Track AddTrack(Track newTrack) => Add(newTrack);
        public List<Track> AddTracks(List<Track> newTracks) => AddMany(newTracks);
        public void DeleteTrack(int trackId, bool deleteFiles) => Delete(trackId, deleteFiles);
        public void DeleteTracks(List<int> trackIds, bool deleteFiles) => DeleteMany(trackIds, deleteFiles);
        public List<Track> GetAllTracks() => GetAll();
        public Track UpdateTrack(Track track) => Update(track);
        public List<Track> UpdateTracks(List<Track> tracks) => UpdateMany(tracks);

        public Track FindByForeignId(string foreignTrackId) => _trackRepository.FindByForeignId(foreignTrackId);
        public List<Track> FindByAlbumId(int albumId) => _trackRepository.FindByAlbumId(albumId);
        public List<Track> GetMonitoredTracks() => _trackRepository.GetMonitored();
        public bool TrackPathExists(string path) => _trackRepository.TrackPathExists(path);
    }
}
