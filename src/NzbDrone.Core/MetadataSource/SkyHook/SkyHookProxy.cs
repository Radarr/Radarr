using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMetadataRequestBuilder _requestBuilder;
        private readonly ICached<HashSet<string>> _cache;

        public SkyHookProxy(IHttpClient httpClient,
                            IMetadataRequestBuilder requestBuilder,
                            IAuthorService authorService,
                            IBookService bookService,
                            Logger logger,
                            ICacheManager cacheManager)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder;
            _authorService = authorService;
            _bookService = bookService;
            _cache = cacheManager.GetCache<HashSet<string>>(GetType());
            _logger = logger;
        }

        public HashSet<string> GetChangedArtists(DateTime startTime)
        {
            return null;
        }

        public Author GetAuthorInfo(string foreignAuthorId)
        {
            _logger.Debug("Getting Author details ReadarrAPI.MetadataID of {0}", foreignAuthorId);

            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                .SetSegment("route", $"author/{foreignAuthorId}")
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<AuthorResource>(httpRequest);

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

            return MapAuthor(httpResponse.Resource);
        }

        public HashSet<string> GetChangedBooks(DateTime startTime)
        {
            return _cache.Get("ChangedAlbums", () => GetChangedAlbumsUncached(startTime), TimeSpan.FromMinutes(30));
        }

        private HashSet<string> GetChangedAlbumsUncached(DateTime startTime)
        {
            return null;
        }

        public Tuple<string, Book, List<AuthorMetadata>> GetBookInfo(string foreignBookId)
        {
            return null;
            /*
            _logger.Debug("Getting Book with ReadarrAPI.MetadataID of {0}", foreignBookId);

            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                                             .SetSegment("route", $"book/{foreignBookId}")
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<BookResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new BookNotFoundException(foreignBookId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignBookId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var b = httpResponse.Resource;
            var book = MapBook(b);

            // var authors = httpResponse.Resource.AuthorMetadata.SelectList(MapAuthor);
            var authorid = GetAuthorId(b).ToString();

            // book.AuthorMetadata = authors.First(x => x.ForeignAuthorId == authorid);
            return new Tuple<string, Book, List<AuthorMetadata>>(authorid, book, null);
            */
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

                var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                    .SetSegment("route", "search")
                    .AddQueryParam("q", q)
                    .Build();

                var result = _httpClient.Get<BookSearchResource>(httpRequest);

                return MapSearchResult(result.Resource);
            }
            catch (HttpException)
            {
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with ReadarrAPI.", title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from ReadarrAPI.", title);
            }
        }

        public List<Book> SearchByIsbn(string isbn)
        {
            return SearchByAlternateId("isbn", isbn);
        }

        public List<Book> SearchByAsin(string asin)
        {
            return SearchByAlternateId("asin", asin.ToUpper());
        }

        public List<Book> SearchByGoodreadsId(int goodreadsId)
        {
            return SearchByAlternateId("goodreads", goodreadsId.ToString());
        }

        private List<Book> SearchByAlternateId(string type, string id)
        {
            try
            {
                var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                    .SetSegment("route", $"book/{type}/{id}")
                    .Build();

                var httpResponse = _httpClient.Get<BookSearchResource>(httpRequest);

                var result = _httpClient.Get<BookSearchResource>(httpRequest);

                return MapSearchResult(result.Resource);
            }
            catch (HttpException)
            {
                throw new SkyHookException("Search for {0} '{1}' failed. Unable to communicate with ReadarrAPI.", type, id);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new SkyHookException("Search for {0 }'{1}' failed. Invalid response received from ReadarrAPI.", type, id);
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

        private Author MapAuthor(AuthorResource resource)
        {
            var metadata = MapAuthor(resource.AuthorMetadata.First(x => x.GoodreadsId == resource.GoodreadsId));

            var books = resource.Works
                .Where(x => GetAuthorId(x) == resource.GoodreadsId)
                .Select(MapBook)
                .ToList();

            books.ForEach(x => x.AuthorMetadata = metadata);

            var series = resource.Series.Select(MapSeries).ToList();

            MapSeriesLinks(series, books, resource);

            var result = new Author
            {
                Metadata = metadata,
                CleanName = Parser.Parser.CleanAuthorName(metadata.Name),
                SortName = Parser.Parser.NormalizeTitle(metadata.Name),
                Books = books,
                Series = series
            };

            return result;
        }

        private void MapSeriesLinks(List<Series> series, List<Book> books, BulkResource resource)
        {
            var bookDict = books.ToDictionary(x => x.ForeignBookId);
            var seriesDict = series.ToDictionary(x => x.ForeignSeriesId);

            // only take series where there are some works
            foreach (var s in resource.Series.Where(x => x.Works.Any()))
            {
                if (seriesDict.TryGetValue(s.GoodreadsId.ToString(), out var curr))
                {
                    curr.LinkItems = s.Works.Where(x => bookDict.ContainsKey(x.GoodreadsId.ToString())).Select(l => new SeriesBookLink
                    {
                        Book = bookDict[l.GoodreadsId.ToString()],
                        Series = curr,
                        IsPrimary = l.Primary,
                        Position = l.Position
                    }).ToList();
                }
            }
        }

        private static AuthorMetadata MapAuthor(AuthorSummaryResource resource)
        {
            var author = new AuthorMetadata
            {
                ForeignAuthorId = resource.GoodreadsId.ToString(),
                TitleSlug = resource.TitleSlug,
                Name = resource.Name.CleanSpaces(),
                Overview = resource.Description,
                Ratings = new Ratings { Votes = resource.RatingsCount, Value = (decimal)resource.AverageRating }
            };

            if (resource.ImageUrl.IsNotNullOrWhiteSpace())
            {
                author.Images.Add(new MediaCover.MediaCover
                {
                    Url = resource.ImageUrl,
                    CoverType = MediaCoverTypes.Poster
                });
            }

            author.Links.Add(new Links { Url = resource.Url, Name = "Goodreads" });

            return author;
        }

        private static Series MapSeries(SeriesResource resource)
        {
            var series = new Series
            {
                ForeignSeriesId = resource.GoodreadsId.ToString(),
                Title = resource.Title,
                Description = resource.Description
            };

            return series;
        }

        private static Book MapBook(WorkResource resource)
        {
            var book = new Book
            {
                ForeignBookId = resource.GoodreadsId.ToString(),
                Title = resource.Title,
                TitleSlug = resource.TitleSlug,
                CleanTitle = Parser.Parser.CleanAuthorName(resource.Title),
                ReleaseDate = resource.ReleaseDate,
            };

            book.Links.Add(new Links { Url = resource.Url, Name = "Goodreads Editions" });

            if (resource.Books != null)
            {
                book.Editions = resource.Books.Select(x => MapEdition(x)).ToList();

                // monitor the most rated release
                var mostPopular = book.Editions.Value.OrderByDescending(x => x.Ratings.Votes).FirstOrDefault();
                if (mostPopular != null)
                {
                    mostPopular.Monitored = true;

                    // fix work title if missing
                    if (book.Title.IsNullOrWhiteSpace())
                    {
                        book.Title = mostPopular.Title;
                    }
                }
            }
            else
            {
                book.Editions = new List<Edition>();
            }

            Debug.Assert(!book.Editions.Value.Any() || book.Editions.Value.Count(x => x.Monitored) == 1, "one edition monitored");

            book.AnyEditionOk = true;

            var ratingCount = book.Editions.Value.Sum(x => x.Ratings.Votes);

            if (ratingCount > 0)
            {
                book.Ratings = new Ratings
                {
                    Votes = ratingCount,
                    Value = book.Editions.Value.Sum(x => x.Ratings.Votes * x.Ratings.Value) / ratingCount
                };
            }
            else
            {
                book.Ratings = new Ratings { Votes = 0, Value = 0 };
            }

            return book;
        }

        private static Edition MapEdition(BookResource resource)
        {
            var edition = new Edition
            {
                ForeignEditionId = resource.GoodreadsId.ToString(),
                TitleSlug = resource.TitleSlug,
                Isbn13 = resource.Isbn13,
                Asin = resource.Asin,
                Title = resource.Title.CleanSpaces(),
                Language = resource.Language,
                Overview = resource.Description,
                Format = resource.Format,
                IsEbook = resource.IsEbook,
                Disambiguation = resource.EditionInformation,
                Publisher = resource.Publisher,
                PageCount = resource.NumPages ?? 0,
                ReleaseDate = resource.ReleaseDate,
                Ratings = new Ratings { Votes = resource.RatingCount, Value = (decimal)resource.AverageRating }
            };

            if (resource.ImageUrl.IsNotNullOrWhiteSpace())
            {
                edition.Images.Add(new MediaCover.MediaCover
                {
                    Url = resource.ImageUrl,
                    CoverType = MediaCoverTypes.Cover
                });
            }

            edition.Links.Add(new Links { Url = resource.Url, Name = "Goodreads Book" });

            return edition;
        }

        private List<Book> MapSearchResult(BookSearchResource resource)
        {
            var metadata = resource.AuthorMetadata.SelectList(MapAuthor).ToDictionary(x => x.ForeignAuthorId);

            var result = new List<Book>();

            foreach (var b in resource.Works)
            {
                var book = _bookService.FindById(b.GoodreadsId.ToString());
                if (book == null)
                {
                    book = MapBook(b);

                    var authorid = GetAuthorId(b);

                    if (authorid == 0)
                    {
                        continue;
                    }

                    var author = _authorService.FindById(authorid.ToString());

                    if (author == null)
                    {
                        var authorMetadata = metadata[authorid.ToString()];

                        author = new Author
                        {
                            CleanName = Parser.Parser.CleanAuthorName(authorMetadata.Name),
                            Metadata = authorMetadata
                        };
                    }

                    book.Author = author;
                    book.AuthorMetadata = author.Metadata.Value;
                }

                result.Add(book);
            }

            var seriesList = resource.Series.Select(MapSeries).ToList();

            MapSeriesLinks(seriesList, result, resource);

            return result;
        }

        private int GetAuthorId(WorkResource b)
        {
            return b.Books.First().Contributors.FirstOrDefault()?.GoodreadsId ?? 0;
        }
    }
}
