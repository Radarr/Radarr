using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Author;
using Readarr.Http;

namespace Readarr.Api.V1.Books
{
    public abstract class BookModuleWithSignalR : ReadarrRestModuleWithSignalR<BookResource, Book>
    {
        protected readonly IBookService _bookService;
        protected readonly IAuthorStatisticsService _authorStatisticsService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;
        protected readonly IMapCoversToLocal _coverMapper;

        protected BookModuleWithSignalR(IBookService bookService,
                                           IAuthorStatisticsService authorStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _bookService = bookService;
            _authorStatisticsService = authorStatisticsService;
            _coverMapper = coverMapper;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetBook;
        }

        protected BookModuleWithSignalR(IBookService bookService,
                                           IAuthorStatisticsService authorStatisticsService,
                                           IMapCoversToLocal coverMapper,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _bookService = bookService;
            _authorStatisticsService = authorStatisticsService;
            _coverMapper = coverMapper;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetBook;
        }

        protected BookResource GetBook(int id)
        {
            var book = _bookService.GetBook(id);
            var resource = MapToResource(book, true);
            return resource;
        }

        protected BookResource MapToResource(Book book, bool includeAuthor)
        {
            var resource = book.ToResource();

            if (includeAuthor)
            {
                var artist = book.Author.Value;

                resource.Author = artist.ToResource();
            }

            FetchAndLinkAlbumStatistics(resource);
            MapCoversToLocal(resource);

            return resource;
        }

        protected List<BookResource> MapToResource(List<Book> books, bool includeAuthor)
        {
            var result = books.ToResource();

            if (includeAuthor)
            {
                var authorDict = new Dictionary<int, NzbDrone.Core.Books.Author>();
                for (var i = 0; i < books.Count; i++)
                {
                    var book = books[i];
                    var resource = result[i];
                    var author = authorDict.GetValueOrDefault(books[i].AuthorMetadataId) ?? book.Author?.Value;
                    authorDict[author.AuthorMetadataId] = author;

                    resource.Author = author.ToResource();
                }
            }

            var authorStats = _authorStatisticsService.AuthorStatistics();
            LinkAuthorStatistics(result, authorStats);
            MapCoversToLocal(result.ToArray());

            return result;
        }

        private void FetchAndLinkAlbumStatistics(BookResource resource)
        {
            LinkAuthorStatistics(resource, _authorStatisticsService.AuthorStatistics(resource.AuthorId));
        }

        private void LinkAuthorStatistics(List<BookResource> resources, List<AuthorStatistics> authorStatistics)
        {
            foreach (var book in resources)
            {
                var stats = authorStatistics.SingleOrDefault(ss => ss.AuthorId == book.AuthorId);
                LinkAuthorStatistics(book, stats);
            }
        }

        private void LinkAuthorStatistics(BookResource resource, AuthorStatistics authorStatistics)
        {
            if (authorStatistics?.BookStatistics != null)
            {
                var dictBookStats = authorStatistics.BookStatistics.ToDictionary(v => v.BookId);

                resource.Statistics = dictBookStats.GetValueOrDefault(resource.Id).ToResource();
            }
        }

        private void MapCoversToLocal(params BookResource[] books)
        {
            foreach (var bookResource in books)
            {
                _coverMapper.ConvertToLocalUrls(bookResource.Id, MediaCoverEntity.Book, bookResource.Images);
            }
        }
    }
}
