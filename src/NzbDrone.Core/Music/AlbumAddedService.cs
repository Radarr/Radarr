using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public interface IAlbumAddedService
    {
        void SearchForRecentlyAdded(int artistId);
    }

    public class AlbumAddedService : IHandle<AlbumInfoRefreshedEvent>, IAlbumAddedService
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IAlbumService _albumService;
        private readonly Logger _logger;
        private readonly ICached<List<int>> _addedAlbumsCache;

        public AlbumAddedService(ICacheManager cacheManager,
                                   IManageCommandQueue commandQueueManager,
                                   IAlbumService albumService,
                                   Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _albumService = albumService;
            _logger = logger;
            _addedAlbumsCache = cacheManager.GetCache<List<int>>(GetType());
        }

        public void SearchForRecentlyAdded(int artistId)
        {
            var previouslyReleased = _addedAlbumsCache.Find(artistId.ToString());

            if (previouslyReleased != null && previouslyReleased.Any())
            {
                var missing = previouslyReleased.Select(e => _albumService.GetAlbum(e)).ToList();

                if (missing.Any())
                {
                    _commandQueueManager.Push(new AlbumSearchCommand(missing.Select(e => e.Id).ToList()));
                }
            }

            _addedAlbumsCache.Remove(artistId.ToString());
        }

        public void Handle(AlbumInfoRefreshedEvent message)
        {
            if (message.Artist.AddOptions == null)
            {
                if (!message.Artist.Monitored)
                {
                    _logger.Debug("Artist is not monitored");
                    return;
                }

                if (message.Added.Empty())
                {
                    _logger.Debug("No new albums, skipping search");
                    return;
                }

                if (message.Added.None(a => a.ReleaseDate.HasValue))
                {
                    _logger.Debug("No new albums have an release date");
                    return;
                }

                var previouslyReleased = message.Added.Where(a => a.ReleaseDate.HasValue && a.ReleaseDate.Value.Before(DateTime.UtcNow.AddDays(1)) && a.Monitored).ToList();

                if (previouslyReleased.Empty())
                {
                    _logger.Debug("Newly added albums all release in the future");
                    return;
                }

                _addedAlbumsCache.Set(message.Artist.Id.ToString(), previouslyReleased.Select(e => e.Id).ToList());
            }
        }
    }
}
