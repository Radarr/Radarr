using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> BlacklistedByTitle(int movieId, string sourceTitle);
        List<Blacklist> BlacklistedByTorrentInfoHash(int movieId, string torrentInfoHash);
        List<Blacklist> BlacklistedByMovies(List<int> movieIds);
        void DeleteForMovies(List<int> movieIds);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(int movieId, string sourceTitle)
        {
            return Query(x => x.MovieId == movieId && x.SourceTitle.Contains(sourceTitle));
        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(int movieId, string torrentInfoHash)
        {
            return Query(x => x.MovieId == movieId && x.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blacklist> BlacklistedByMovies(List<int> movieIds)
        {
            return Query(x => movieIds.Contains(x.MovieId));
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(x => movieIds.Contains(x.MovieId));
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder().Join<Blacklist, Movie>((b, m) => b.MovieId == m.Id);
        protected override IEnumerable<Blacklist> PagedQuery(SqlBuilder sql) => _database.QueryJoined<Blacklist, Movie>(sql, (bl, movie) =>
                    {
                        bl.Movie = movie;
                        return bl;
                    });
    }
}
