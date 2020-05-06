using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumService
    {
        bool RefreshAlbumInfo(Book album, List<Book> remoteAlbums, Author remoteData, bool forceUpdateFileTags);
        bool RefreshAlbumInfo(List<Book> albums, List<Book> remoteAlbums, Author remoteData, bool forceAlbumRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);
    }

    public class RefreshAlbumService : RefreshEntityServiceBase<Book, object>, IRefreshAlbumService
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IProvideBookInfo _albumInfo;
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfAlbumShouldBeRefreshed _checkIfAlbumShouldBeRefreshed;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IAddArtistService addArtistService,
                                   IArtistMetadataService artistMetadataService,
                                   IProvideBookInfo albumInfo,
                                   IMediaFileService mediaFileService,
                                   IHistoryService historyService,
                                   IEventAggregator eventAggregator,
                                   ICheckIfAlbumShouldBeRefreshed checkIfAlbumShouldBeRefreshed,
                                   IMapCoversToLocal mediaCoverService,
                                   Logger logger)
        : base(logger, artistMetadataService)
        {
            _albumService = albumService;
            _artistService = artistService;
            _addArtistService = addArtistService;
            _albumInfo = albumInfo;
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _checkIfAlbumShouldBeRefreshed = checkIfAlbumShouldBeRefreshed;
            _mediaCoverService = mediaCoverService;
            _logger = logger;
        }

        protected override RemoteData GetRemoteData(Book local, List<Book> remote, Author data)
        {
            var result = new RemoteData();

            var book = remote.SingleOrDefault(x => x.ForeignWorkId == local.ForeignWorkId);

            if (book == null && ShouldDelete(local))
            {
                return result;
            }

            if (book == null)
            {
                book = data.Books.Value.SingleOrDefault(x => x.ForeignWorkId == local.ForeignWorkId);
            }

            result.Entity = book;
            if (result.Entity != null)
            {
                result.Entity.Id = local.Id;
            }

            return result;
        }

        protected override void EnsureNewParent(Book local, Book remote)
        {
            // Make sure the appropriate artist exists (it could be that an album changes parent)
            // The artistMetadata entry will be in the db but make sure a corresponding artist is too
            // so that the album doesn't just disappear.

            // TODO filter by metadata id before hitting database
            _logger.Trace($"Ensuring parent artist exists [{remote.AuthorMetadata.Value.ForeignAuthorId}]");

            var newArtist = _artistService.FindById(remote.AuthorMetadata.Value.ForeignAuthorId);

            if (newArtist == null)
            {
                var oldArtist = local.Author.Value;
                var addArtist = new Author
                {
                    Metadata = remote.AuthorMetadata.Value,
                    MetadataProfileId = oldArtist.MetadataProfileId,
                    QualityProfileId = oldArtist.QualityProfileId,
                    RootFolderPath = oldArtist.RootFolderPath,
                    Monitored = oldArtist.Monitored,
                    Tags = oldArtist.Tags
                };
                _logger.Debug($"Adding missing parent artist {addArtist}");
                _addArtistService.AddArtist(addArtist);
            }
        }

        protected override bool ShouldDelete(Book local)
        {
            // not manually added and has no files
            return local.AddOptions.AddType != AlbumAddType.Manual &&
                !_mediaFileService.GetFilesByAlbum(local.Id).Any();
        }

        protected override void LogProgress(Book local)
        {
            _logger.ProgressInfo("Updating Info for {0}", local.Title);
        }

        protected override bool IsMerge(Book local, Book remote)
        {
            return local.ForeignBookId != remote.ForeignBookId;
        }

        protected override UpdateResult UpdateEntity(Book local, Book remote)
        {
            UpdateResult result;

            remote.UseDbFieldsFrom(local);

            if (local.Title != (remote.Title ?? "Unknown") ||
                local.ForeignBookId != remote.ForeignBookId ||
                local.AuthorMetadata.Value.ForeignAuthorId != remote.AuthorMetadata.Value.ForeignAuthorId)
            {
                result = UpdateResult.UpdateTags;
            }
            else if (!local.Equals(remote))
            {
                result = UpdateResult.Standard;
            }
            else
            {
                result = UpdateResult.None;
            }

            // Force update and fetch covers if images have changed so that we can write them into tags
            // if (remote.Images.Any() && !local.Images.SequenceEqual(remote.Images))
            // {
            //     _mediaCoverService.EnsureAlbumCovers(remote);
            //     result = UpdateResult.UpdateTags;
            // }
            local.UseMetadataFrom(remote);

            local.AuthorMetadataId = remote.AuthorMetadata.Value.Id;
            local.LastInfoSync = DateTime.UtcNow;

            return result;
        }

        protected override UpdateResult MergeEntity(Book local, Book target, Book remote)
        {
            _logger.Warn($"Album {local} was merged with {remote} because the original was a duplicate.");

            // Update album ids for trackfiles
            var files = _mediaFileService.GetFilesByAlbum(local.Id);
            files.ForEach(x => x.BookId = target.Id);
            _mediaFileService.Update(files);

            // Update album ids for history
            var items = _historyService.GetByAlbum(local.Id, null);
            items.ForEach(x => x.BookId = target.Id);
            _historyService.UpdateMany(items);

            // Finally delete the old album
            _albumService.DeleteMany(new List<Book> { local });

            return UpdateResult.UpdateTags;
        }

        protected override Book GetEntityByForeignId(Book local)
        {
            return _albumService.FindById(local.ForeignBookId);
        }

        protected override void SaveEntity(Book local)
        {
            // Use UpdateMany to avoid firing the album edited event
            _albumService.UpdateMany(new List<Book> { local });
        }

        protected override void DeleteEntity(Book local, bool deleteFiles)
        {
            _albumService.DeleteAlbum(local.Id, true);
        }

        protected override List<object> GetRemoteChildren(Book local, Book remote)
        {
            return new List<object>();
        }

        protected override List<object> GetLocalChildren(Book entity, List<object> remoteChildren)
        {
            return new List<object>();
        }

        protected override Tuple<object, List<object>> GetMatchingExistingChildren(List<object> existingChildren, object remote)
        {
            return null;
        }

        protected override void PrepareNewChild(object child, Book entity)
        {
        }

        protected override void PrepareExistingChild(object local, object remote, Book entity)
        {
        }

        protected override void AddChildren(List<object> children)
        {
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<object> remoteChildren, Author remoteData, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            return false;
        }

        protected override void PublishEntityUpdatedEvent(Book entity)
        {
            // Fetch fresh from DB so all lazy loads are available
            _eventAggregator.PublishEvent(new AlbumUpdatedEvent(_albumService.GetAlbum(entity.Id)));
        }

        public bool RefreshAlbumInfo(List<Book> albums, List<Book> remoteAlbums, Author remoteData, bool forceAlbumRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            var updated = false;

            HashSet<string> updatedMusicbrainzAlbums = null;

            if (lastUpdate.HasValue && lastUpdate.Value.AddDays(14) > DateTime.UtcNow)
            {
                updatedMusicbrainzAlbums = _albumInfo.GetChangedAlbums(lastUpdate.Value);
            }

            foreach (var album in albums)
            {
                if (forceAlbumRefresh ||
                    (updatedMusicbrainzAlbums == null && _checkIfAlbumShouldBeRefreshed.ShouldRefresh(album)) ||
                    (updatedMusicbrainzAlbums != null && updatedMusicbrainzAlbums.Contains(album.ForeignBookId)))
                {
                    updated |= RefreshAlbumInfo(album, remoteAlbums, remoteData, forceUpdateFileTags);
                }
                else
                {
                    _logger.Debug("Skipping refresh of album: {0}", album.Title);
                }
            }

            return updated;
        }

        public bool RefreshAlbumInfo(Book album, List<Book> remoteAlbums, Author remoteData, bool forceUpdateFileTags)
        {
            return RefreshEntityInfo(album, remoteAlbums, remoteData, true, forceUpdateFileTags, null);
        }
    }
}
