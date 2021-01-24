using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> BlacklistedByTitle(int authorId, string sourceTitle);
        List<Blacklist> BlacklistedByTorrentInfoHash(int authorId, string torrentInfoHash);
        List<Blacklist> BlacklistedByAuthor(int authorId);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(int authorId, string sourceTitle)
        {
            return Query(e => e.AuthorId == authorId && e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(int authorId, string torrentInfoHash)
        {
            return Query(e => e.AuthorId == authorId && e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blacklist> BlacklistedByAuthor(int authorId)
        {
            return Query(b => b.AuthorId == authorId);
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder().Join<Blacklist, Author>((b, m) => b.AuthorId == m.Id);
        protected override IEnumerable<Blacklist> PagedQuery(SqlBuilder builder) => _database.QueryJoined<Blacklist, Author>(builder, (bl, author) =>
                    {
                        bl.Author = author;
                        return bl;
                    });
    }
}
