using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using System;
using System.Collections.Generic;

namespace NzbDrone.Api.Music
{
    public class ArtistModule : NzbDroneRestModuleWithSignalR<ArtistResource, Core.Music.Artist>,
                                IHandle<TrackImportedEvent>,
                                IHandle<TrackFileDeletedEvent>,
                                IHandle<ArtistUpdatedEvent>,
                                IHandle<ArtistEditedEvent>,
                                IHandle<ArtistDeletedEvent>,
                                IHandle<ArtistRenamedEvent>
                                //IHandle<MediaCoversUpdatedEvent>
    {
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addSeriesService;
        private readonly ISeriesStatisticsService _seriesStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;

        public ArtistModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IArtistService artistService,
                            IAddArtistService addSeriesService,
                            ISeriesStatisticsService seriesStatisticsService,
                            IMapCoversToLocal coverMapper,
                            RootFolderValidator rootFolderValidator,
                            ArtistPathValidator seriesPathValidator,
                            ArtistExistsValidator artistExistsValidator,
                            DroneFactoryValidator droneFactoryValidator,
                            SeriesAncestorValidator seriesAncestorValidator,
                            ProfileExistsValidator profileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _artistService = artistService;
            _addSeriesService = addSeriesService;
            _seriesStatisticsService = seriesStatisticsService;

            _coverMapper = coverMapper;

            GetResourceAll = AllArtist;
            GetResourceById = GetArtist;
            CreateResource = AddArtist;
            UpdateResource = UpdatArtist;
            DeleteResource = DeleteArtist;

            Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.ProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(seriesPathValidator)
                           .SetValidator(droneFactoryValidator)
                           .SetValidator(seriesAncestorValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.ProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.SpotifyId).NotEqual("").SetValidator(artistExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private ArtistResource GetArtist(int id)
        {
            var artist = _artistService.GetArtist(id);
            return MapToResource(artist);
        }

        private ArtistResource MapToResource(Artist artist)
        {
            if (artist == null) return null;

            var resource = artist.ToResource();
            MapCoversToLocal(resource);
            //FetchAndLinkSeriesStatistics(resource);
            //PopulateAlternateTitles(resource);

            return resource;
        }

        private List<ArtistResource> AllArtist()
        {
            //var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var artistResources = _artistService.GetAllArtists().ToResource();
            MapCoversToLocal(artistResources.ToArray());
            //LinkSeriesStatistics(seriesResources, seriesStats);
            //PopulateAlternateTitles(seriesResources);

            return artistResources;
        }

        private int AddArtist(ArtistResource artistResource)
        {
            var model = artistResource.ToModel();

            return _addSeriesService.AddArtist(model).Id;
        }

        private void UpdatArtist(ArtistResource artistResource)
        {
            var model = artistResource.ToModel(_artistService.GetArtist(artistResource.Id));

            _artistService.UpdateArtist(model);

            BroadcastResourceChange(ModelAction.Updated, artistResource.Id);
        }

        private void DeleteArtist(int id)
        {
            var deleteFiles = false;
            var deleteFilesQuery = Request.Query.deleteFiles;

            if (deleteFilesQuery.HasValue)
            {
                deleteFiles = Convert.ToBoolean(deleteFilesQuery.Value);
            }

            _artistService.DeleteArtist(id, deleteFiles);
        }

        private void MapCoversToLocal(params ArtistResource[] artists)
        {
            foreach (var artistResource in artists)
            {
                _coverMapper.ConvertToLocalUrls(artistResource.Id, artistResource.Images);
            }
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedTrack.Id); // TODO: Ensure we can pass DB ID instead of Metadata ID (SpotifyID)
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            BroadcastResourceChange(ModelAction.Updated, message.TrackFile.Id); // TODO: Ensure we can pass DB ID instead of Metadata ID (SpotifyID)
        }

        public void Handle(ArtistUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        }

        public void Handle(ArtistEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        }

        public void Handle(ArtistDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Artist.ToResource());
        }

        public void Handle(ArtistRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        }

        //public void Handle(MediaCoversUpdatedEvent message)
        //{
        //    BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        //}

    }
}
