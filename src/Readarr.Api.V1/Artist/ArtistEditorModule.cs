using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Messaging.Commands;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Artist
{
    public class ArtistEditorModule : ReadarrV1Module
    {
        private readonly IAuthorService _authorService;
        private readonly IManageCommandQueue _commandQueueManager;

        public ArtistEditorModule(IAuthorService authorService, IManageCommandQueue commandQueueManager)
            : base("/artist/editor")
        {
            _authorService = authorService;
            _commandQueueManager = commandQueueManager;
            Put("/", artist => SaveAll());
            Delete("/", artist => DeleteArtist());
        }

        private object SaveAll()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();
            var artistToUpdate = _authorService.GetAuthors(resource.AuthorIds);
            var artistToMove = new List<BulkMoveAuthor>();

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

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    artist.RootFolderPath = resource.RootFolderPath;
                    artistToMove.Add(new BulkMoveAuthor
                    {
                        AuthorId = artist.Id,
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
                _commandQueueManager.Push(new BulkMoveAuthorCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Author = artistToMove
                });
            }

            return ResponseWithCode(_authorService.UpdateAuthors(artistToUpdate, !resource.MoveFiles)
                                 .ToResource(),
                                 HttpStatusCode.Accepted);
        }

        private object DeleteArtist()
        {
            var resource = Request.Body.FromJson<ArtistEditorResource>();

            foreach (var authorId in resource.AuthorIds)
            {
                _authorService.DeleteAuthor(authorId, false);
            }

            return new object();
        }
    }
}
