using System.Collections.Generic;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
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
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(int artistId, string sourceTitle)
        {
            return Query(e => e.ArtistId == artistId && e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(int artistId, string torrentInfoHash)
        {
            return Query(e => e.ArtistId == artistId && e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blacklist> BlacklistedByArtist(int artistId)
        {
            return Query(b => b.ArtistId == artistId);
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder().Join<Blacklist, Artist>((b, m) => b.ArtistId == m.Id);
        protected override IEnumerable<Blacklist> PagedQuery(SqlBuilder builder) => _database.QueryJoined<Blacklist, Artist>(builder, (bl, artist) =>
                    {
                        bl.Artist = artist;
                        return bl;
                    });
    }
}
