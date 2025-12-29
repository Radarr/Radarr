using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.MetadataSource.Book
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075", Justification = "API base URLs are necessarily hardcoded")]
    public class BookInfoProxy : IProvideBookInfo
    {
        private const string BaseUrl = "https://openlibrary.org";
        private const string SearchUrl = "https://openlibrary.org/search.json";
        private const string CoversUrl = "https://covers.openlibrary.org/b";
        private const string UserAgent = "Aletheia/1.0 (https://github.com/cheir-mneme/aletheia)";
        private const string SearchFields = "key,title,author_name,first_publish_year,isbn,cover_i,number_of_pages_median,subject,publisher,language";

        // API parameter names and JSON field names
        private const string ParamLimit = "limit";
        private const string ParamFields = "fields";
        private const string ParamTitle = "title";
        private const string JsonFieldTitle = "title";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public BookInfoProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public BookMetadata GetByExternalId(string externalId)
        {
            if (externalId.StartsWith("/works/", StringComparison.Ordinal))
            {
                return GetWorkById(externalId);
            }

            return GetWorkById($"/works/{externalId}");
        }

        public BookMetadata GetById(int providerId)
        {
            _logger.Debug("GetById called for: {0} (OpenLibrary uses string IDs)", providerId);
            return null;
        }

        public List<BookMetadata> GetBulkInfo(List<int> providerIds)
        {
            _logger.Debug("GetBulkInfo called for {0} IDs (not supported by OpenLibrary)", providerIds.Count);
            return new List<BookMetadata>();
        }

        public List<BookMetadata> GetTrending()
        {
            try
            {
                var request = BuildRequestBuilder($"{BaseUrl}/trending/daily.json")
                    .AddQueryParam(ParamLimit, "20")
                    .Build();
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<BookMetadata>();
                }

                var works = response["works"] as JArray;
                if (works == null)
                {
                    return new List<BookMetadata>();
                }

                return works.Select(ParseTrendingWork).Where(b => b != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching trending books from OpenLibrary");
                return new List<BookMetadata>();
            }
        }

        public List<BookMetadata> GetPopular()
        {
            return GetTrending();
        }

        public HashSet<int> GetChangedItems(DateTime startTime)
        {
            _logger.Debug("GetChangedItems not supported by OpenLibrary");
            return new HashSet<int>();
        }

        public List<BookMetadata> SearchByTitle(string title)
        {
            try
            {
                var request = BuildRequestBuilder(SearchUrl)
                    .AddQueryParam(ParamTitle, title)
                    .AddQueryParam(ParamLimit, "25")
                    .AddQueryParam(ParamFields, SearchFields)
                    .Build();

                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<BookMetadata>();
                }

                var docs = response["docs"] as JArray;
                if (docs == null)
                {
                    return new List<BookMetadata>();
                }

                return docs.Select(ParseSearchResult).Where(b => b != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching books for '{0}'", title);
                return new List<BookMetadata>();
            }
        }

        public List<BookMetadata> SearchByTitle(string title, int year)
        {
            try
            {
                var request = BuildRequestBuilder(SearchUrl)
                    .AddQueryParam(ParamTitle, title)
                    .AddQueryParam("first_publish_year", year.ToString(CultureInfo.InvariantCulture))
                    .AddQueryParam(ParamLimit, "25")
                    .AddQueryParam(ParamFields, SearchFields)
                    .Build();

                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<BookMetadata>();
                }

                var docs = response["docs"] as JArray;
                if (docs == null)
                {
                    return new List<BookMetadata>();
                }

                return docs.Select(ParseSearchResult).Where(b => b != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching books for '{0}' ({1})", title, year);
                return new List<BookMetadata>();
            }
        }

        public BookMetadata GetByIsbn(string isbn)
        {
            return GetByIsbnInternal(isbn);
        }

        public BookMetadata GetByIsbn13(string isbn13)
        {
            return GetByIsbnInternal(isbn13);
        }

        private BookMetadata GetByIsbnInternal(string isbn)
        {
            try
            {
                var request = BuildRequestBuilder($"{BaseUrl}/isbn/{isbn}.json").Build();
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                return ParseEdition(response, isbn);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching book by ISBN {0}", isbn);
                return null;
            }
        }

        public BookMetadata GetByAsin(string asin)
        {
            try
            {
                var request = BuildRequestBuilder(SearchUrl)
                    .AddQueryParam("q", $"asin:{asin}")
                    .AddQueryParam(ParamLimit, "1")
                    .AddQueryParam(ParamFields, SearchFields)
                    .Build();

                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                var docs = response["docs"] as JArray;
                if (docs == null || docs.Count == 0)
                {
                    return null;
                }

                return ParseSearchResult(docs[0]);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching book by ASIN {0}", asin);
                return null;
            }
        }

        public List<BookMetadata> GetByAuthor(string authorName)
        {
            try
            {
                var request = BuildRequestBuilder(SearchUrl)
                    .AddQueryParam("author", authorName)
                    .AddQueryParam(ParamLimit, "50")
                    .AddQueryParam(ParamFields, SearchFields)
                    .Build();

                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<BookMetadata>();
                }

                var docs = response["docs"] as JArray;
                if (docs == null)
                {
                    return new List<BookMetadata>();
                }

                return docs.Select(ParseSearchResult).Where(b => b != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching books by author '{0}'", authorName);
                return new List<BookMetadata>();
            }
        }

        private BookMetadata GetWorkById(string workId)
        {
            try
            {
                var request = BuildRequestBuilder($"{BaseUrl}{workId}.json").Build();
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                return ParseWork(response, workId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching work {0}", workId);
                return null;
            }
        }

        private static HttpRequestBuilder BuildRequestBuilder(string url)
        {
            return new HttpRequestBuilder(url)
                .SetHeader("User-Agent", UserAgent)
                .SetHeader("Accept", "application/json");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1168", Justification = "Null indicates not found, distinct from empty response")]
        private JObject ExecuteRequest(HttpRequest request)
        {
            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.HasHttpError)
            {
                return JObject.Parse(response.Content);
            }

            _logger.Warn("OpenLibrary request failed: {0}", response.StatusCode);
            return null;
        }

        private static BookMetadata ParseSearchResult(JToken json)
        {
            var key = json["key"]?.ToString();
            var coverId = json["cover_i"]?.Value<int?>();

            var book = new BookMetadata
            {
                ForeignBookId = key,
                Title = json[JsonFieldTitle]?.ToString(),
                Authors = new List<string>(),
                Genres = new List<string>()
            };

            var authorNames = json["author_name"] as JArray;
            if (authorNames != null)
            {
                book.Authors = authorNames.Select(a => a.ToString()).ToList();
            }

            var year = json["first_publish_year"]?.Value<int?>();
            if (year.HasValue)
            {
                book.ReleaseDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            ParseIsbns(json, book);
            book.PageCount = json["number_of_pages_median"]?.Value<int?>();
            ParseSubjects(json, book);
            ParsePublisher(json, book);
            ParseLanguage(json, book);

            if (coverId.HasValue)
            {
                book.CoverUrl = $"{CoversUrl}/id/{coverId}-L.jpg";
            }

            return book;
        }

        private static void ParseIsbns(JToken json, BookMetadata book)
        {
            var isbns = json["isbn"] as JArray;
            if (isbns != null && isbns.Count > 0)
            {
                var isbnList = isbns.Select(i => i.ToString()).ToList();
                book.Isbn13 = isbnList.FirstOrDefault(i => i.Length == 13);
                book.Isbn = isbnList.FirstOrDefault(i => i.Length == 10) ?? book.Isbn13;
            }
        }

        private static void ParseSubjects(JToken json, BookMetadata book)
        {
            var subjects = json["subject"] as JArray ?? json["subjects"] as JArray;
            if (subjects != null)
            {
                book.Genres = subjects.Take(10).Select(s => s.ToString()).ToList();
            }
        }

        private static void ParsePublisher(JToken json, BookMetadata book)
        {
            var publishers = json["publisher"] as JArray ?? json["publishers"] as JArray;
            if (publishers != null && publishers.Count > 0)
            {
                book.Publisher = publishers[0].ToString();
            }
        }

        private static void ParseLanguage(JToken json, BookMetadata book)
        {
            var languages = json["language"] as JArray;
            if (languages != null && languages.Count > 0)
            {
                book.Language = languages[0].ToString();
            }
        }

        private BookMetadata ParseWork(JObject json, string workId)
        {
            var book = new BookMetadata
            {
                ForeignBookId = workId,
                Title = json[JsonFieldTitle]?.ToString(),
                Authors = new List<string>(),
                Genres = new List<string>()
            };

            ParseDescription(json, book);
            ParseSubjects(json, book);
            ParseCoverFromCovers(json, book);
            ParseAuthorsFromWork(json, book);

            return book;
        }

        private static void ParseDescription(JObject json, BookMetadata book)
        {
            var description = json["description"];
            if (description != null)
            {
                book.Description = description.Type == JTokenType.String
                    ? description.ToString()
                    : description["value"]?.ToString();
            }
        }

        private static void ParseCoverFromCovers(JObject json, BookMetadata book)
        {
            var covers = json["covers"] as JArray;
            if (covers != null && covers.Count > 0)
            {
                var coverId = covers[0].Value<int>();
                book.CoverUrl = $"{CoversUrl}/id/{coverId}-L.jpg";
            }
        }

        private void ParseAuthorsFromWork(JObject json, BookMetadata book)
        {
            var authors = json["authors"] as JArray;
            if (authors == null)
            {
                return;
            }

            foreach (var authorRef in authors)
            {
                var authorKey = authorRef["author"]?["key"]?.ToString();
                if (!string.IsNullOrEmpty(authorKey))
                {
                    var authorName = GetAuthorName(authorKey);
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        book.Authors.Add(authorName);
                    }
                }
            }
        }

        private BookMetadata ParseEdition(JObject json, string isbn)
        {
            var book = new BookMetadata
            {
                Title = json[JsonFieldTitle]?.ToString(),
                Authors = new List<string>(),
                Genres = new List<string>()
            };

            SetIsbnFromInput(isbn, book);
            ParseIsbnArrays(json, book);
            book.PageCount = json["number_of_pages"]?.Value<int?>();
            ParsePublisher(json, book);
            ParsePublishDate(json, book);
            ParseCoverFromCovers(json, book);
            EnrichFromWork(json, book);
            ParseAuthorsFromEdition(json, book);

            return book;
        }

        private static void SetIsbnFromInput(string isbn, BookMetadata book)
        {
            if (isbn.Length == 13)
            {
                book.Isbn13 = isbn;
            }
            else
            {
                book.Isbn = isbn;
            }
        }

        private static void ParseIsbnArrays(JObject json, BookMetadata book)
        {
            var isbn13Array = json["isbn_13"] as JArray;
            if (isbn13Array != null && isbn13Array.Count > 0)
            {
                book.Isbn13 = isbn13Array[0].ToString();
            }

            var isbn10Array = json["isbn_10"] as JArray;
            if (isbn10Array != null && isbn10Array.Count > 0)
            {
                book.Isbn = isbn10Array[0].ToString();
            }
        }

        private static void ParsePublishDate(JObject json, BookMetadata book)
        {
            var publishDate = json["publish_date"]?.ToString();
            if (string.IsNullOrEmpty(publishDate))
            {
                return;
            }

            if (DateTime.TryParse(publishDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                book.ReleaseDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
            else if (int.TryParse(publishDate, out var year))
            {
                book.ReleaseDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        private void EnrichFromWork(JObject json, BookMetadata book)
        {
            var workKey = (json["works"] as JArray)?[0]?["key"]?.ToString();
            if (string.IsNullOrEmpty(workKey))
            {
                return;
            }

            book.ForeignBookId = workKey;

            var workInfo = GetWorkById(workKey);
            if (workInfo != null)
            {
                book.Description = workInfo.Description;
                book.Authors = workInfo.Authors;
                book.Genres = workInfo.Genres;
            }
        }

        private void ParseAuthorsFromEdition(JObject json, BookMetadata book)
        {
            if (book.Authors.Count > 0)
            {
                return;
            }

            var authors = json["authors"] as JArray;
            if (authors == null)
            {
                return;
            }

            foreach (var authorRef in authors)
            {
                var authorKey = authorRef["key"]?.ToString();
                if (!string.IsNullOrEmpty(authorKey))
                {
                    var authorName = GetAuthorName(authorKey);
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        book.Authors.Add(authorName);
                    }
                }
            }
        }

        private static BookMetadata ParseTrendingWork(JToken json)
        {
            var key = json["key"]?.ToString();
            var coverId = json["cover_i"]?.Value<int?>();

            var book = new BookMetadata
            {
                ForeignBookId = key,
                Title = json[JsonFieldTitle]?.ToString(),
                Authors = new List<string>(),
                Genres = new List<string>()
            };

            var authorName = json["author_name"]?.ToString();
            if (!string.IsNullOrEmpty(authorName))
            {
                book.Authors.Add(authorName);
            }

            var year = json["first_publish_year"]?.Value<int?>();
            if (year.HasValue)
            {
                book.ReleaseDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            if (coverId.HasValue)
            {
                book.CoverUrl = $"{CoversUrl}/id/{coverId}-L.jpg";
            }

            return book;
        }

        private string GetAuthorName(string authorKey)
        {
            try
            {
                var request = BuildRequestBuilder($"{BaseUrl}{authorKey}.json").Build();
                var response = ExecuteRequest(request);

                return response?["name"]?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
