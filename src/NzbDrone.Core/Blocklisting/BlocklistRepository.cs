using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Blocklisting
{
    public interface IBlocklistRepository : IBasicRepository<Blocklist>
    {
        List<Blocklist> BlocklistedByTitle(int movieId, string sourceTitle);
        List<Blocklist> BlocklistedByTorrentInfoHash(int movieId, string torrentInfoHash);
        List<Blocklist> BlocklistedByMovie(int movieId);
        void DeleteForMovies(List<int> movieIds);
    }

    public class BlocklistRepository : BasicRepository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blocklist> BlocklistedByTitle(int movieId, string sourceTitle)
        {
            return Query(x => x.MovieId == movieId && x.SourceTitle.Contains(sourceTitle));
        }

        public List<Blocklist> BlocklistedByTorrentInfoHash(int movieId, string torrentInfoHash)
        {
            return Query(x => x.MovieId == movieId && x.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blocklist> BlocklistedByMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId);
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder(_database.DatabaseType).Join<Blocklist, Movie>((b, m) => b.MovieId == m.Id);
        protected override IEnumerable<Blocklist> PagedQuery(SqlBuilder sql) => _database.QueryJoined<Blocklist, Movie>(sql, (bl, movie) =>
                    {
                        bl.Movie = movie;
                        return bl;
                    });
    }
}
