using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;

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
        private readonly IAddArtistService _addArtistService;
        private readonly IArtistStatisticsService _artistStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;

        public ArtistModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IArtistService artistService,
                            IAddArtistService addArtistService,
                            IArtistStatisticsService artistStatisticsService,
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
            _addArtistService = addArtistService;
            _artistStatisticsService = artistStatisticsService;

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
            PostValidator.RuleFor(s => s.ForeignArtistId).NotEqual("").SetValidator(artistExistsValidator);

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
            FetchAndLinkArtistStatistics(resource);
            //PopulateAlternateTitles(resource);

            return resource;
        }

        private List<ArtistResource> AllArtist()
        {
            var artistStats = _artistStatisticsService.ArtistStatistics();
            var artistResources = _artistService.GetAllArtists().ToResource();

            MapCoversToLocal(artistResources.ToArray());
            LinkArtistStatistics(artistResources, artistStats);
            //PopulateAlternateTitles(seriesResources);

            return artistResources;
        }

        private int AddArtist(ArtistResource artistResource)
        {
            var model = artistResource.ToModel();

            return _addArtistService.AddArtist(model).Id;
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

        private void FetchAndLinkArtistStatistics(ArtistResource resource)
        {
            LinkArtistStatistics(resource, _artistStatisticsService.ArtistStatistics(resource.Id));
        }


        private void LinkArtistStatistics(List<ArtistResource> resources, List<ArtistStatistics> artistStatistics)
        {
            var dictArtistStats = artistStatistics.ToDictionary(v => v.ArtistId);

            foreach (var artist in resources)
            {
                var stats = dictArtistStats.GetValueOrDefault(artist.Id);
                if (stats == null) continue;

                LinkArtistStatistics(artist, stats);
            }
        }

        private void LinkArtistStatistics(ArtistResource resource, ArtistStatistics artistStatistics)
        {
            resource.TotalTrackCount = artistStatistics.TotalTrackCount;
            resource.TrackCount = artistStatistics.TrackCount;
            resource.TrackFileCount = artistStatistics.TrackFileCount;
            resource.SizeOnDisk = artistStatistics.SizeOnDisk;
            resource.AlbumCount = artistStatistics.AlbumCount;

            //if (artistStatistics.AlbumStatistics != null)
            //{
            //    var dictSeasonStats = artistStatistics.SeasonStatistics.ToDictionary(v => v.SeasonNumber);

            //    foreach (var album in resource.Albums)
            //    {
            //        album.Statistics = dictSeasonStats.GetValueOrDefault(album.Id).ToResource();
            //    }
            //}
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedTrack.ArtistId);
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            BroadcastResourceChange(ModelAction.Updated, message.TrackFile.ArtistId); 
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
