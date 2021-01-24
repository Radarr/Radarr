using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Readarr.Http;
using Readarr.Http.Extensions;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Readarr.Api.V1.BookFiles
{
    public class BookFileModule : ReadarrRestModuleWithSignalR<BookFileResource, BookFile>,
                                 IHandle<BookFileAddedEvent>,
                                 IHandle<BookFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public BookFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                               IMediaFileService mediaFileService,
                               IDeleteMediaFiles mediaFileDeletionService,
                               IAudioTagService audioTagService,
                               IAuthorService authorService,
                               IBookService bookService,
                               IUpgradableSpecification upgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _audioTagService = audioTagService;
            _authorService = authorService;
            _bookService = bookService;
            _upgradableSpecification = upgradableSpecification;

            GetResourceById = GetBookFile;
            GetResourceAll = GetBookFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteBookFile;

            Put("/editor", trackFiles => SetQuality());
            Delete("/bulk", trackFiles => DeleteBookFiles());
        }

        private BookFileResource MapToResource(BookFile bookFile)
        {
            if (bookFile.EditionId > 0 && bookFile.Author != null && bookFile.Author.Value != null)
            {
                return bookFile.ToResource(bookFile.Author.Value, _upgradableSpecification);
            }
            else
            {
                return bookFile.ToResource();
            }
        }

        private BookFileResource GetBookFile(int id)
        {
            var resource = MapToResource(_mediaFileService.Get(id));
            resource.AudioTags = _audioTagService.ReadTags(resource.Path);
            return resource;
        }

        private List<BookFileResource> GetBookFiles()
        {
            var authorIdQuery = Request.Query.AuthorId;
            var bookFileIdsQuery = Request.Query.TrackFileIds;
            var bookIdQuery = Request.Query.BookId;
            var unmappedQuery = Request.Query.Unmapped;

            if (!authorIdQuery.HasValue && !bookFileIdsQuery.HasValue && !bookIdQuery.HasValue && !unmappedQuery.HasValue)
            {
                throw new Readarr.Http.REST.BadRequestException("authorId, bookId, bookFileIds or unmapped must be provided");
            }

            if (unmappedQuery.HasValue && Convert.ToBoolean(unmappedQuery.Value))
            {
                var files = _mediaFileService.GetUnmappedFiles();
                return files.ConvertAll(f => MapToResource(f));
            }

            if (authorIdQuery.HasValue && !bookIdQuery.HasValue)
            {
                int authorId = Convert.ToInt32(authorIdQuery.Value);
                var author = _authorService.GetAuthor(authorId);

                return _mediaFileService.GetFilesByAuthor(authorId).ConvertAll(f => f.ToResource(author, _upgradableSpecification));
            }

            if (bookIdQuery.HasValue)
            {
                string bookIdValue = bookIdQuery.Value.ToString();

                var bookIds = bookIdValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => Convert.ToInt32(e))
                    .ToList();

                var result = new List<BookFileResource>();
                foreach (var bookId in bookIds)
                {
                    var book = _bookService.GetBook(bookId);
                    var bookAuthor = _authorService.GetAuthor(book.AuthorId);
                    result.AddRange(_mediaFileService.GetFilesByBook(book.Id).ConvertAll(f => f.ToResource(bookAuthor, _upgradableSpecification)));
                }

                return result;
            }
            else
            {
                string bookFileIdsValue = bookFileIdsQuery.Value.ToString();

                var bookFileIds = bookFileIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => Convert.ToInt32(e))
                                                        .ToList();

                // trackfiles will come back with the author already populated
                var bookFiles = _mediaFileService.Get(bookFileIds);
                return bookFiles.ConvertAll(e => MapToResource(e));
            }
        }

        private void SetQuality(BookFileResource bookFileResource)
        {
            var bookFile = _mediaFileService.Get(bookFileResource.Id);
            bookFile.Quality = bookFileResource.Quality;
            _mediaFileService.Update(bookFile);
        }

        private object SetQuality()
        {
            var resource = Request.Body.FromJson<BookFileListResource>();
            var bookFiles = _mediaFileService.Get(resource.BookFileIds);

            foreach (var bookFile in bookFiles)
            {
                if (resource.Quality != null)
                {
                    bookFile.Quality = resource.Quality;
                }
            }

            _mediaFileService.Update(bookFiles);

            return ResponseWithCode(bookFiles.ConvertAll(f => f.ToResource(bookFiles.First().Author.Value, _upgradableSpecification)),
                               Nancy.HttpStatusCode.Accepted);
        }

        private void DeleteBookFile(int id)
        {
            var bookFile = _mediaFileService.Get(id);

            if (bookFile == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Book file not found");
            }

            if (bookFile.EditionId > 0 && bookFile.Author != null && bookFile.Author.Value != null)
            {
                _mediaFileDeletionService.DeleteTrackFile(bookFile.Author.Value, bookFile);
            }
            else
            {
                _mediaFileDeletionService.DeleteTrackFile(bookFile, "Unmapped_Files");
            }
        }

        private object DeleteBookFiles()
        {
            var resource = Request.Body.FromJson<BookFileListResource>();
            var bookFiles = _mediaFileService.Get(resource.BookFileIds);
            var author = bookFiles.First().Author.Value;

            foreach (var bookFile in bookFiles)
            {
                _mediaFileDeletionService.DeleteTrackFile(author, bookFile);
            }

            return new object();
        }

        public void Handle(BookFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.BookFile));
        }

        public void Handle(BookFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, MapToResource(message.BookFile));
        }
    }
}
