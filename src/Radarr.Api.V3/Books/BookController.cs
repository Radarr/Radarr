using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.BookStats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Books
{
    [V3ApiController]
    public class BookController : BaseMediaCrudController<BookResource, Book>,
                                  IHandle<BookAddedEvent>,
                                  IHandle<BookEditedEvent>,
                                  IHandle<BooksDeletedEvent>,
                                  IHandle<BooksBulkEditedEvent>
    {
        private readonly IBookService _bookService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IHierarchicalMonitoringService _monitoringService;
        private readonly IBookStatisticsService _bookStatisticsService;

        protected override IBaseMediaService<Book> MediaService => _bookService;
        protected override IRootFolderService RootFolderService => _rootFolderService;

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

            SetupPathValidation(rootFolderValidator, mappedNetworkDriveValidator, recycleBinValidator, systemFolderValidator, rootFolderExistsValidator);
            SetupQualityValidation(qualityProfileExistsValidator);
            SetupTitleValidation();
        }

        protected override string GetPath(BookResource resource) => resource.Path;
        protected override string GetRootFolderPath(BookResource resource) => resource.RootFolderPath;
        protected override int GetQualityProfileId(BookResource resource) => resource.QualityProfileId;
        protected override string GetTitle(BookResource resource) => resource.Title;

        protected override BookResource MapToResource(Book book)
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

        protected override Book ResourceToModel(BookResource resource) => resource.ToModel();
        protected override Book ApplyResourceToModel(BookResource resource, Book book) => resource.ToModel(book);

        [HttpGet]
        public List<BookResource> GetBooks(int? authorId = null, int? bookSeriesId = null)
        {
            List<Book> books;

            if (authorId.HasValue)
            {
                books = _bookService.FindByAuthorId(authorId.Value);
            }
            else if (bookSeriesId.HasValue)
            {
                books = _bookService.FindByBookSeriesId(bookSeriesId.Value);
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
