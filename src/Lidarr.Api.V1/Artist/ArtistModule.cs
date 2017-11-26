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
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Lidarr.Api.V1.Albums;
using NzbDrone.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using Lidarr.Http.Mapping;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistModule : LidarrRestModuleWithSignalR<ArtistResource, NzbDrone.Core.Music.Artist>, 
                                IHandle<TrackImportedEvent>, 
                                IHandle<TrackFileDeletedEvent>,
                                IHandle<ArtistUpdatedEvent>,       
                                IHandle<ArtistEditedEvent>,  
                                IHandle<ArtistDeletedEvent>,
                                IHandle<ArtistRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>

    {
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IArtistStatisticsService _artistStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IAlbumService _albumService;

        public ArtistModule(IBroadcastSignalRMessage signalRBroadcaster,
                            IArtistService artistService,
                            IAddArtistService addArtistService,
                            IArtistStatisticsService artistStatisticsService,
                            IMapCoversToLocal coverMapper,
                            IAlbumService albumService,
                            RootFolderValidator rootFolderValidator,
                            ArtistPathValidator artistPathValidator,
                            ArtistExistsValidator artistExistsValidator,
                            ArtistAncestorValidator artistAncestorValidator,
                            ProfileExistsValidator profileExistsValidator,
                            LanguageProfileExistsValidator languageProfileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _artistService = artistService;
            _addArtistService = addArtistService;
            _artistStatisticsService = artistStatisticsService;

            _coverMapper = coverMapper;
            _albumService = albumService;

            GetResourceAll = AllArtists;
            GetResourceById = GetArtist;
            CreateResource = AddArtist;
            UpdateResource = UpdateArtist;
            DeleteResource = DeleteArtist;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(artistPathValidator)
                           .SetValidator(artistAncestorValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);
            SharedValidator.RuleFor(s => s.LanguageProfileId).SetValidator(languageProfileExistsValidator);

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
            //PopulateAlternateTitles(resource);

            return resource;
        }

        private List<ArtistResource> AllArtists()
        {
            var artistStats = _artistStatisticsService.ArtistStatistics();
            var artistsResources = _artistService.GetAllArtists().ToResource();

            MapCoversToLocal(artistsResources.ToArray());
            //MapAlbums(artistsResources.ToArray());
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
            var model = artistResource.ToModel(_artistService.GetArtist(artistResource.Id));

            _artistService.UpdateArtist(model);

            BroadcastResourceChange(ModelAction.Updated, artistResource);
        }

        private void DeleteArtist(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");

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
            foreach (var artist in resources)
            {
                var stats = artistStatistics.SingleOrDefault(ss => ss.ArtistId == artist.Id);
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

            if (artistStatistics.AlbumStatistics != null)
            {
               foreach (var album in resource.Albums)
                {
                    album.Statistics = artistStatistics.AlbumStatistics.SingleOrDefault(s => s.AlbumId == album.Id).ToResource();
                }
            }
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

        public void Handle(MediaCoversUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Artist.Id);
        }
    }
}
