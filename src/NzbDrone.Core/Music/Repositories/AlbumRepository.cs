using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public interface IAlbumRepository : IBasicRepository<Album>
    {
        List<Album> GetAlbums(int artistId);
        List<Album> GetLastAlbums(IEnumerable<int> artistMetadataIds);
        List<Album> GetNextAlbums(IEnumerable<int> artistMetadataIds);
        List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId);
        List<Album> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds);
        Album FindByTitle(int artistMetadataId, string title);
        Album FindById(string foreignAlbumId);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Album album, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        Album FindAlbumByRelease(string albumReleaseId);
        Album FindAlbumByTrack(int trackId);
        List<Album> GetArtistAlbumsWithFiles(Artist artist);
    }

    public class AlbumRepository : BasicRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Album> GetAlbums(int artistId)
        {
            return Query(Builder().Join<Album, Artist>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId).Where<Artist>(a => a.Id == artistId));
        }

        public List<Album> GetLastAlbums(IEnumerable<int> artistMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Album>(x => artistMetadataIds.Contains(x.ArtistMetadataId) && x.ReleaseDate < now)
                         .GroupBy<Album>(x => x.ArtistMetadataId)
                         .Having("Albums.ReleaseDate = MAX(Albums.ReleaseDate)"));
        }

        public List<Album> GetNextAlbums(IEnumerable<int> artistMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Album>(x => artistMetadataIds.Contains(x.ArtistMetadataId) && x.ReleaseDate > now)
                         .GroupBy<Album>(x => x.ArtistMetadataId)
                         .Having("Albums.ReleaseDate = MIN(Albums.ReleaseDate)"));
        }

        public List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return Query(s => s.ArtistMetadataId == artistMetadataId);
        }

        public List<Album> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds)
        {
            return Query(a => a.ArtistMetadataId == artistMetadataId || foreignIds.Contains(a.ForeignAlbumId));
        }

        public Album FindById(string foreignAlbumId)
        {
            return Query(s => s.ForeignAlbumId == foreignAlbumId).SingleOrDefault();
        }

        //x.Id == null is converted to SQL, so warning incorrect
#pragma warning disable CS0472
        private SqlBuilder AlbumsWithoutFilesBuilder(DateTime currentTime, bool monitored) => Builder()
            .Join<Album, Artist>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
            .Join<Album, AlbumRelease>((a, r) => a.Id == r.AlbumId)
            .Join<AlbumRelease, Track>((r, t) => r.Id == t.AlbumReleaseId)
            .LeftJoin<Track, TrackFile>((t, f) => t.TrackFileId == f.Id)
            .Where<TrackFile>(f => f.Id == null)
            .Where<AlbumRelease>(r => r.Monitored == true)
            .Where<Album>(a => a.ReleaseDate <= currentTime && a.Monitored == monitored)
            .Where<Artist>(a => a.Monitored == monitored)
            .GroupBy<Album>(a => a.Id);
#pragma warning restore CS0472

        public PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec)
        {
            var currentTime = DateTime.UtcNow;
            var monitored = pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True");

            pagingSpec.Records = GetPagedRecords(AlbumsWithoutFilesBuilder(currentTime, monitored), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(AlbumsWithoutFilesBuilder(currentTime, monitored).SelectCountDistinct<Album>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        private SqlBuilder AlbumsWhereCutoffUnmetBuilder(bool monitored, List<QualitiesBelowCutoff> qualitiesBelowCutoff) => Builder()
            .Join<Album, Artist>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
            .Join<Album, AlbumRelease>((a, r) => a.Id == r.AlbumId)
            .Join<AlbumRelease, Track>((r, t) => r.Id == t.AlbumReleaseId)
            .LeftJoin<Track, TrackFile>((t, f) => t.TrackFileId == f.Id)
            .Where<AlbumRelease>(r => r.Monitored == true)
            .Where<Album>(a => a.Monitored == monitored)
            .Where<Artist>(a => a.Monitored == monitored)
            .GroupBy<Album>(a => a.Id)
            .Having(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(Artists.[QualityProfileId] = {0} AND MIN(TrackFiles.Quality) LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var monitored = pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True");

            pagingSpec.Records = GetPagedRecords(AlbumsWhereCutoffUnmetBuilder(monitored, qualitiesBelowCutoff), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM {TableMapping.Mapper.TableNameMapping(typeof(Album))} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/)";
            pagingSpec.TotalRecords = GetPagedRecordCount(AlbumsWhereCutoffUnmetBuilder(monitored, qualitiesBelowCutoff).Select(typeof(Album)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Album>(rg => rg.ReleaseDate >= startDate && rg.ReleaseDate <= endDate);

            if (!includeUnmonitored)
            {
                builder = builder.Where<Album>(e => e.Monitored == true)
                    .Where<Artist>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Album>(rg => rg.ReleaseDate >= startDate &&
                                                 rg.ReleaseDate <= endDate &&
                                                 rg.ArtistMetadataId == artist.ArtistMetadataId);

            if (!includeUnmonitored)
            {
                builder = builder.Where<Album>(e => e.Monitored == true)
                    .Where<Artist>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Album album, bool monitored)
        {
            album.Monitored = monitored;
            SetFields(album, p => p.Monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var albums = ids.Select(x => new Album { Id = x, Monitored = monitored }).ToList();
            SetFields(albums, p => p.Monitored);
        }

        public Album FindByTitle(int artistMetadataId, string title)
        {
            var cleanTitle = Parser.Parser.CleanArtistName(title);

            if (string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = title;
            }

            return Query(s => (s.CleanTitle == cleanTitle || s.Title == title) && s.ArtistMetadataId == artistMetadataId)
                .ExclusiveOrDefault();
        }

        public Album FindAlbumByRelease(string albumReleaseId)
        {
            return Query(Builder().Join<Album, AlbumRelease>((a, r) => a.Id == r.AlbumId)
                         .Where<AlbumRelease>(x => x.ForeignReleaseId == albumReleaseId)).FirstOrDefault();
        }

        public Album FindAlbumByTrack(int trackId)
        {
            return Query(Builder().Join<Album, AlbumRelease>((a, r) => a.Id == r.AlbumId)
                         .Join<AlbumRelease, Track>((r, t) => r.Id == t.AlbumReleaseId)
                         .Where<Track>(x => x.Id == trackId)).FirstOrDefault();
        }

        public List<Album> GetArtistAlbumsWithFiles(Artist artist)
        {
            var id = artist.ArtistMetadataId;
            return Query(Builder().Join<Album, AlbumRelease>((a, r) => a.Id == r.AlbumId)
                         .Join<AlbumRelease, Track>((r, t) => r.Id == t.AlbumReleaseId)
                         .Join<Track, TrackFile>((t, f) => t.TrackFileId == f.Id)
                         .Where<Album>(x => x.ArtistMetadataId == id)
                         .Where<AlbumRelease>(r => r.Monitored == true)
                         .GroupBy<Album>(x => x.Id));
        }
    }
}
