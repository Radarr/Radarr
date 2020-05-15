using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Books
{
    public class BookModule : BookModuleWithSignalR,
        IHandle<BookGrabbedEvent>,
        IHandle<BookEditedEvent>,
        IHandle<BookUpdatedEvent>,
        IHandle<BookImportedEvent>,
        IHandle<TrackImportedEvent>,
        IHandle<BookFileDeletedEvent>
    {
        protected readonly IAuthorService _authorService;
        protected readonly IAddBookService _addBookService;

        public BookModule(IAuthorService authorService,
                           IBookService bookService,
                           IAddBookService addBookService,
                           IAuthorStatisticsService authorStatisticsService,
                           IMapCoversToLocal coverMapper,
                           IUpgradableSpecification upgradableSpecification,
                           IBroadcastSignalRMessage signalRBroadcaster,
                           QualityProfileExistsValidator qualityProfileExistsValidator,
                           MetadataProfileExistsValidator metadataProfileExistsValidator)

        : base(bookService, authorStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
        {
            _authorService = authorService;
            _addBookService = addBookService;

            GetResourceAll = GetBooks;
            CreateResource = AddBook;
            UpdateResource = UpdateBook;
            DeleteResource = DeleteBook;
            Put("/monitor", x => SetBooksMonitored());

            PostValidator.RuleFor(s => s.ForeignBookId).NotEmpty();
            PostValidator.RuleFor(s => s.Author.QualityProfileId).SetValidator(qualityProfileExistsValidator);
            PostValidator.RuleFor(s => s.Author.MetadataProfileId).SetValidator(metadataProfileExistsValidator);
            PostValidator.RuleFor(s => s.Author.RootFolderPath).IsValidPath().When(s => s.Author.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Author.ForeignAuthorId).NotEmpty();
        }

        private List<BookResource> GetBooks()
        {
            var authorIdQuery = Request.Query.AuthorId;
            var bookIdsQuery = Request.Query.BookIds;
            var slugQuery = Request.Query.TitleSlug;
            var includeAllAuthorBooksQuery = Request.Query.IncludeAllAuthorBooks;

            if (!Request.Query.AuthorId.HasValue && !bookIdsQuery.HasValue && !slugQuery.HasValue)
            {
                var books = _bookService.GetAllBooks();

                var authors = _authorService.GetAllAuthors().ToDictionary(x => x.AuthorMetadataId);

                foreach (var book in books)
                {
                    book.Author = authors[book.AuthorMetadataId];
                }

                return MapToResource(books, false);
            }

            if (authorIdQuery.HasValue)
            {
                int authorId = Convert.ToInt32(authorIdQuery.Value);

                return MapToResource(_bookService.GetBooksByAuthor(authorId), false);
            }

            if (slugQuery.HasValue)
            {
                string titleSlug = slugQuery.Value.ToString();

                var book = _bookService.FindBySlug(titleSlug);

                if (book == null)
                {
                    return MapToResource(new List<Book>(), false);
                }

                if (includeAllAuthorBooksQuery.HasValue && Convert.ToBoolean(includeAllAuthorBooksQuery.Value))
                {
                    return MapToResource(_bookService.GetBooksByAuthor(book.AuthorId), false);
                }
                else
                {
                    return MapToResource(new List<Book> { book }, false);
                }
            }

            string bookIdsValue = bookIdsQuery.Value.ToString();

            var bookIds = bookIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_bookService.GetBooks(bookIds), false);
        }

        private int AddBook(BookResource bookResource)
        {
            var book = _addBookService.AddBook(bookResource.ToModel());

            return book.Id;
        }

        private void UpdateBook(BookResource bookResource)
        {
            var book = _bookService.GetBook(bookResource.Id);

            var model = bookResource.ToModel(book);

            _bookService.UpdateBook(model);

            BroadcastResourceChange(ModelAction.Updated, model.Id);
        }

        private void DeleteBook(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _bookService.DeleteBook(id, deleteFiles, addImportListExclusion);
        }

        private object SetBooksMonitored()
        {
            var resource = Request.Body.FromJson<BooksMonitoredResource>();

            _bookService.SetMonitored(resource.BookIds, resource.Monitored);

            return ResponseWithCode(MapToResource(_bookService.GetBooks(resource.BookIds), false), HttpStatusCode.Accepted);
        }

        public void Handle(BookGrabbedEvent message)
        {
            foreach (var book in message.Book.Books)
            {
                var resource = book.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(BookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book, true));
        }

        public void Handle(BookUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book, true));
        }

        public void Handle(BookDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Book.ToResource());
        }

        public void Handle(BookImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book, true));
        }

        public void Handle(TrackImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.BookInfo.Book.ToResource());
        }

        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.BookFile.Book.Value, true));
        }
    }
}
