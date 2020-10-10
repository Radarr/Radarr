using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Readarr.Api.V1.Indexers
{
    public class ReleaseModule : ReleaseModuleBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        private readonly ICached<RemoteBook> _remoteBookCache;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IAuthorService authorService,
                             IBookService bookService,
                             IParsingService parsingService,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _authorService = authorService;
            _bookService = bookService;
            _parsingService = parsingService;
            _logger = logger;

            GetResourceAll = GetReleases;
            Post("/", x => DownloadRelease(ReadResourceFromRequest()));

            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteBookCache = cacheManager.GetCache<RemoteBook>(GetType(), "remoteBooks");
        }

        private object DownloadRelease(ReleaseResource release)
        {
            var remoteBook = _remoteBookCache.Find(GetCacheKey(release));

            if (remoteBook == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (remoteBook.Author == null)
                {
                    if (release.BookId.HasValue)
                    {
                        var book = _bookService.GetBook(release.BookId.Value);

                        remoteBook.Author = _authorService.GetAuthor(book.AuthorId);
                        remoteBook.Books = new List<Book> { book };
                    }
                    else if (release.AuthorId.HasValue)
                    {
                        var author = _authorService.GetAuthor(release.AuthorId.Value);
                        var books = _parsingService.GetAlbums(remoteBook.ParsedBookInfo, author);

                        if (books.Empty())
                        {
                            throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to parse books in the release");
                        }

                        remoteBook.Author = author;
                        remoteBook.Books = books;
                    }
                    else
                    {
                        throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to find matching author and books");
                    }
                }
                else if (remoteBook.Books.Empty())
                {
                    var books = _parsingService.GetAlbums(remoteBook.ParsedBookInfo, remoteBook.Author);

                    if (books.Empty() && release.BookId.HasValue)
                    {
                        var book = _bookService.GetBook(release.BookId.Value);

                        books = new List<Book> { book };
                    }

                    remoteBook.Books = books;
                }

                if (remoteBook.Books.Empty())
                {
                    throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to parse books in the release");
                }

                _downloadService.DownloadReport(remoteBook);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, "Getting release from indexer failed");
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        private List<ReleaseResource> GetReleases()
        {
            if (Request.Query.bookId.HasValue)
            {
                return GetBookReleases(Request.Query.bookId);
            }

            if (Request.Query.authorId.HasValue)
            {
                return GetAuthorReleases(Request.Query.authorId);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetBookReleases(int bookId)
        {
            try
            {
                var decisions = _nzbSearchService.BookSearch(bookId, true, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Book search failed");
                throw new NzbDroneClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private List<ReleaseResource> GetAuthorReleases(int authorId)
        {
            try
            {
                var decisions = _nzbSearchService.AuthorSearch(authorId, false, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Author search failed");
                throw new NzbDroneClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private List<ReleaseResource> GetRss()
        {
            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteBookCache.Set(GetCacheKey(resource), decision.RemoteBook, TimeSpan.FromMinutes(30));
            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
