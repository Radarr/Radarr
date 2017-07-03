using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Api.REST;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.SignalR;
using System;

namespace NzbDrone.Api.TrackFiles
{
    public class TrackFileModule : NzbDroneRestModuleWithSignalR<TrackFileResource, TrackFile>,
                                 IHandle<TrackFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly ISeriesService _seriesService;
        private readonly IArtistService _artistService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public TrackFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDiskProvider diskProvider,
                             IRecycleBinProvider recycleBinProvider,
                             ISeriesService seriesService,
                             IArtistService artistService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             Logger logger)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _seriesService = seriesService;
            _artistService = artistService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
            GetResourceById = GetTrackFile;
            GetResourceAll = GetTrackFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteTrackFile;
        }

        private TrackFileResource GetTrackFile(int id)
        {
            var trackFile = _mediaFileService.Get(id);
            var artist = _artistService.GetArtist(trackFile.ArtistId);

            return trackFile.ToResource(artist, _qualityUpgradableSpecification);
        }

        private List<TrackFileResource> GetTrackFiles()
        {
            if (!Request.Query.ArtistId.HasValue)
            {
                throw new BadRequestException("artistId is missing");
            }

            var artistId = (int)Request.Query.ArtistId;

            var artist = _artistService.GetArtist(artistId);

            return _mediaFileService.GetFilesByArtist(artistId).ConvertAll(f => f.ToResource(artist, _qualityUpgradableSpecification));
        }

        private void SetQuality(TrackFileResource trackFileResource)
        {
            var trackFile = _mediaFileService.Get(trackFileResource.Id);
            trackFile.Quality = trackFileResource.Quality;
            _mediaFileService.Update(trackFile);
        }

        private void DeleteTrackFile(int id)
        {
            throw new NotImplementedException();
            //var episodeFile = _mediaFileService.Get(id);
            //var series = _seriesService.GetSeries(episodeFile.SeriesId);
            //var fullPath = Path.Combine(series.Path, episodeFile.RelativePath);
            //var subfolder = _diskProvider.GetParentFolder(series.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));

            //_logger.Info("Deleting episode file: {0}", fullPath);
            //_recycleBinProvider.DeleteFile(fullPath, subfolder);
            //_mediaFileService.Delete(episodeFile, DeleteMediaFileReason.Manual);
        }

        public void Handle(TrackFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.TrackFile.Id);
        }
    }
}