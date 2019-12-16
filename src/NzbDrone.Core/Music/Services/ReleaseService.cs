using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System.Collections.Generic;

namespace NzbDrone.Core.Music
{
    public interface IReleaseService
    {
        AlbumRelease GetRelease(int id);
        AlbumRelease GetReleaseByForeignReleaseId(string foreignReleaseId, bool checkRedirect = false);
        void InsertMany(List<AlbumRelease> releases);
        void UpdateMany(List<AlbumRelease> releases);
        void DeleteMany(List<AlbumRelease> releases);
        List<AlbumRelease> GetReleasesForRefresh(int albumId, IEnumerable<string> foreignReleaseIds);
        List<AlbumRelease> GetReleasesByAlbum(int releaseGroupId);
        List<AlbumRelease> GetReleasesByRecordingIds(List<string> recordingIds);
        List<AlbumRelease> SetMonitored(AlbumRelease release);
    }

    public class ReleaseService : IReleaseService,
                                  IHandle<AlbumDeletedEvent>
    {
        private readonly IReleaseRepository _releaseRepository;
        private readonly IEventAggregator _eventAggregator;

        public ReleaseService(IReleaseRepository releaseRepository,
                              IEventAggregator eventAggregator)
        {
            _releaseRepository = releaseRepository;
            _eventAggregator = eventAggregator;
        }

        public AlbumRelease GetRelease(int id)
        {
            return _releaseRepository.Get(id);
        }

        public AlbumRelease GetReleaseByForeignReleaseId(string foreignReleaseId, bool checkRedirect = false)
        {
            return _releaseRepository.FindByForeignReleaseId(foreignReleaseId, checkRedirect);
        }

        public void InsertMany(List<AlbumRelease> releases)
        {
            _releaseRepository.InsertMany(releases);
        }

        public void UpdateMany(List<AlbumRelease> releases)
        {
            _releaseRepository.UpdateMany(releases);
        }

        public void DeleteMany(List<AlbumRelease> releases)
        {
            _releaseRepository.DeleteMany(releases);
            foreach (var release in releases)
            {
                _eventAggregator.PublishEvent(new ReleaseDeletedEvent(release));
            }
        }

        public List<AlbumRelease> GetReleasesForRefresh(int albumId, IEnumerable<string> foreignReleaseIds)
        {
            return _releaseRepository.GetReleasesForRefresh(albumId, foreignReleaseIds);
        }

        public List<AlbumRelease> GetReleasesByAlbum(int releaseGroupId)
        {
            return _releaseRepository.FindByAlbum(releaseGroupId);
        }

        public List<AlbumRelease> GetReleasesByRecordingIds(List<string> recordingIds)
        {
            return _releaseRepository.FindByRecordingId(recordingIds);
        }

        public List<AlbumRelease> SetMonitored(AlbumRelease release)
        {
            return _releaseRepository.SetMonitored(release);
        }

        public void Handle(AlbumDeletedEvent message)
        {
            var releases = GetReleasesByAlbum(message.Album.Id);
            DeleteMany(releases);
        }

    }
}
