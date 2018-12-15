using System;
using System.Linq;
using NLog;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public interface IAlbumRepository : IBasicRepository<Album>
    {
        List<Album> GetAlbums(int artistId);
        List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId);
        Album FindByName(string cleanTitle);
        Album FindByTitle(int artistMetadataId, string title);
        Album FindByArtistAndName(string artistName, string cleanTitle);
        Album FindById(string spotifyId);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff);
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
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _database = database;
            _logger = logger;
        }

        public List<Album> GetAlbums(int artistId)
        {
            return Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .Where<Artist>(a => a.Id == artistId).ToList();
        }

        public List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return Query.Where(s => s.ArtistMetadataId == artistMetadataId);
        }

        public Album FindById(string foreignAlbumId)
        {
            return Query.Where(s => s.ForeignAlbumId == foreignAlbumId).SingleOrDefault();
        }

        public PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec)
        {
            var currentTime = DateTime.UtcNow;

            //pagingSpec.TotalRecords = GetMissingAlbumsQuery(pagingSpec, currentTime).GetRowCount(); Cant Use GetRowCount with a Manual Query

            pagingSpec.TotalRecords = GetMissingAlbumsQueryCount(pagingSpec, currentTime);
            pagingSpec.Records = GetMissingAlbumsQuery(pagingSpec, currentTime).ToList();

            return pagingSpec;
        }

        public PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff)
        {

            pagingSpec.TotalRecords = GetCutOffAlbumsQueryCount(pagingSpec, qualitiesBelowCutoff, languagesBelowCutoff);
            pagingSpec.Records = GetCutOffAlbumsQuery(pagingSpec, qualitiesBelowCutoff, languagesBelowCutoff).ToList();

            return pagingSpec;
        }

        public List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var query = Query.Join<Album, Artist>(JoinType.Inner, rg => rg.Artist, (rg, a) => rg.ArtistMetadataId == a.ArtistMetadataId)
                             .Where<Album>(rg => rg.ReleaseDate >= startDate)
                             .AndWhere(rg => rg.ReleaseDate <= endDate);


            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored)
                     .AndWhere(e => e.Artist.Value.Monitored);
            }

            return query.ToList();
        }

        public List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var query = Query.Join<Album, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistMetadataId == s.ArtistMetadataId)
                .Where<Album>(e => e.ReleaseDate >= startDate)
                .AndWhere(e => e.ReleaseDate <= endDate)
                .AndWhere(e => e.ArtistMetadataId == artist.ArtistMetadataId);


            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored)
                    .AndWhere(e => e.Artist.Value.Monitored);
            }

            return query.ToList();
        }

        private QueryBuilder<Album> GetMissingAlbumsQuery(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            string sortKey;
            string monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            if (pagingSpec.SortKey == "releaseDate")
            {
                sortKey = "Albums." + pagingSpec.SortKey;
            }
            else if (pagingSpec.SortKey == "artist.sortName")
            {
                sortKey = "Artists." + pagingSpec.SortKey.Split('.').Last();
            }
            else if (pagingSpec.SortKey == "albumTitle")
            {
                sortKey = "Albums.title";
            }
            else
            {
                sortKey = "Albums.releaseDate";
            }

            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN Artists ON Albums.ArtistMetadataId = Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "LEFT OUTER JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE TrackFiles.Id IS NULL " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "AND ({0}) AND {1} " +
                                         "GROUP BY Albums.Id " +
                                         " ORDER BY {2} {3} LIMIT {4} OFFSET {5}",
                                         monitored,
                                         BuildReleaseDateCutoffWhereClause(currentTime),
                                         sortKey,
                                         pagingSpec.ToSortDirection(),
                                         pagingSpec.PageSize,
                                         pagingSpec.PagingOffset());

            return Query.QueryText(query);
        }

        private int GetMissingAlbumsQueryCount(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            var monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN Artists ON Albums.ArtistMetadataId = Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "LEFT OUTER JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE TrackFiles.Id IS NULL " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "AND ({0}) AND {1} " +
                                         "GROUP BY Albums.Id ",
                                         monitored,
                                         BuildReleaseDateCutoffWhereClause(currentTime));

            return Query.QueryText(query).Count();
        }

        private string BuildReleaseDateCutoffWhereClause(DateTime currentTime)
        {
            return string.Format("datetime(strftime('%s', Albums.[ReleaseDate]),  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private QueryBuilder<Album> GetCutOffAlbumsQuery(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff)
        {
            string sortKey;
            string monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            if (pagingSpec.SortKey == "releaseDate")
            {
                sortKey = "Albums." + pagingSpec.SortKey;
            }
            else if (pagingSpec.SortKey == "artist.sortName")
            {
                sortKey = "Artists." + pagingSpec.SortKey.Split('.').Last();
            }
            else if (pagingSpec.SortKey == "albumTitle")
            {
                sortKey = "Albums.title";
            }
            else
            {
                sortKey = "Albums.releaseDate";
            }

            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN Artists on Albums.ArtistMetadataId == Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE {0} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "GROUP BY Albums.Id " +
                                         "HAVING ({1} OR {2}) " +
                                         "ORDER BY {3} {4} LIMIT {5} OFFSET {6}",
                                         monitored,
                                         BuildQualityCutoffWhereClause(qualitiesBelowCutoff),
                                         BuildLanguageCutoffWhereClause(languagesBelowCutoff),
                                         sortKey,
                                         pagingSpec.ToSortDirection(),
                                         pagingSpec.PageSize,
                                         pagingSpec.PagingOffset());

            return Query.QueryText(query);

        }

        private int GetCutOffAlbumsQueryCount(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff)
        {
            var monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN Artists on Albums.ArtistMetadataId == Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE {0} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "GROUP BY Albums.Id " +
                                         "HAVING ({1} OR {2}) ",
                                         monitored,
                                         BuildQualityCutoffWhereClause(qualitiesBelowCutoff),
                                         BuildLanguageCutoffWhereClause(languagesBelowCutoff));

            return Query.QueryText(query).Count();
        }


        private string BuildLanguageCutoffWhereClause(List<LanguagesBelowCutoff> languagesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var language in languagesBelowCutoff)
            {
                foreach (var belowCutoff in language.LanguageIds)
                {
                    clauses.Add(string.Format("(Artists.[LanguageProfileId] = {0} AND TrackFiles.[Language] = {1})", language.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(Artists.[ProfileId] = {0} AND MIN(TrackFiles.Quality) LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public void SetMonitoredFlat(Album album, bool monitored)
        {
            album.Monitored = monitored;
            SetFields(album, p => p.Monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("monitored", monitored);

            var sql = "UPDATE Albums " +
                      "SET Monitored = @monitored " +
                      $"WHERE Id IN ({string.Join(", ", ids)})";

            mapper.ExecuteNonQuery(sql);
        }

        public Album FindByName(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle).SingleOrDefault();
        }

        public Album FindByTitle(int artistMetadataId, string title)
        {
            var cleanTitle = Parser.Parser.CleanArtistName(title);
            
            if (string.IsNullOrEmpty(cleanTitle))
                cleanTitle = title;
            
            return Query.Where(s => s.CleanTitle == cleanTitle || s.Title == title)
                        .AndWhere(s => s.ArtistMetadataId == artistMetadataId)
                        .FirstOrDefault();
        }

        public Album FindByArtistAndName(string artistName, string cleanTitle)
        {
            var cleanArtistName = Parser.Parser.CleanArtistName(artistName);
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Join<Album, Artist>(JoinType.Inner, rg => rg.Artist, (rg, artist) => rg.ArtistMetadataId == artist.ArtistMetadataId)
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Album FindAlbumByRelease(string albumReleaseId)
        {
            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId = Albums.Id " +
                                         "WHERE AlbumReleases.ForeignReleaseId = '{0}'",
                                         albumReleaseId);
            return Query.QueryText(query).FirstOrDefault();
        }

        public Album FindAlbumByTrack(int trackId)
        {
            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId = Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId = AlbumReleases.Id " +
                                         "WHERE Tracks.Id = {0}",
                                         trackId);
            return Query.QueryText(query).FirstOrDefault();
        }

        public List<Album> GetArtistAlbumsWithFiles(Artist artist)
        {
            string query = string.Format("SELECT Albums.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE Albums.ArtistMetadataId == {0} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "GROUP BY Albums.Id ",
                                         artist.ArtistMetadataId);

            return Query.QueryText(query).ToList();
        }
    }
}
