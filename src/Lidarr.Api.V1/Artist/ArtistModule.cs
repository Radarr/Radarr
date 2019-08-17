using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Lidarr.Api.V1.Albums;
using NzbDrone.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistModule : LidarrRestModuleWithSignalR<ArtistResource, NzbDrone.Core.Music.Artist>, 
                                IHandle<AlbumImportedEvent>,
                                IHandle<AlbumEditedEvent>,
                                IHandle<TrackFileDeletedEvent>,
                                IHandle<ArtistUpdatedEvent>,       
                                IHandle<ArtistEditedEvent>,  
                                IHandle<ArtistDeletedEvent>,
                                IHandle<ArtistRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>

    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IAddArtistService _addArtistService;
        private readonly IArtistStatisticsService _artistStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;

        public ArtistModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IArtistService artistService,
                            IAlbumService albumService,
                            IAddArtistService addArtistService,
                            IArtistStatisticsService artistStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IRootFolderService rootFolderService,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            ArtistPathValidator artistPathValidator,
                            ArtistExistsValidator artistExistsValidator,
                            ArtistAncestorValidator artistAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            ProfileExistsValidator profileExistsValidator,
                            MetadataProfileExistsValidator metadataProfileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _artistService = artistService;
            _albumService = albumService;
            _addArtistService = addArtistService;
            _artistStatisticsService = artistStatisticsService;

            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;

            GetResourceAll = AllArtists;
            GetResourceById = GetArtist;
            CreateResource = AddArtist;
            UpdateResource = UpdateArtist;
            DeleteResource = DeleteArtist;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));
            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.MetadataProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(artistPathValidator)
                           .SetValidator(artistAncestorValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);
            SharedValidator.RuleFor(s => s.MetadataProfileId).SetValidator(metadataProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.ArtistName).NotEmpty();
            PostValidator.RuleFor(s => s.ForeignArtistId).NotEmpty().SetValidator(artistExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private ArtistResource GetArtist(int id)
        {
            var artist = _artistService.GetArtist(id);
            return GetArtistResource(artist);
        }

        private ArtistResource GetArtistResource(NzbDrone.Core.Music.Artist artist)
        {
            if (artist == null) return null;

            var resource = artist.ToResource();
            MapCoversToLocal(resource);
            FetchAndLinkArtistStatistics(resource);
            LinkNextPreviousAlbums(resource);
            //PopulateAlternateTitles(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private List<ArtistResource> AllArtists()
        {
            var artistStats = _artistStatisticsService.ArtistStatistics();
            var artistsResources = _artistService.GetAllArtists().ToResource();

            MapCoversToLocal(artistsResources.ToArray());
            LinkNextPreviousAlbums(artistsResources.ToArray());
            LinkArtistStatistics(artistsResources, artistStats);
            //PopulateAlternateTitles(seriesResources);

            return artistsResources;
        }

        private int AddArtist(ArtistResource artistResource)
        {
            var artist = _addArtistService.AddArtist(artistResource.ToModel());

            return artist.Id;
        }

        private void UpdateArtist(ArtistResource artistResource)
        {
            var moveFiles = Request.GetBooleanQueryParameter("moveFiles");
            var artist = _artistService.GetArtist(artistResource.Id);

            if (moveFiles)
            {
                var sourcePath = artist.Path;
                var destinationPath = artistResource.Path;

                _commandQueueManager.Push(new MoveArtistCommand
                {
                    ArtistId = artist.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Trigger = CommandTrigger.Manual
                });
            }

            var model = artistResource.ToModel(artist);

            _artistService.UpdateArtist(model);

            BroadcastResourceChange(ModelAction.Updated, artistResource);
        }

        private void DeleteArtist(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _artistService.DeleteArtist(id, deleteFiles, addImportListExclusion);
        }

        private void MapCoversToLocal(params ArtistResource[] artists)
        {
            foreach (var artistResource in artists)
            {
                _coverMapper.ConvertToLocalUrls(artistResource.Id, MediaCoverEntity.Artist, artistResource.Images);
            }
        }

        private void LinkNextPreviousAlbums(params ArtistResource[] artists)
        {
            var nextAlbums = _albumService.GetNextAlbumsByArtistMetadataId(artists.Select(x => x.ArtistMetadataId));
            var lastAlbums = _albumService.GetLastAlbumsByArtistMetadataId(artists.Select(x => x.ArtistMetadataId));

            foreach (var artistResource in artists)
            {
                artistResource.NextAlbum = nextAlbums.FirstOrDefault(x => x.ArtistMetadataId == artistResource.ArtistMetadataId);
                artistResource.LastAlbum = lastAlbums.FirstOrDefault(x => x.ArtistMetadataId == artistResource.ArtistMetadataId);
            }
        }

        private void FetchAndLinkArtistStatistics(ArtistResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.ArtistStatistics(resource.Id));
        }

        private void LinkArtistStatistics(List<ArtistResource> resources, List<ArtistStatistics> artistStatistics)
        {
            foreach (var artist in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.ArtistId == artist.Id);
                if (stats == null) continue;

                LinkArtistStatistics(artist, stats);
            }
        }

        private void LinkArtistStatistics(ArtistResource resource, ArtistStatistics artistStatistics)
        {
            resource.Statistics = artistStatistics.ToResource();
        }

        //private void PopulateAlternateTitles(List<ArtistResource> resources)
        //{
        //    foreach (var resource in resources)
        //    {
        //        PopulateAlternateTitles(resource);
        //    }
        //}

        //private void PopulateAlternateTitles(ArtistResource resource)
        //{
        //    var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

        //    if (mappings == null) return;

        //    resource.AlternateTitles = mappings.Select(v => new AlternateTitleResource { Title = v.Title, SeasonNumber = v.SeasonNumber, SceneSeasonNumber = v.SceneSeasonNumber }).ToList();
        //}

        private void LinkRootFolderPath(ArtistResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }

        public void Handle(AlbumImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Artist));
        }

        public void Handle(AlbumEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Album.Artist.Value));
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.TrackFile.Artist.Value));
        }

        public void Handle(ArtistUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Artist));
        }

        public void Handle(ArtistEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Artist));
        }

        public void Handle(ArtistDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Artist.ToResource());
        }

        public void Handle(ArtistRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, GetArtistResource(message.Artist));
        }
    }
}
