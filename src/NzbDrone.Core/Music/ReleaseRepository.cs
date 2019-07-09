using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
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
            var release = Query
                .Where(x => x.ForeignReleaseId == foreignReleaseId)
                .SingleOrDefault();

            if (release == null && checkRedirect)
            {
                release = Query.Where(x => x.OldForeignReleaseIds.Contains(foreignReleaseId))
                               .SingleOrDefault();
            }
            
            return release;
        }

        public List<AlbumRelease> GetReleasesForRefresh(int albumId, IEnumerable<string> foreignReleaseIds)
        {
            return Query
                .Where(r => r.AlbumId == albumId)
                .OrWhere($"[ForeignReleaseId] IN ('{string.Join("', '", foreignReleaseIds)}')")
                .ToList();
        }

        public List<AlbumRelease> FindByAlbum(int id)
        {
            // populate the albums and artist metadata also
            // this hopefully speeds up the track matching a lot
            return Query
                .Join<AlbumRelease, Album>(JoinType.Left, r => r.Album, (r, a) => r.AlbumId == a.Id)
                .Join<Album, ArtistMetadata>(JoinType.Left, a => a.ArtistMetadata, (a, m) => a.ArtistMetadataId == m.Id)
                .Where<AlbumRelease>(r => r.AlbumId == id)
                .ToList();
        }

        public List<AlbumRelease> FindByRecordingId(List<string> recordingIds)
        {
            var query = "SELECT DISTINCT AlbumReleases.*" +
                "FROM AlbumReleases " +
                "JOIN Tracks ON Tracks.AlbumReleaseId = AlbumReleases.Id " +
                $"WHERE Tracks.ForeignRecordingId IN ('{string.Join("', '", recordingIds)}')";

            return Query.QueryText(query).ToList();
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
