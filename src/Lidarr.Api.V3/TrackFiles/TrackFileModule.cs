using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using NLog;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using Lidarr.Http.REST;

namespace Lidarr.Api.V3.TrackFiles
{
    public class TrackModule : LidarrRestModuleWithSignalR<TrackFileResource, TrackFile>,
                                 IHandle<TrackFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public TrackModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IRecycleBinProvider recycleBinProvider,
                             IArtistService artistService,
                             IAlbumService albumService,
                             IUpgradableSpecification upgradableSpecification,
                             Logger logger)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _recycleBinProvider = recycleBinProvider;
            _artistService = artistService;
            _albumService = albumService;
            _upgradableSpecification = upgradableSpecification;
            _logger = logger;

            GetResourceById = GetTrackFile;
            GetResourceAll = GetTrackFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteTrackFile;

            Put["/editor"] = trackFiles => SetQuality();
            Delete["/bulk"] = trackFiles => DeleteTrackFiles();
        }

        private TrackFileResource GetTrackFile(int id)
        {
            var trackFile = _mediaFileService.Get(id);
            var artist = _artistService.GetArtist(trackFile.ArtistId);

            return trackFile.ToResource(artist, _upgradableSpecification);
        }

        private List<TrackFileResource> GetTrackFiles()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var trackFileIdsQuery = Request.Query.TrackFileIds;
            var albumIdQuery = Request.Query.AlbumId;

            if (!artistIdQuery.HasValue && !trackFileIdsQuery.HasValue && !albumIdQuery.HasValue)
            {
                throw new BadRequestException("artistId, albumId, or trackFileIds must be provided");
            }

            if (artistIdQuery.HasValue && !albumIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);
                var artist = _artistService.GetArtist(artistId);

                return _mediaFileService.GetFilesByArtist(artistId).ConvertAll(f => f.ToResource(artist, _upgradableSpecification));
            }

            if (albumIdQuery.HasValue)
            {
                int albumId = Convert.ToInt32(albumIdQuery.Value);
                var album = _albumService.GetAlbum(albumId);

                return _mediaFileService.GetFilesByAlbum(album.ArtistId, album.Id).ConvertAll(f => f.ToResource(album.Artist, _upgradableSpecification));
            }

            else
            {
                string episodeFileIdsValue = trackFileIdsQuery.Value.ToString();

                var trackFileIds = episodeFileIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => Convert.ToInt32(e))
                                                        .ToList();

                var trackFiles = _mediaFileService.Get(trackFileIds);

                return trackFiles.GroupBy(e => e.ArtistId)
                                   .SelectMany(f => f.ToList()
                                                     .ConvertAll( e => e.ToResource(_artistService.GetArtist(f.Key), _upgradableSpecification)))
                                   .ToList();
            }
        }

        private void SetQuality(TrackFileResource trackFileResource)
        {
            var trackFile = _mediaFileService.Get(trackFileResource.Id);
            trackFile.Quality = trackFileResource.Quality;
            _mediaFileService.Update(trackFile);
        }

        private Response SetQuality()
        {
            var resource = Request.Body.FromJson<TrackFileListResource>();
            var trackFiles = _mediaFileService.GetFiles(resource.TrackFileIds);

            foreach (var trackFile in trackFiles)
            {
                if (resource.Language != null)
                {
                    trackFile.Language = resource.Language;
                }

                if (resource.Quality != null)
                {
                    trackFile.Quality = resource.Quality;
                }
            }

            _mediaFileService.Update(trackFiles);

            var series = _artistService.GetArtist(trackFiles.First().ArtistId);

            return trackFiles.ConvertAll(f => f.ToResource(series, _upgradableSpecification))
                               .AsResponse(HttpStatusCode.Accepted);
        }

        private void DeleteTrackFile(int id)
        {
            var trackFile = _mediaFileService.Get(id);
            var artist = _artistService.GetArtist(trackFile.ArtistId);
            var fullPath = Path.Combine(artist.Path, trackFile.RelativePath);

            _logger.Info("Deleting track file: {0}", fullPath);
            _recycleBinProvider.DeleteFile(fullPath);
            _mediaFileService.Delete(trackFile, DeleteMediaFileReason.Manual);
        }

        private Response DeleteTrackFiles()
        {
            var resource = Request.Body.FromJson<TrackFileListResource>();
            var trackFiles = _mediaFileService.GetFiles(resource.TrackFileIds);
            var artist = _artistService.GetArtist(trackFiles.First().ArtistId);

            foreach (var trackFile in trackFiles)
            {
                var fullPath = Path.Combine(artist.Path, trackFile.RelativePath);

                _logger.Info("Deleting track file: {0}", fullPath);
                _recycleBinProvider.DeleteFile(fullPath);
                _mediaFileService.Delete(trackFile, DeleteMediaFileReason.Manual);
            }

            return new object().AsResponse();
        }

        public void Handle(TrackFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.TrackFile.Id);
        }
    }
}
