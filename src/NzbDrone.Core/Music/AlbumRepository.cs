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
        Album FindByArtistAndName(string artistName, string cleanTitle);
        Album FindById(string spotifyId);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        List<Album> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Album album, bool monitored);
    }

    public class AlbumRepository : BasicRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
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

            pagingSpec.TotalRecords = GetMissingAlbumsQuery(pagingSpec, currentTime).GetRowCount();
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

        private SortBuilder<Album> GetMissingAlbumsQuery(PagingSpec<Album> pagingSpec, DateTime currentTime)
        {
            return Query.Join<Album, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
                            .Where<Album>(pagingSpec.FilterExpression)
                            
                            .AndWhere(BuildReleaseDateCutoffWhereClause(currentTime))
                            //.Where<Track>(t => t.TrackFileId == 0)
                            .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                            .Skip(pagingSpec.PagingOffset())
                            .Take(pagingSpec.PageSize);
        }

        private string BuildReleaseDateCutoffWhereClause(DateTime currentTime)
        {
            return string.Format("WHERE datetime(strftime('%s', [t0].[ReleaseDate]),  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public void SetMonitoredFlat(Album album, bool monitored)
        {
            album.Monitored = monitored;
            SetFields(album, p => p.Monitored);
        }

        public Album FindByName(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Album FindByArtistAndName(string artistName, string cleanTitle)
        {
            var cleanArtistName = Parser.Parser.CleanArtistTitle(artistName);
            cleanTitle = cleanTitle.ToLowerInvariant();
            var query = Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (album, artist) => album.ArtistId == artist.Id)
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle);
            return Query.Join<Album, Artist>(JoinType.Inner, album => album.Artist, (album, artist) => album.ArtistId == artist.Id )
                        .Where<Artist>(artist => artist.CleanName == cleanArtistName)
                        .Where<Album>(album => album.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }
    }
}
