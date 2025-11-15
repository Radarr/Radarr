using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.Filmweb
{
    public class FilmwebParser : IParseImportListResponse
    {
        private readonly IHttpClient _httpClient;
        private readonly int _limit;
        private ImportListResponse _importResponse;

        public FilmwebParser(IHttpClient httpClient, int limit)
        {
            _httpClient = httpClient;

            if (limit <= 0)
            {
                limit = 100;
            }

            if (limit > 1000)
            {
                limit = 1000;
            }

            _limit = limit;
        }

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var content = _importResponse.Content;

            try
            {
                var movieEntities = JsonSerializer.Deserialize<List<FilmwebMovieEntity>>(content);

                if (movieEntities != null)
                {
                    var limitedEntities = movieEntities.Take(_limit).ToList();

                    foreach (var entity in limitedEntities)
                    {
                        try
                        {
                            var movieInfo = GetMovieInfo(entity.Entity);
                            if (movieInfo != null)
                            {
                                var movie = new ImportListMovie()
                                {
                                    Title = !string.IsNullOrEmpty(movieInfo.Title) ? movieInfo.Title : movieInfo.OriginalTitle,
                                    Year = movieInfo.Year
                                };

                                movies.AddIfNotNull(movie);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }

            return movies;
        }

        private FilmwebMovieInfo GetMovieInfo(long entityId)
        {
            try
            {
                var request = new HttpRequestBuilder("https://www.filmweb.pl")
                    .Resource($"/api/v1/title/{entityId}/info")
                    .Accept(HttpAccept.Json)
                    .Build();

                var response = _httpClient.Get(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonSerializer.Deserialize<FilmwebMovieInfo>(response.Content);
                }
            }
            catch
            {
            }

            return null;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Filmweb call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            return true;
        }
    }

    public class FilmwebMovieEntity
    {
        [JsonPropertyName("entity")]
        public long Entity { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }
    }

    public class FilmwebMovieInfo
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("originalTitle")]
        public string OriginalTitle { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("subType")]
        public string SubType { get; set; }

        [JsonPropertyName("posterPath")]
        public string PosterPath { get; set; }
    }
}
