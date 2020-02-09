using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Lidarr.Http.Extensions;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumModule : AlbumModuleWithSignalR,
        IHandle<AlbumGrabbedEvent>,
        IHandle<AlbumEditedEvent>,
        IHandle<AlbumUpdatedEvent>,
        IHandle<AlbumImportedEvent>,
        IHandle<TrackImportedEvent>,
        IHandle<TrackFileDeletedEvent>
    {
        protected readonly IReleaseService _releaseService;
        protected readonly IAddAlbumService _addAlbumService;

        public AlbumModule(IAlbumService albumService,
                           IAddAlbumService addAlbumService,
                           IReleaseService releaseService,
                           IArtistStatisticsService artistStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IUpgradableSpecification upgradableSpecification,
                           IBroadcastSignalRMessage signalRBroadcaster,
                           QualityProfileExistsValidator qualityProfileExistsValidator,
                           MetadataProfileExistsValidator metadataProfileExistsValidator)

        : base(albumService, artistStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
        {
            _releaseService = releaseService;
            _addAlbumService = addAlbumService;

            GetResourceAll = GetAlbums;
            CreateResource = AddAlbum;
            UpdateResource = UpdateAlbum;
            DeleteResource = DeleteAlbum;
            Put("/monitor", x => SetAlbumsMonitored());

            PostValidator.RuleFor(s => s.ForeignAlbumId).NotEmpty();
            PostValidator.RuleFor(s => s.Artist.QualityProfileId).SetValidator(qualityProfileExistsValidator);
            PostValidator.RuleFor(s => s.Artist.MetadataProfileId).SetValidator(metadataProfileExistsValidator);
            PostValidator.RuleFor(s => s.Artist.RootFolderPath).IsValidPath().When(s => s.Artist.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Artist.ForeignArtistId).NotEmpty();
        }

        private List<AlbumResource> GetAlbums()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var albumIdsQuery = Request.Query.AlbumIds;
            var foreignIdQuery = Request.Query.ForeignAlbumId;
            var includeAllArtistAlbumsQuery = Request.Query.IncludeAllArtistAlbums;

            if (!Request.Query.ArtistId.HasValue && !albumIdsQuery.HasValue && !foreignIdQuery.HasValue)
            {
                return MapToResource(_albumService.GetAllAlbums(), false);
            }

            if (artistIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);

                return MapToResource(_albumService.GetAlbumsByArtist(artistId), false);
            }

            if (foreignIdQuery.HasValue)
            {
                string foreignAlbumId = foreignIdQuery.Value.ToString();

                var album = _albumService.FindById(foreignAlbumId);

                if (album == null)
                {
                    return MapToResource(new List<Album>(), false);
                }

                if (includeAllArtistAlbumsQuery.HasValue && Convert.ToBoolean(includeAllArtistAlbumsQuery.Value))
                {
                    return MapToResource(_albumService.GetAlbumsByArtist(album.ArtistId), false);
                }
                else
                {
                    return MapToResource(new List<Album> { album }, false);
                }
            }

            string albumIdsValue = albumIdsQuery.Value.ToString();

            var albumIds = albumIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_albumService.GetAlbums(albumIds), false);
        }

        private int AddAlbum(AlbumResource albumResource)
        {
            var album = _addAlbumService.AddAlbum(albumResource.ToModel());

            return album.Id;
        }

        private void UpdateAlbum(AlbumResource albumResource)
        {
            var album = _albumService.GetAlbum(albumResource.Id);

            var model = albumResource.ToModel(album);

            _albumService.UpdateAlbum(model);
            _releaseService.UpdateMany(model.AlbumReleases.Value);

            BroadcastResourceChange(ModelAction.Updated, model.Id);
        }

        private void DeleteAlbum(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _albumService.DeleteAlbum(id, deleteFiles, addImportListExclusion);
        }

        private object SetAlbumsMonitored()
        {
            var resource = Request.Body.FromJson<AlbumsMonitoredResource>();

            _albumService.SetMonitored(resource.AlbumIds, resource.Monitored);

            return ResponseWithCode(MapToResource(_albumService.GetAlbums(resource.AlbumIds), false), HttpStatusCode.Accepted);
        }

        public void Handle(AlbumGrabbedEvent message)
        {
            foreach (var album in message.Album.Albums)
            {
                var resource = album.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(AlbumEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(AlbumUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(AlbumDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Album.ToResource());
        }

        public void Handle(AlbumImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Album, true));
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.TrackInfo.Album.ToResource());
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.TrackFile.Album.Value, true));
        }
    }
}
