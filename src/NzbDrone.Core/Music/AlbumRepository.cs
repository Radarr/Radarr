using System;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Events;
using Marr.Data.QGen;

namespace NzbDrone.Core.Music
{
    public interface IAlbumRepository : IBasicRepository<Album>
    {
        bool AlbumPathExists(string path);
        List<Album> GetAlbums(int artistId);
        Album FindByName(string cleanTitle);
        Album FindByTitle(int artistId, string title);
        Album FindByArtistAndName(string artistName, string cleanTitle);
        Album FindById(string spotifyId);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Album album, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
    }

    public class AlbumRepository : BasicRepository<Album>, IAlbumRepository
    {
        private readonly IMainDatabase _database;

        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }


        public bool AlbumPathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
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

        private QueryBuilder<Album> GetMissingAlbumsQuery(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            string sortKey;
            string monitored = "(Albums.[Monitored] = 0) OR (Artists.[Monitored] = 0)";

            if (pagingSpec.FilterExpression.ToString().Contains("True"))
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

            //Use Manual Query until we find a way to "NOT EXIST(SELECT 1 from Tracks WHERE [t2].trackFileId <> 0)"

            //return Query.Join<Album, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
            //                .Where<Album>(pagingSpec.FilterExpression)
            //                .AndWhere(BuildReleaseDateCutoffWhereClause(currentTime))
            //                //.Where<Track>(t => t.TrackFileId == 0)
            //                .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
            //                .Skip(pagingSpec.PagingOffset())
            //                .Take(pagingSpec.PageSize);
        }

        private int GetMissingAlbumsQueryCount(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            var monitored = 0;

            if (pagingSpec.FilterExpression.ToString().Contains("True"))
            {
                monitored = 1;
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

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Album FindByTitle(int artistId, string title)
        {
            title = Parser.Parser.CleanArtistTitle(title);

            return Query.Where(s => s.CleanTitle == title)
                        .AndWhere(s => s.ArtistId == artistId)
                        .FirstOrDefault();
        }

        public Album FindByArtistAndName(string artistName, string cleanTitle)
        {
            var cleanArtistName = Parser.Parser.CleanArtistTitle(artistName);
            cleanTitle = cleanTitle.ToLowerInvariant();
            var query = Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (album, artist) => album.ArtistId == artist.Id)
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle);
            return Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (album, artist) => album.ArtistId == artist.Id)
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }
    }
}
