using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Messaging.Commands;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Author
{
    public class AuthorEditorModule : ReadarrV1Module
    {
        private readonly IAuthorService _authorService;
        private readonly IManageCommandQueue _commandQueueManager;

        public AuthorEditorModule(IAuthorService authorService, IManageCommandQueue commandQueueManager)
            : base("/author/editor")
        {
            _authorService = authorService;
            _commandQueueManager = commandQueueManager;
            Put("/", author => SaveAll());
            Delete("/", author => DeleteAuthor());
        }

        private object SaveAll()
        {
            var resource = Request.Body.FromJson<AuthorEditorResource>();
            var authorsToUpdate = _authorService.GetAuthors(resource.AuthorIds);
            var authorsToMove = new List<BulkMoveAuthor>();

            foreach (var author in authorsToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    author.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    author.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.MetadataProfileId.HasValue)
                {
                    author.MetadataProfileId = resource.MetadataProfileId.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    author.RootFolderPath = resource.RootFolderPath;
                    authorsToMove.Add(new BulkMoveAuthor
                    {
                        AuthorId = author.Id,
                        SourcePath = author.Path
                    });
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => author.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => author.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            author.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            if (resource.MoveFiles && authorsToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveAuthorCommand
                {
                    DestinationRootFolder = resource.RootFolderPath,
                    Author = authorsToMove
                });
            }

            return ResponseWithCode(_authorService.UpdateAuthors(authorsToUpdate, !resource.MoveFiles)
                                 .ToResource(),
                                 HttpStatusCode.Accepted);
        }

        private object DeleteAuthor()
        {
            var resource = Request.Body.FromJson<AuthorEditorResource>();

            foreach (var authorId in resource.AuthorIds)
            {
                _authorService.DeleteAuthor(authorId, false);
            }

            return new object();
        }
    }
}
