using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.MusicStats;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Music
{
    [V3ApiController]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107", Justification = "DI pattern")]
    public class AlbumController : RestControllerWithSignalR<AlbumResource, Album>,
                                   IHandle<AlbumAddedEvent>,
                                   IHandle<AlbumEditedEvent>,
                                   IHandle<AlbumsDeletedEvent>
    {
        private readonly IAlbumService _albumService;
        private readonly IAddAlbumService _addAlbumService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;
        private readonly IMusicStatisticsService _musicStatisticsService;

        public AlbumController(IBroadcastSignalRMessage signalRBroadcaster,
                               IAlbumService albumService,
                               IAddAlbumService addAlbumService,
                               IRootFolderService rootFolderService,
                               IHierarchicalMonitoringService monitoringService,
                               IMusicStatisticsService musicStatisticsService,
                               RootFolderValidator rootFolderValidator,
                               MappedNetworkDriveValidator mappedNetworkDriveValidator,
                               RecycleBinValidator recycleBinValidator,
                               SystemFolderValidator systemFolderValidator,
                               QualityProfileExistsValidator qualityProfileExistsValidator,
                               RootFolderExistsValidator rootFolderExistsValidator)
            : base(signalRBroadcaster)
        {
            _albumService = albumService;
            _addAlbumService = addAlbumService;
            _rootFolderService = rootFolderService;
            _monitoringService = monitoringService;
            _musicStatisticsService = musicStatisticsService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<AlbumResource> GetAlbums(int? artistId = null)
        {
            List<Album> albums;

            if (artistId.HasValue)
            {
                albums = _albumService.FindByArtistId(artistId.Value);
            }
            else
            {
                albums = _albumService.GetAllAlbums();
            }

            var resources = albums.ToResource();
            var rootFolders = _rootFolderService.All();
            var albumStats = _musicStatisticsService.AlbumStatistics();
            var sdict = albumStats.ToDictionary(x => x.AlbumId);

            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Path.IsNotNullOrWhiteSpace())
                {
                    resources[i].RootFolderPath = _rootFolderService.GetBestRootFolderPath(resources[i].Path, rootFolders);
                }

                resources[i].EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(albums[i]);
            }

            LinkAlbumStatistics(resources, sdict);

            return resources;
        }

        protected override AlbumResource GetResourceById(int id)
        {
            var album = _albumService.GetAlbum(id);
            return MapToResource(album);
        }

        private AlbumResource MapToResource(Album album)
        {
            if (album == null)
            {
                return null;
            }

            var resource = album.ToResource();

            if (resource.Path.IsNotNullOrWhiteSpace())
            {
                resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
            }

            resource.EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(album);
            FetchAndLinkAlbumStatistics(resource);

            return resource;
        }

        private void FetchAndLinkAlbumStatistics(AlbumResource resource)
        {
            LinkAlbumStatistics(resource, _musicStatisticsService.AlbumStatistics(resource.Id));
        }

        private void LinkAlbumStatistics(List<AlbumResource> resources, Dictionary<int, MusicStatistics> sDict)
        {
            foreach (var album in resources)
            {
                if (sDict.TryGetValue(album.Id, out var stats))
                {
                    LinkAlbumStatistics(album, stats);
                }
            }
        }

        private static void LinkAlbumStatistics(AlbumResource resource, MusicStatistics musicStatistics)
        {
            resource.Statistics = musicStatistics.ToResource();
            resource.HasFile = musicStatistics.TrackFileCount > 0;
            resource.SizeOnDisk = musicStatistics.SizeOnDisk;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AlbumResource> AddAlbum([FromBody] AlbumResource albumResource)
        {
            var album = _addAlbumService.AddAlbum(albumResource.ToModel());
            return Created(album.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<AlbumResource> UpdateAlbum([FromBody] AlbumResource albumResource)
        {
            var album = _albumService.GetAlbum(albumResource.Id);
            var updatedAlbum = _albumService.UpdateAlbum(albumResource.ToModel(album));
            var resource = MapToResource(updatedAlbum);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteAlbum(int id, bool deleteFiles = false)
        {
            _albumService.DeleteAlbum(id, deleteFiles);
            return NoContent();
        }

        [NonAction]
        public void Handle(AlbumAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album));
        }

        [NonAction]
        public void Handle(AlbumEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album));
        }

        [NonAction]
        public void Handle(AlbumsDeletedEvent message)
        {
            foreach (var album in message.Albums)
            {
                BroadcastResourceChange(ModelAction.Deleted, album.Id);
            }
        }
    }
}
