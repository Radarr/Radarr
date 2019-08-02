using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistEditorModule : LidarrV1Module
    {
        private readonly IArtistService _artistService;
        private readonly IManageCommandQueue _commandQueueManager;

        public ArtistEditorModule(IArtistService artistService, IManageCommandQueue commandQueueManager)
            : base("/artist/editor")
        {
            _artistService = artistService;
            _commandQueueManager = commandQueueManager;
            Put["/"] = artist => SaveAll();
            Delete["/"] = artist => DeleteArtist();
        }

        private Response SaveAll()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();
            var artistToUpdate = _artistService.GetArtists(resource.ArtistIds);
            var artistToMove = new List<BulkMoveArtist>();

            foreach (var artist in artistToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    artist.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    artist.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.MetadataProfileId.HasValue)
                {
                    artist.MetadataProfileId = resource.MetadataProfileId.Value;
                }

                if (resource.AlbumFolder.HasValue)
                {
                    artist.AlbumFolder = resource.AlbumFolder.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    artist.RootFolderPath = resource.RootFolderPath;
                    artistToMove.Add(new BulkMoveArtist
                    {
                        ArtistId = artist.Id,
                        SourcePath = artist.Path
                    });

                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => artist.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => artist.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            artist.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            if (resource.MoveFiles && artistToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveArtistCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Artist = artistToMove
                });
            }

            return _artistService.UpdateArtists(artistToUpdate, !resource.MoveFiles)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }

        private Response DeleteArtist()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();

            foreach (var artistId in resource.ArtistIds)
            {
                _artistService.DeleteArtist(artistId, false);
            }

            return new object().AsResponse();
        }
    }
}
