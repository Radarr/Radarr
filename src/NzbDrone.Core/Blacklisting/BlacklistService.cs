using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(int authorId, ReleaseInfo release);
        PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec);
        void Delete(int id);
        void Delete(List<int> ids);
    }

    public class BlacklistService : IBlacklistService,

                                    IExecute<ClearBlacklistCommand>,
                                    IHandle<DownloadFailedEvent>,
                                    IHandleAsync<AuthorDeletedEvent>
    {
        private readonly IBlacklistRepository _blacklistRepository;

        public BlacklistService(IBlacklistRepository blacklistRepository)
        {
            _blacklistRepository = blacklistRepository;
        }

        public bool Blacklisted(int authorId, ReleaseInfo release)
        {
            var blacklistedByTitle = _blacklistRepository.BlacklistedByTitle(authorId, release.Title);

            if (release.DownloadProtocol == DownloadProtocol.Torrent)
            {
                var torrentInfo = release as TorrentInfo;

                if (torrentInfo == null)
                {
                    return false;
                }

                if (torrentInfo.InfoHash.IsNullOrWhiteSpace())
                {
                    return blacklistedByTitle.Where(b => b.Protocol == DownloadProtocol.Torrent)
                                             .Any(b => SameTorrent(b, torrentInfo));
                }

                var blacklistedByTorrentInfohash = _blacklistRepository.BlacklistedByTorrentInfoHash(authorId, torrentInfo.InfoHash);

                return blacklistedByTorrentInfohash.Any(b => SameTorrent(b, torrentInfo));
            }

            return blacklistedByTitle.Where(b => b.Protocol == DownloadProtocol.Usenet)
                                     .Any(b => SameNzb(b, release));
        }

        public PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec)
        {
            return _blacklistRepository.GetPaged(pagingSpec);
        }

        public void Delete(int id)
        {
            _blacklistRepository.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _blacklistRepository.DeleteMany(ids);
        }

        private bool SameNzb(Blacklist item, ReleaseInfo release)
        {
            if (item.PublishedDate == release.PublishDate)
            {
                return true;
            }

            if (!HasSameIndexer(item, release.Indexer) &&
                HasSamePublishedDate(item, release.PublishDate) &&
                HasSameSize(item, release.Size))
            {
                return true;
            }

            return false;
        }

        private bool SameTorrent(Blacklist item, TorrentInfo release)
        {
            if (release.InfoHash.IsNotNullOrWhiteSpace())
            {
                return release.InfoHash.Equals(item.TorrentInfoHash);
            }

            return item.Indexer.Equals(release.Indexer, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasSameIndexer(Blacklist item, string indexer)
        {
            if (item.Indexer.IsNullOrWhiteSpace())
            {
                return true;
            }

            return item.Indexer.Equals(indexer, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasSamePublishedDate(Blacklist item, DateTime publishedDate)
        {
            if (!item.PublishedDate.HasValue)
            {
                return true;
            }

            return item.PublishedDate.Value.AddMinutes(-2) <= publishedDate &&
                   item.PublishedDate.Value.AddMinutes(2) >= publishedDate;
        }

        private bool HasSameSize(Blacklist item, long size)
        {
            if (!item.Size.HasValue)
            {
                return true;
            }

            var difference = Math.Abs(item.Size.Value - size);

            return difference <= 2.Megabytes();
        }

        public void Execute(ClearBlacklistCommand message)
        {
            _blacklistRepository.Purge();
        }

        public void Handle(DownloadFailedEvent message)
        {
            var blacklist = new Blacklist
            {
                AuthorId = message.AuthorId,
                BookIds = message.BookIds,
                SourceTitle = message.SourceTitle,
                Quality = message.Quality,
                Date = DateTime.UtcNow,
                PublishedDate = DateTime.Parse(message.Data.GetValueOrDefault("publishedDate")),
                Size = long.Parse(message.Data.GetValueOrDefault("size", "0")),
                Indexer = message.Data.GetValueOrDefault("indexer"),
                Protocol = (DownloadProtocol)Convert.ToInt32(message.Data.GetValueOrDefault("protocol")),
                Message = message.Message,
                TorrentInfoHash = message.Data.GetValueOrDefault("torrentInfoHash")
            };

            _blacklistRepository.Insert(blacklist);
        }

        public void HandleAsync(AuthorDeletedEvent message)
        {
            var blacklisted = _blacklistRepository.BlacklistedByAuthor(message.Author.Id);

            _blacklistRepository.DeleteMany(blacklisted);
        }
    }
}
