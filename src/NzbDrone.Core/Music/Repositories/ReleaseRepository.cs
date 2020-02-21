using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IReleaseRepository : IBasicRepository<AlbumRelease>
    {
        AlbumRelease FindByForeignReleaseId(string foreignReleaseId, bool checkRedirect = false);
        List<AlbumRelease> FindByAlbum(int id);
        List<AlbumRelease> FindByRecordingId(List<string> recordingIds);
        List<AlbumRelease> GetReleasesForRefresh(int albumId, IEnumerable<string> foreignReleaseIds);
        List<AlbumRelease> SetMonitored(AlbumRelease release);
    }

    public class ReleaseRepository : BasicRepository<AlbumRelease>, IReleaseRepository
    {
        public ReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public AlbumRelease FindByForeignReleaseId(string foreignReleaseId, bool checkRedirect = false)
        {
            var release = Query(x => x.ForeignReleaseId == foreignReleaseId).SingleOrDefault();

            if (release == null && checkRedirect)
            {
                var id = "\"" + foreignReleaseId + "\"";
                release = Query(x => x.OldForeignReleaseIds.Contains(id)).SingleOrDefault();
            }

            return release;
        }

        public List<AlbumRelease> GetReleasesForRefresh(int albumId, IEnumerable<string> foreignReleaseIds)
        {
            return Query(r => r.AlbumId == albumId || foreignReleaseIds.Contains(r.ForeignReleaseId));
        }

        public List<AlbumRelease> FindByAlbum(int id)
        {
            // populate the albums and artist metadata also
            // this hopefully speeds up the track matching a lot
            var builder = new SqlBuilder()
                .Join<AlbumRelease, Album>((r, a) => r.AlbumId == a.Id)
                .Join<Album, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id)
                .Where<AlbumRelease>(r => r.AlbumId == id);

            return _database.QueryJoined<AlbumRelease, Album, ArtistMetadata>(builder, (release, album, metadata) =>
                    {
                        release.Album = album;
                        release.Album.Value.ArtistMetadata = metadata;
                        return release;
                    }).ToList();
        }

        public List<AlbumRelease> FindByRecordingId(List<string> recordingIds)
        {
            return Query(Builder()
                         .Join<AlbumRelease, Track>((r, t) => r.Id == t.AlbumReleaseId)
                         .Where<Track>(t => Enumerable.Contains(recordingIds, t.ForeignRecordingId))
                         .GroupBy<AlbumRelease>(x => x.Id));
        }

        public List<AlbumRelease> SetMonitored(AlbumRelease release)
        {
            var allReleases = FindByAlbum(release.AlbumId);
            allReleases.ForEach(r => r.Monitored = r.Id == release.Id);
            Ensure.That(allReleases.Count(x => x.Monitored) == 1).IsTrue();
            UpdateMany(allReleases);
            return allReleases;
        }
    }
}
