using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using Marr.Data.QGen;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> BlacklistedByTitle(int artistId, string sourceTitle);
        List<Blacklist> BlacklistedByTorrentInfoHash(int artistId, string torrentInfoHash);
        List<Blacklist> BlacklistedByArtist(int artistId);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator) :
            base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(int artistId, string sourceTitle)
        {
            return Query.Where(e => e.ArtistId == artistId)
                        .AndWhere(e => e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(int artistId, string torrentInfoHash)
        {
            return Query.Where(e => e.ArtistId == artistId)
                        .AndWhere(e => e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blacklist> BlacklistedByArtist(int artistId)
        {
            return Query.Where(b => b.ArtistId == artistId);
        }

        protected override SortBuilder<Blacklist> GetPagedQuery(QueryBuilder<Blacklist> query, PagingSpec<Blacklist> pagingSpec)
        {
            var baseQuery = query.Join<Blacklist, Artist>(JoinType.Inner, h => h.Artist, (h, s) => h.ArtistId == s.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }
    }
}
