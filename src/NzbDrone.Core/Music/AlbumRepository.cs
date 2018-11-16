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
        Album FindByName(string cleanTitle);
        Album FindByTitle(int artistId, string title);
        Album FindByArtistAndName(string artistName, string cleanTitle);
        Album FindById(string spotifyId);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        PagingSpec<Album> AlbumsWhereCutoffUnmet(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff);
        List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Album album, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        Album FindAlbumByRelease(string releaseId);
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
            return Query.Where(s => s.ArtistId == artistId).ToList();
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
            var query = Query.Join<Album, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
                             .Where<Album>(e => e.ReleaseDate >= startDate)
                             .AndWhere(e => e.ReleaseDate <= endDate);


            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored)
                     .AndWhere(e => e.Artist.Monitored);
            }

            return query.ToList();
        }

        public List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var query = Query.Join<Album, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
                .Where<Album>(e => e.ReleaseDate >= startDate)
                .AndWhere(e => e.ReleaseDate <= endDate)
                .AndWhere(e => e.ArtistId == artist.Id);


            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored)
                    .AndWhere(e => e.Artist.Monitored);
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

            string query = string.Format("SELECT Albums.* FROM (SELECT Tracks.AlbumId, COUNT(*) AS TotalTrackCount," + "" +
                "SUM(CASE WHEN TrackFileId > 0 THEN 1 ELSE 0 END) AS AvailableTrackCount FROM Tracks GROUP BY Tracks.ArtistId, Tracks.AlbumId) as Tracks" +
                 " LEFT OUTER JOIN Albums ON Tracks.AlbumId == Albums.Id" +
                 " LEFT OUTER JOIN Artists ON Albums.ArtistId == Artists.Id" +
                 " WHERE Tracks.TotalTrackCount != Tracks.AvailableTrackCount AND ({0}) AND {1}" +
                 " GROUP BY Tracks.AlbumId" +
                 " ORDER BY {2} {3} LIMIT {4} OFFSET {5}",
                 monitored, BuildReleaseDateCutoffWhereClause(currentTime), sortKey, pagingSpec.ToSortDirection(), pagingSpec.PageSize, pagingSpec.PagingOffset());

            return Query.QueryText(query);
        }

        private int GetMissingAlbumsQueryCount(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            var monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            string query = string.Format("SELECT Albums.* FROM (SELECT Tracks.AlbumId, COUNT(*) AS TotalTrackCount," +
                " SUM(CASE WHEN TrackFileId > 0 THEN 1 ELSE 0 END) AS AvailableTrackCount FROM Tracks GROUP BY Tracks.ArtistId, Tracks.AlbumId) as Tracks" +
                " LEFT OUTER JOIN Albums ON Tracks.AlbumId == Albums.Id" +
                " LEFT OUTER JOIN Artists ON Albums.ArtistId == Artists.Id" +
                " WHERE Tracks.TotalTrackCount != Tracks.AvailableTrackCount AND ({0}) AND {1}" +
                " GROUP BY Tracks.AlbumId",
                monitored, BuildReleaseDateCutoffWhereClause(currentTime));

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

            string query = string.Format("SELECT Albums.* FROM(SELECT TrackFiles.AlbumId, TrackFiles.Language, COUNT(*) AS FileCount, " +
                " MIN(Quality) AS MinQuality FROM TrackFiles GROUP BY TrackFiles.ArtistId, TrackFiles.AlbumId) as TrackFiles" +
                " LEFT OUTER JOIN Albums ON TrackFiles.AlbumId == Albums.Id" +
                " LEFT OUTER JOIN Artists ON Albums.ArtistId == Artists.Id" +
                " WHERE ({0}) AND ({1} OR {2})" +
                " GROUP BY TrackFiles.AlbumId" +
                " ORDER BY {3} {4} LIMIT {5} OFFSET {6}",
                monitored, BuildQualityCutoffWhereClause(qualitiesBelowCutoff), BuildLanguageCutoffWhereClause(languagesBelowCutoff), sortKey, pagingSpec.ToSortDirection(), pagingSpec.PageSize, pagingSpec.PagingOffset());

            return Query.QueryText(query);

        }

        private int GetCutOffAlbumsQueryCount(PagingSpec<Album> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff)
        {
            var monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpressions.FirstOrDefault().ToString().Contains("True"))
            {
                monitored = "(Albums.[Monitored] = 1) AND (Artists.[Monitored] = 1)";
            }

            string query = string.Format("SELECT Albums.* FROM (SELECT TrackFiles.AlbumId, TrackFiles.Language, COUNT(*) AS FileCount," +
                " MIN(Quality) AS MinQuality FROM TrackFiles GROUP BY TrackFiles.ArtistId, TrackFiles.AlbumId) as TrackFiles" +
                " LEFT OUTER JOIN Albums ON TrackFiles.AlbumId == Albums.Id" +
                " LEFT OUTER JOIN Artists ON Albums.ArtistId == Artists.Id" +
                " WHERE ({0}) AND ({1} OR {2})" +
                " GROUP BY TrackFiles.AlbumId",
                monitored, BuildQualityCutoffWhereClause(qualitiesBelowCutoff), BuildLanguageCutoffWhereClause(languagesBelowCutoff));

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
                    clauses.Add(string.Format("(Artists.[ProfileId] = {0} AND TrackFiles.MinQuality LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
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

        public Album FindByTitle(int artistId, string title)
        {
            var cleanTitle = Parser.Parser.CleanArtistName(title);
            
            if (string.IsNullOrEmpty(cleanTitle))
                cleanTitle = title;
            
            return Query.Where(s => s.CleanTitle == cleanTitle || s.Title == title)
                        .AndWhere(s => s.ArtistId == artistId)
                        .FirstOrDefault();
        }

        public Album FindByArtistAndName(string artistName, string cleanTitle)
        {
            var cleanArtistName = Parser.Parser.CleanArtistName(artistName);
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (album, artist) => album.ArtistId == artist.Id)
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Album FindAlbumByRelease(string releaseId)
        {
            return Query.FirstOrDefault(e => e.Releases.Any(r => r.Id == releaseId));
        }

        public List<Album> GetArtistAlbumsWithFiles(Artist artist)
        {
            string query = string.Format("SELECT Albums.* FROM (SELECT Tracks.AlbumId, COUNT(*) AS TotalTrackCount," + "" +
                                         "SUM(CASE WHEN TrackFileId > 0 THEN 1 ELSE 0 END) AS AvailableTrackCount FROM Tracks GROUP BY Tracks.ArtistId, Tracks.AlbumId) as Tracks" +
                                         " LEFT OUTER JOIN Albums ON Tracks.AlbumId == Albums.Id" +
                                         " LEFT OUTER JOIN Artists ON Albums.ArtistId == Artists.Id" +
                                         " WHERE Tracks.AvailableTrackCount > 0" +
                                         " AND Albums.ArtistId=" + artist.Id + 
                                         " GROUP BY Tracks.AlbumId");

            return Query.QueryText(query);

        }
    }
}
