using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using NzbDrone.Core.Exceptions;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Lidarr.Api.V1.TrackFiles
{
    public class TrackFileModule : LidarrRestModuleWithSignalR<TrackFileResource, TrackFile>,
                                 IHandle<TrackFileAddedEvent>,
                                 IHandle<TrackFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IAudioTagService _audioTagService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public TrackFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                               IMediaFileService mediaFileService,
                               IDeleteMediaFiles mediaFileDeletionService,
                               IAudioTagService audioTagService,
                               IArtistService artistService,
                               IAlbumService albumService,
                               IUpgradableSpecification upgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _audioTagService = audioTagService;
            _artistService = artistService;
            _albumService = albumService;
            _upgradableSpecification = upgradableSpecification;

            GetResourceById = GetTrackFile;
            GetResourceAll = GetTrackFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteTrackFile;

            Put["/editor"] = trackFiles => SetQuality();
            Delete["/bulk"] = trackFiles => DeleteTrackFiles();
        }

        private TrackFileResource MapToResource(TrackFile trackFile)
        {
            if (trackFile.AlbumId > 0 && trackFile.Artist != null && trackFile.Artist.Value != null)
            {
                return trackFile.ToResource(trackFile.Artist.Value, _upgradableSpecification);
            }
            else
            {
                return trackFile.ToResource();
            }
        }

        private TrackFileResource GetTrackFile(int id)
        {
            var resource = MapToResource(_mediaFileService.Get(id));
            resource.AudioTags = _audioTagService.ReadTags(resource.Path);
            return resource;
        }

        private List<TrackFileResource> GetTrackFiles()
        {
            var artistIdQuery = Request.Query.ArtistId;
            var trackFileIdsQuery = Request.Query.TrackFileIds;
            var albumIdQuery = Request.Query.AlbumId;
            var unmappedQuery = Request.Query.Unmapped;

            if (!artistIdQuery.HasValue && !trackFileIdsQuery.HasValue && !albumIdQuery.HasValue  && !unmappedQuery.HasValue)
            {
                throw new Lidarr.Http.REST.BadRequestException("artistId, albumId, trackFileIds or unmapped must be provided");
            }

            if (unmappedQuery.HasValue && Convert.ToBoolean(unmappedQuery.Value))
            {
                var files = _mediaFileService.GetUnmappedFiles();
                return files.ConvertAll(f => MapToResource(f));
            }

            if (artistIdQuery.HasValue && !albumIdQuery.HasValue)
            {
                int artistId = Convert.ToInt32(artistIdQuery.Value);
                var artist = _artistService.GetArtist(artistId);

                return _mediaFileService.GetFilesByArtist(artistId).ConvertAll(f => f.ToResource(artist, _upgradableSpecification));
            }

            if (albumIdQuery.HasValue)
            {
                string albumIdValue = albumIdQuery.Value.ToString();

                var albumIds = albumIdValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => Convert.ToInt32(e))
                    .ToList();

                var result = new List<TrackFileResource>();
                foreach (var albumId in albumIds)
                {
                    var album = _albumService.GetAlbum(albumId);
                    var albumArtist = _artistService.GetArtist(album.ArtistId);
                    result.AddRange(_mediaFileService.GetFilesByAlbum(album.Id).ConvertAll(f => f.ToResource(albumArtist, _upgradableSpecification)));
                }
                
                return result;
            }

            else
            {
                string trackFileIdsValue = trackFileIdsQuery.Value.ToString();

                var trackFileIds = trackFileIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => Convert.ToInt32(e))
                                                        .ToList();

                // trackfiles will come back with the artist already populated
                var trackFiles = _mediaFileService.Get(trackFileIds);
                return trackFiles.ConvertAll(e => MapToResource(e));
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
            var trackFiles = _mediaFileService.Get(resource.TrackFileIds);

            foreach (var trackFile in trackFiles)
            {
                if (resource.Quality != null)
                {
                    trackFile.Quality = resource.Quality;
                }
            }

            _mediaFileService.Update(trackFiles);

            return trackFiles.ConvertAll(f => f.ToResource(trackFiles.First().Artist.Value, _upgradableSpecification))
                               .AsResponse(Nancy.HttpStatusCode.Accepted);
        }

        private void DeleteTrackFile(int id)
        {
            var trackFile = _mediaFileService.Get(id);

            if (trackFile == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Track file not found");
            }

            if (trackFile.AlbumId > 0 && trackFile.Artist != null && trackFile.Artist.Value != null)
            {
                _mediaFileDeletionService.DeleteTrackFile(trackFile.Artist.Value, trackFile);
            }
            else
            {
                _mediaFileDeletionService.DeleteTrackFile(trackFile, "Unmapped_Files");
            }
        }

        private Response DeleteTrackFiles()
        {
            var resource = Request.Body.FromJson<TrackFileListResource>();
            var trackFiles = _mediaFileService.Get(resource.TrackFileIds);
            var artist = trackFiles.First().Artist.Value;

            foreach (var trackFile in trackFiles)
            {
                _mediaFileDeletionService.DeleteTrackFile(artist, trackFile);
            }

            return new object().AsResponse();
        }

        public void Handle(TrackFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.TrackFile));
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, MapToResource(message.TrackFile));
        }
    }
}
