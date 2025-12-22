using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.BookStats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Books
{
    [V3ApiController]
    public class BookController : RestControllerWithSignalR<BookResource, Book>,
                                  IHandle<BookAddedEvent>,
                                  IHandle<BookEditedEvent>,
                                  IHandle<BooksDeletedEvent>,
                                  IHandle<BooksBulkEditedEvent>
    {
        private readonly IBookService _bookService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;
        private readonly IBookStatisticsService _bookStatisticsService;

        public BookController(IBroadcastSignalRMessage signalRBroadcaster,
                              IBookService bookService,
                              IRootFolderService rootFolderService,
                              IHierarchicalMonitoringService monitoringService,
                              IBookStatisticsService bookStatisticsService,
                              RootFolderValidator rootFolderValidator,
                              MappedNetworkDriveValidator mappedNetworkDriveValidator,
                              RecycleBinValidator recycleBinValidator,
                              SystemFolderValidator systemFolderValidator,
                              QualityProfileExistsValidator qualityProfileExistsValidator,
                              RootFolderExistsValidator rootFolderExistsValidator)
            : base(signalRBroadcaster)
        {
            _bookService = bookService;
            _rootFolderService = rootFolderService;
            _monitoringService = monitoringService;
            _bookStatisticsService = bookStatisticsService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(recycleBinValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => s.Path.IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<BookResource> GetBooks(int? authorId = null, int? seriesId = null)
        {
            List<Book> books;

            if (authorId.HasValue)
            {
                books = _bookService.FindByAuthorId(authorId.Value);
            }
            else if (seriesId.HasValue)
            {
                books = _bookService.FindBySeriesId(seriesId.Value);
            }
            else
            {
                books = _bookService.GetAllBooks();
            }

            var resources = books.ToResource();
            var rootFolders = _rootFolderService.All();
            var bookStats = _bookStatisticsService.BookStatistics();
            var sdict = bookStats.ToDictionary(x => x.BookId);

            for (var i = 0; i < resources.Count; i++)
            {
                resources[i].RootFolderPath = _rootFolderService.GetBestRootFolderPath(resources[i].Path, rootFolders);
                resources[i].EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(books[i]);
            }

            LinkBookStatistics(resources, sdict);

            return resources;
        }

        protected override BookResource GetResourceById(int id)
        {
            var book = _bookService.GetBook(id);
            return MapToResource(book);
        }

        private BookResource MapToResource(Book book)
        {
            if (book == null)
            {
                return null;
            }

            var resource = book.ToResource();
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
            resource.EffectivelyMonitored = _monitoringService.IsEffectivelyMonitored(book);
            FetchAndLinkBookStatistics(resource);

            return resource;
        }

        private void FetchAndLinkBookStatistics(BookResource resource)
        {
            LinkBookStatistics(resource, _bookStatisticsService.BookStatistics(resource.Id));
        }

        private void LinkBookStatistics(List<BookResource> resources, Dictionary<int, BookStatistics> sDict)
        {
            foreach (var book in resources)
            {
                if (sDict.TryGetValue(book.Id, out var stats))
                {
                    LinkBookStatistics(book, stats);
                }
            }
        }

        private static void LinkBookStatistics(BookResource resource, BookStatistics bookStatistics)
        {
            resource.Statistics = bookStatistics.ToResource();
            resource.HasFile = bookStatistics.BookFileCount > 0;
            resource.SizeOnDisk = bookStatistics.SizeOnDisk;
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<BookResource> AddBook([FromBody] BookResource bookResource)
        {
            var book = _bookService.AddBook(bookResource.ToModel());
            return Created(book.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<BookResource> UpdateBook([FromBody] BookResource bookResource)
        {
            var book = _bookService.GetBook(bookResource.Id);
            var updatedBook = _bookService.UpdateBook(bookResource.ToModel(book));
            var resource = MapToResource(updatedBook);

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteBook(int id, bool deleteFiles = false)
        {
            _bookService.DeleteBook(id, deleteFiles);
            return NoContent();
        }

        [NonAction]
        public void Handle(BookAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book));
        }

        [NonAction]
        public void Handle(BookEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.Book));
        }

        [NonAction]
        public void Handle(BooksDeletedEvent message)
        {
            foreach (var book in message.Books)
            {
                BroadcastResourceChange(ModelAction.Deleted, book.Id);
            }
        }

        [NonAction]
        public void Handle(BooksBulkEditedEvent message)
        {
            foreach (var book in message.Books)
            {
                BroadcastResourceChange(ModelAction.Updated, MapToResource(book));
            }
        }
    }
}
