using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IAlbumRepository : IBasicRepository<Album>
    {
        Album FindByForeignId(string foreignAlbumId);
        List<Album> FindByArtistId(int artistId);
        List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        Album FindByPath(string path);
        Dictionary<int, string> AllAlbumPaths();
        Dictionary<int, List<int>> AllAlbumTags();
        bool AlbumPathExists(string path);
    }

    public class AlbumRepository : BasicRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Album FindByForeignId(string foreignAlbumId)
        {
            return Query(a => a.ForeignAlbumId == foreignAlbumId).FirstOrDefault();
        }

        public List<Album> FindByArtistId(int artistId)
        {
            return Query(a => a.ArtistId == artistId);
        }

        public List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var query = Query(a => a.ReleaseDate >= start && a.ReleaseDate <= end);

            if (!includeUnmonitored)
            {
                query = query.Where(a => a.Monitored).ToList();
            }

            return query;
        }

        public Album FindByPath(string path)
        {
            return Query(a => a.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllAlbumPaths()
        {
            var albums = All();
            return albums.ToDictionary(a => a.Id, a => a.Path);
        }

        public Dictionary<int, List<int>> AllAlbumTags()
        {
            var albums = All();
            return albums.ToDictionary(a => a.Id, a => a.Tags.ToList());
        }

        public bool AlbumPathExists(string path)
        {
            return Query(a => a.Path == path).Any();
        }
    }
}
