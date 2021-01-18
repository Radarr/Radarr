using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Http;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    public class GoodreadsProxy : IProvideAuthorInfo, ISearchForNewAuthor, IProvideBookInfo, ISearchForNewBook, ISearchForNewEntity
    {
        private static readonly RegexReplace FullSizeImageRegex = new RegexReplace(@"\._[SU][XY]\d+_.jpg$",
                                                                                   ".jpg",
                                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex NoPhotoRegex = new Regex(@"/nophoto/(book|user)/",
                                                               RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IHttpClient _httpClient;
        private readonly ICachedHttpResponseService _cachedHttpClient;
        private readonly Logger _logger;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IEditionService _editionService;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IHttpRequestBuilderFactory _searchBuilder;
        private readonly ICached<HashSet<string>> _cache;

        public GoodreadsProxy(IHttpClient httpClient,
                              ICachedHttpResponseService cachedHttpClient,
                              IAuthorService authorService,
                              IBookService bookService,
                              IEditionService editionService,
                              Logger logger,
                              ICacheManager cacheManager)
        {
            _httpClient = httpClient;
            _cachedHttpClient = cachedHttpClient;
            _authorService = authorService;
            _bookService = bookService;
            _editionService = editionService;
            _cache = cacheManager.GetCache<HashSet<string>>(GetType());
            _logger = logger;

            _requestBuilder = new HttpRequestBuilder("https://www.goodreads.com/{route}")
                .AddQueryParam("key", new string("gSuM2Onzl6sjMU25HY1Xcd".Reverse().ToArray()))
                .AddQueryParam("_nc", "1")
                .SetHeader("User-Agent", "Dalvik/1.6.0 (Linux; U; Android 4.1.2; GT-I9100 Build/JZO54K)")
                .KeepAlive()
                .CreateFactory();

            _searchBuilder = new HttpRequestBuilder("https://www.goodreads.com/book/auto_complete")
                .AddQueryParam("format", "json")
                .SetHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
                .KeepAlive()
                .CreateFactory();
        }

        public HashSet<string> GetChangedArtists(DateTime startTime)
        {
            return null;
        }

        public Author GetAuthorInfo(string foreignAuthorId, bool useCache = true)
        {
            _logger.Debug("Getting Author details GoodreadsId of {0}", foreignAuthorId);

            var httpRequest = _requestBuilder.Create()
                .SetSegment("route", $"author/show/{foreignAuthorId}.xml")
                .AddQueryParam("exclude_books", "true")
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _cachedHttpClient.Get(httpRequest, useCache, TimeSpan.FromDays(30));

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AuthorNotFoundException(foreignAuthorId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignAuthorId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var resource = httpResponse.Deserialize<AuthorResource>();
            var author = new Author
            {
                Metadata = MapAuthor(resource)
            };
            author.CleanName = Parser.Parser.CleanAuthorName(author.Metadata.Value.Name);
            author.SortName = Parser.Parser.NormalizeTitle(author.Metadata.Value.Name);

            // we can only get a rating from the author list page...
            var listResource = GetAuthorBooksPageResource(foreignAuthorId, 10, 1);
            var authorResource = listResource.List.SelectMany(x => x.Authors).FirstOrDefault(a => a.Id.ToString() == foreignAuthorId);
            author.Metadata.Value.Ratings = new Ratings
            {
                Votes = authorResource?.RatingsCount ?? 0,
                Value = authorResource?.AverageRating ?? 0
            };

            return author;
        }

        public Author GetAuthorAndBooks(string foreignAuthorId, double minPopularity = 0)
        {
            var author = GetAuthorInfo(foreignAuthorId);

            var bookList = GetAuthorBooks(foreignAuthorId, minPopularity);
            var books = bookList.Select((x, i) =>
            {
                _logger.ProgressDebug($"{author}: Fetching book {i}/{bookList.Count}");
                return GetBookInfo(x.Editions.Value.First().ForeignEditionId).Item2;
            }).ToList();

            var existingAuthor = _authorService.FindById(foreignAuthorId);
            if (existingAuthor != null)
            {
                var existingEditions = _editionService.GetEditionsByAuthor(existingAuthor.Id);
                var extraEditionIds = existingEditions.Select(x => x.ForeignEditionId).Except(books.Select(x => x.Editions.Value.First().ForeignEditionId));

                _logger.Debug($"Getting data for extra editions {extraEditionIds.ConcatToString()}");
                var extraEditions = extraEditionIds.Select(x => GetBookInfo(x));

                var bookDict = books.ToDictionary(x => x.ForeignBookId);
                foreach (var edition in extraEditions)
                {
                    var b = edition.Item2;

                    if (bookDict.TryGetValue(b.ForeignBookId, out var book))
                    {
                        book.Editions.Value.Add(b.Editions.Value.First());
                    }
                    else
                    {
                        bookDict.Add(b.ForeignBookId, b);
                    }
                }

                books = bookDict.Values.ToList();
            }

            books.ForEach(x => x.AuthorMetadata = author.Metadata.Value);
            author.Books = books;

            author.Series = GetAuthorSeries(foreignAuthorId, author.Books);

            return author;
        }

        private List<Book> GetAuthorBooks(string foreignAuthorId, double minPopularity)
        {
            var perPage = 100;
            var page = 0;

            var result = new List<Book>();
            List<Book> current;
            IEnumerable<Book> filtered;

            do
            {
                current = GetAuthorBooksPage(foreignAuthorId, perPage, ++page);
                filtered = current.Where(x => x.Editions.Value.First().Ratings.Popularity >= minPopularity);
                result.AddRange(filtered);
            }
            while (current.Count == perPage && filtered.Any());

            return result;
        }

        private List<Book> GetAuthorBooksPage(string foreignAuthorId, int perPage, int page)
        {
            var resource = GetAuthorBooksPageResource(foreignAuthorId, perPage, page);

            var books = resource?.List.Where(x => x.Authors.First().Id.ToString() == foreignAuthorId)
                .Select(MapBook)
                .ToList() ??
                new List<Book>();

            books.ForEach(x => x.CleanTitle = x.Title.CleanAuthorName());

            return books;
        }

        private AuthorBookListResource GetAuthorBooksPageResource(string foreignAuthorId, int perPage, int page)
        {
            _logger.Debug("Getting Author Books with GoodreadsId of {0}", foreignAuthorId);

            var httpRequest = _requestBuilder.Create()
                .SetSegment("route", $"author/list/{foreignAuthorId}.xml")
                .AddQueryParam("per_page", perPage)
                .AddQueryParam("page", page)
                .AddQueryParam("sort", "popularity")
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _cachedHttpClient.Get(httpRequest, true, TimeSpan.FromDays(7));

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AuthorNotFoundException(foreignAuthorId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignAuthorId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            return httpResponse.Deserialize<AuthorBookListResource>();
        }

        private List<Series> GetAuthorSeries(string foreignAuthorId, List<Book> books)
        {
            _logger.Debug("Getting Author Series with GoodreadsId of {0}", foreignAuthorId);

            var httpRequest = _requestBuilder.Create()
                .SetSegment("route", $"series/list/{foreignAuthorId}.xml")
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _cachedHttpClient.Get(httpRequest, true, TimeSpan.FromDays(90));

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AuthorNotFoundException(foreignAuthorId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignAuthorId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var resource = httpResponse.Deserialize<AuthorSeriesListResource>();

            var result = new List<Series>();
            var bookDict = books.ToDictionary(x => x.ForeignBookId);

            // only take series where there are some works
            // and the title is not null
            // e.g. https://www.goodreads.com/series/work/6470221?format=xml is in series 260494
            // which has a null title and is not shown anywhere on goodreads webpage
            foreach (var seriesResource in resource.List.Where(x => x.Title.IsNotNullOrWhiteSpace() && x.Works.Any()))
            {
                var series = MapSeries(seriesResource);
                series.LinkItems = new List<SeriesBookLink>();

                var works = seriesResource.Works
                    .Where(x => x.BestBook.AuthorId.ToString() == foreignAuthorId &&
                           bookDict.ContainsKey(x.Id.ToString()));
                foreach (var work in works)
                {
                    series.LinkItems.Value.Add(new SeriesBookLink
                    {
                        Book = bookDict[work.Id.ToString()],
                        Series = series,
                        IsPrimary = true,
                        Position = work.UserPosition
                    });
                }

                if (series.LinkItems.Value.Any())
                {
                    result.Add(series);
                }
            }

            return result;
        }

        public HashSet<string> GetChangedBooks(DateTime startTime)
        {
            return _cache.Get("ChangedBooks", () => GetChangedBooksUncached(startTime), TimeSpan.FromMinutes(30));
        }

        private HashSet<string> GetChangedBooksUncached(DateTime startTime)
        {
            return null;
        }

        public Tuple<string, Book, List<AuthorMetadata>> GetBookInfo(string foreignEditionId, bool useCache = true)
        {
            _logger.Debug("Getting Book with GoodreadsId of {0}", foreignEditionId);

            var httpRequest = _requestBuilder.Create()
                .SetSegment("route", $"api/book/basic_book_data/{foreignEditionId}")
                .AddQueryParam("format", "xml")
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _cachedHttpClient.Get(httpRequest, useCache, TimeSpan.FromDays(90));

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new BookNotFoundException(foreignEditionId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignEditionId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var resource = httpResponse.Deserialize<BookResource>();

            var book = MapBook(resource);
            book.CleanTitle = Parser.Parser.CleanAuthorName(book.Title);

            var authors = resource.Authors.SelectList(MapAuthor);
            book.AuthorMetadata = authors.First();

            return new Tuple<string, Book, List<AuthorMetadata>>(resource.Authors.First().Id.ToString(), book, authors);
        }

        public List<Author> SearchForNewAuthor(string title)
        {
            var books = SearchForNewBook(title, null);

            return books.Select(x => x.Author.Value).ToList();
        }

        public List<Book> SearchForNewBook(string title, string author)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                var split = lowerTitle.Split(':');
                var prefix = split[0];

                if (split.Length == 2 && new[] { "readarr", "readarrid", "goodreads", "isbn", "asin" }.Contains(prefix))
                {
                    var slug = split[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                    {
                        return new List<Book>();
                    }

                    if (prefix == "goodreads" || prefix == "readarr" || prefix == "readarrid")
                    {
                        var isValid = int.TryParse(slug, out var searchId);
                        if (!isValid)
                        {
                            return new List<Book>();
                        }

                        return SearchByGoodreadsId(searchId);
                    }
                    else if (prefix == "isbn")
                    {
                        return SearchByIsbn(slug);
                    }
                    else if (prefix == "asin")
                    {
                        return SearchByAsin(slug);
                    }
                }

                var q = title.ToLower().Trim();
                if (author != null)
                {
                    q += " " + author;
                }

                return SearchByField("all", q);
            }
            catch (HttpException)
            {
                throw new GoodreadsException("Search for '{0}' failed. Unable to communicate with Goodreads.", title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new GoodreadsException("Search for '{0}' failed. Invalid response received from Goodreads.", title);
            }
        }

        public List<Book> SearchByIsbn(string isbn)
        {
            return SearchByField("isbn", isbn);
        }

        public List<Book> SearchByAsin(string asin)
        {
            return SearchByField("isbn", asin);
        }

        public List<Book> SearchByGoodreadsId(int id)
        {
            try
            {
                var remote = GetBookInfo(id.ToString());

                var book = _bookService.FindById(remote.Item2.ForeignBookId);
                var result = book ?? remote.Item2;

                var edition = _editionService.GetEditionByForeignEditionId(remote.Item2.Editions.Value.Single(x => x.Monitored).ForeignEditionId);
                if (edition != null)
                {
                    result.Editions = new List<Edition> { edition };
                }

                var author = _authorService.FindById(remote.Item1);
                if (author == null)
                {
                    author = new Author
                    {
                        CleanName = Parser.Parser.CleanAuthorName(remote.Item2.AuthorMetadata.Value.Name),
                        Metadata = remote.Item2.AuthorMetadata.Value
                    };
                }

                result.Author = author;

                return new List<Book> { result };
            }
            catch (BookNotFoundException)
            {
                return new List<Book>();
            }
        }

        public List<Book> SearchByField(string field, string query)
        {
            try
            {
                var httpRequest = _searchBuilder.Create()
                    .AddQueryParam("q", query)
                    .Build();

                var result = _httpClient.Get<List<SearchJsonResource>>(httpRequest);

                return result.Resource.SelectList(MapJsonSearchResult);
            }
            catch (HttpException)
            {
                throw new GoodreadsException("Search for {0} '{1}' failed. Unable to communicate with Goodreads.", field, query);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new GoodreadsException("Search for {0} '{1}' failed. Invalid response received from Goodreads.", field, query);
            }
        }

        public List<object> SearchForNewEntity(string title)
        {
            var books = SearchForNewBook(title, null);

            var result = new List<object>();
            foreach (var book in books)
            {
                var author = book.Author.Value;

                if (!result.Contains(author))
                {
                    result.Add(author);
                }

                result.Add(book);
            }

            return result;
        }

        private static AuthorMetadata MapAuthor(AuthorResource resource)
        {
            var author = new AuthorMetadata
            {
                ForeignAuthorId = resource.Id.ToString(),
                TitleSlug = resource.Id.ToString(),
                Name = resource.Name.CleanSpaces(),
                Overview = resource.About,
                Gender = resource.Gender,
                Hometown = resource.Hometown,
                Born = resource.BornOnDate,
                Died = resource.DiedOnDate,
                Status = resource.DiedOnDate < DateTime.UtcNow ? AuthorStatusType.Ended : AuthorStatusType.Continuing
            };

            if (!NoPhotoRegex.IsMatch(resource.LargeImageUrl))
            {
                author.Images.Add(new MediaCover.MediaCover
                {
                    Url = FullSizeImageRegex.Replace(resource.LargeImageUrl),
                    CoverType = MediaCoverTypes.Poster
                });
            }

            author.Links.Add(new Links { Url = resource.Link, Name = "Goodreads" });

            return author;
        }

        private static AuthorMetadata MapAuthor(AuthorSummaryResource resource)
        {
            var author = new AuthorMetadata
            {
                ForeignAuthorId = resource.Id.ToString(),
                Name = resource.Name.CleanSpaces(),
                TitleSlug = resource.Id.ToString()
            };

            if (resource.RatingsCount.HasValue)
            {
                author.Ratings = new Ratings
                {
                    Votes = resource.RatingsCount ?? 0,
                    Value = resource.AverageRating ?? 0
                };
            }

            if (!NoPhotoRegex.IsMatch(resource.ImageUrl))
            {
                author.Images.Add(new MediaCover.MediaCover
                {
                    Url = FullSizeImageRegex.Replace(resource.ImageUrl),
                    CoverType = MediaCoverTypes.Poster
                });
            }

            return author;
        }

        private static Series MapSeries(SeriesResource resource)
        {
            var series = new Series
            {
                ForeignSeriesId = resource.Id.ToString(),
                Title = resource.Title,
                Description = resource.Description,
                Numbered = resource.IsNumbered,
                WorkCount = resource.SeriesWorksCount,
                PrimaryWorkCount = resource.PrimaryWorksCount
            };

            return series;
        }

        private static Book MapBook(BookResource resource)
        {
            var book = new Book
            {
                ForeignBookId = resource.Work.Id.ToString(),
                Title = (resource.Work.OriginalTitle ?? resource.TitleWithoutSeries).CleanSpaces(),
                TitleSlug = resource.Id.ToString(),
                ReleaseDate = resource.Work.OriginalPublicationDate ?? resource.PublicationDate,
                Ratings = new Ratings { Votes = resource.Work.RatingsCount, Value = resource.Work.AverageRating },
                AnyEditionOk = true
            };

            if (resource.EditionsUrl != null)
            {
                book.Links.Add(new Links { Url = resource.EditionsUrl, Name = "Goodreads Editions" });
            }

            var edition = new Edition
            {
                ForeignEditionId = resource.Id.ToString(),
                TitleSlug = resource.Id.ToString(),
                Isbn13 = resource.Isbn13,
                Asin = resource.Asin ?? resource.KindleAsin,
                Title = resource.TitleWithoutSeries,
                Language = resource.LanguageCode,
                Overview = resource.Description,
                Format = resource.Format,
                IsEbook = resource.IsEbook,
                Disambiguation = resource.EditionInformation,
                Publisher = resource.Publisher,
                PageCount = resource.Pages,
                ReleaseDate = resource.PublicationDate,
                Ratings = new Ratings { Votes = resource.RatingsCount, Value = resource.AverageRating },
                Monitored = true
            };

            if (resource.ImageUrl.IsNotNullOrWhiteSpace() && !NoPhotoRegex.IsMatch(resource.ImageUrl))
            {
                edition.Images.Add(new MediaCover.MediaCover
                {
                    Url = FullSizeImageRegex.Replace(resource.ImageUrl),
                    CoverType = MediaCoverTypes.Cover
                });
            }

            edition.Links.Add(new Links { Url = resource.Url, Name = "Goodreads Book" });

            book.Editions = new List<Edition> { edition };

            Debug.Assert(!book.Editions.Value.Any() || book.Editions.Value.Count(x => x.Monitored) == 1, "one edition monitored");

            return book;
        }

        private Book MapSearchResult(WorkResource resource)
        {
            var book = _bookService.FindById(resource.Id.ToString());
            if (resource.BestBook != null)
            {
                var edition = _editionService.GetEditionByForeignEditionId(resource.BestBook.Id.ToString());

                if (edition == null)
                {
                    edition = new Edition
                    {
                        ForeignEditionId = resource.BestBook.Id.ToString(),
                        Title = resource.BestBook.Title,
                        TitleSlug = resource.BestBook.Id.ToString(),
                        Ratings = new Ratings { Votes = resource.RatingsCount, Value = resource.AverageRating },
                    };
                }

                edition.Monitored = true;
                edition.ManualAdd = true;

                if (resource.BestBook.ImageUrl.IsNotNullOrWhiteSpace() && !NoPhotoRegex.IsMatch(resource.BestBook.ImageUrl))
                {
                    edition.Images.Add(new MediaCover.MediaCover
                    {
                        Url = FullSizeImageRegex.Replace(resource.BestBook.ImageUrl),
                        CoverType = MediaCoverTypes.Cover
                    });
                }

                if (book == null)
                {
                    book = new Book
                    {
                        ForeignBookId = resource.Id.ToString(),
                        Title = resource.BestBook.Title,
                        TitleSlug = resource.Id.ToString(),
                        ReleaseDate = resource.OriginalPublicationDate,
                        Ratings = new Ratings { Votes = resource.RatingsCount, Value = resource.AverageRating },
                        AnyEditionOk = true
                    };
                }

                book.Editions = new List<Edition> { edition };

                var authorId = resource.BestBook.AuthorId.ToString();
                var author = _authorService.FindById(authorId);

                if (author == null)
                {
                    author = new Author
                    {
                        CleanName = Parser.Parser.CleanAuthorName(resource.BestBook.AuthorName),
                        Metadata = new AuthorMetadata()
                        {
                            ForeignAuthorId = resource.BestBook.AuthorId.ToString(),
                            Name = resource.BestBook.AuthorName,
                            TitleSlug = resource.BestBook.AuthorId.ToString()
                        }
                    };
                }

                book.Author = author;
                book.AuthorMetadata = book.Author.Value.Metadata.Value;
                book.CleanTitle = book.Title.CleanAuthorName();
            }

            return book;
        }

        private Book MapJsonSearchResult(SearchJsonResource resource)
        {
            var book = _bookService.FindById(resource.WorkId.ToString());
            var edition = _editionService.GetEditionByForeignEditionId(resource.BookId.ToString());

            if (edition == null)
            {
                edition = new Edition
                {
                    ForeignEditionId = resource.BookId.ToString(),
                    Title = resource.BookTitleBare,
                    TitleSlug = resource.BookId.ToString(),
                    Ratings = new Ratings { Votes = resource.RatingsCount, Value = resource.AverageRating },
                    PageCount = resource.PageCount,
                    Overview = resource.Description?.Html ?? string.Empty
                };
            }

            edition.Monitored = true;
            edition.ManualAdd = true;

            if (resource.ImageUrl.IsNotNullOrWhiteSpace() && !NoPhotoRegex.IsMatch(resource.ImageUrl))
            {
                edition.Images.Add(new MediaCover.MediaCover
                {
                    Url = FullSizeImageRegex.Replace(resource.ImageUrl),
                    CoverType = MediaCoverTypes.Cover
                });
            }

            if (book == null)
            {
                book = new Book
                {
                    ForeignBookId = resource.WorkId.ToString(),
                    Title = resource.BookTitleBare,
                    TitleSlug = resource.WorkId.ToString(),
                    Ratings = new Ratings { Votes = resource.RatingsCount, Value = resource.AverageRating },
                    AnyEditionOk = true
                };
            }

            book.Editions = new List<Edition> { edition };

            var authorId = resource.Author.Id.ToString();
            var author = _authorService.FindById(authorId);

            if (author == null)
            {
                author = new Author
                {
                    CleanName = Parser.Parser.CleanAuthorName(resource.Author.Name),
                    Metadata = new AuthorMetadata()
                    {
                        ForeignAuthorId = resource.Author.Id.ToString(),
                        Name = DuplicateSpacesRegex.Replace(resource.Author.Name, " "),
                        TitleSlug = resource.Author.Id.ToString()
                    }
                };
            }

            book.Author = author;
            book.AuthorMetadata = book.Author.Value.Metadata.Value;
            book.CleanTitle = book.Title.CleanAuthorName();

            return book;
        }
    }
}
