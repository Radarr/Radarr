using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using NzbDrone.Core.Datastore;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class MovieFilterResource
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public JsonElement Value { get; set; }
    }

    public class MoviePagingResource : PagingResource<MovieResource>
    {
        public List<int> MovieIds { get; set; } = new();
        public MovieFacetResource Facets { get; set; }
        public Dictionary<string, MovieJumpResource> JumpBar { get; set; } = new();
    }

    public class MovieJumpResource
    {
        public int Count { get; set; }
        public int Page { get; set; }
    }

    public class MovieFacetResource
    {
        public List<string> Certifications { get; set; } = new();
        public List<string> Collections { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public List<string> OriginalLanguages { get; set; } = new();
        public List<string> ReleaseGroups { get; set; } = new();
        public List<string> Studios { get; set; } = new();
        public List<int> QualityProfileIds { get; set; } = new();
        public List<int> TmdbIds { get; set; } = new();
        public int TotalRecords { get; set; }
    }

    public class MovieLinkResource
    {
        public string Title { get; set; }
        public string TitleSlug { get; set; }
    }

    public class MovieDetailsResource
    {
        public MovieResource Movie { get; set; }
        public MovieLinkResource PreviousMovie { get; set; }
        public MovieLinkResource NextMovie { get; set; }
    }

    public static class MovieQueryService
    {
        public const int PageSize = 100;

        public static MoviePagingResource Page(IEnumerable<MovieResource> source, PagingRequestResource request, string serializedFilters)
        {
            var page = Math.Max(request.Page ?? 1, 1);
            var pageSize = Math.Clamp(request.PageSize ?? PageSize, 1, PageSize);
            var sortKey = request.SortKey ?? "sortTitle";
            var sortDirection = request.SortDirection ?? SortDirection.Ascending;
            var filters = DeserializeFilters(serializedFilters);
            var sourceItems = source.ToList();
            var records = sourceItems.Where(movie => filters.All(filter => Matches(movie, filter)));
            var comparer = Comparer<object>.Create(CompareValues);

            records = sortDirection == SortDirection.Descending
                ? records.OrderByDescending(movie => GetSortValue(movie, sortKey), comparer).ThenByDescending(movie => movie.SortTitle).ThenByDescending(movie => movie.Id)
                : records.OrderBy(movie => GetSortValue(movie, sortKey), comparer).ThenBy(movie => movie.SortTitle).ThenBy(movie => movie.Id);

            var materialized = records.ToList();

            return new MoviePagingResource
            {
                Page = page,
                PageSize = pageSize,
                SortKey = sortKey,
                SortDirection = sortDirection,
                TotalRecords = materialized.Count,
                Records = materialized.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                MovieIds = materialized.Select(movie => movie.Id).ToList(),
                Facets = CreateFacets(sourceItems),
                JumpBar = CreateJumpBar(materialized, pageSize)
            };
        }

        private static Dictionary<string, MovieJumpResource> CreateJumpBar(List<MovieResource> movies, int pageSize)
        {
            var result = new Dictionary<string, MovieJumpResource>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < movies.Count; index++)
            {
                var title = movies[index].SortTitle;
                var character = string.IsNullOrWhiteSpace(title) || char.IsDigit(title[0]) ? "#" : char.ToUpperInvariant(title[0]).ToString();

                if (!result.TryGetValue(character, out var jump))
                {
                    jump = new MovieJumpResource { Page = (index / pageSize) + 1 };
                    result.Add(character, jump);
                }

                jump.Count++;
            }

            return result;
        }

        public static MovieFacetResource CreateFacets(List<MovieResource> movies)
        {
            return new MovieFacetResource
            {
                Certifications = Distinct(movies.Select(movie => movie.Certification)),
                Collections = Distinct(movies.Select(movie => movie.Collection?.Title)),
                Genres = Distinct(movies.SelectMany(movie => movie.Genres ?? new List<string>())),
                Keywords = Distinct(movies.SelectMany(movie => movie.Keywords ?? new List<string>())),
                OriginalLanguages = Distinct(movies.Select(movie => movie.OriginalLanguage?.Name)),
                ReleaseGroups = Distinct(movies.SelectMany(movie => movie.Statistics?.ReleaseGroups ?? new List<string>())),
                Studios = Distinct(movies.Select(movie => movie.Studio)),
                QualityProfileIds = movies.Select(movie => movie.QualityProfileId).Distinct().OrderBy(id => id).ToList(),
                TmdbIds = movies.Select(movie => movie.TmdbId).Distinct().OrderBy(id => id).ToList(),
                TotalRecords = movies.Count
            };
        }

        private static List<string> Distinct(IEnumerable<string> values)
        {
            return values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList();
        }

        private static List<MovieFilterResource> DeserializeFilters(string serializedFilters)
        {
            if (string.IsNullOrWhiteSpace(serializedFilters))
            {
                return new List<MovieFilterResource>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<MovieFilterResource>>(serializedFilters, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<MovieFilterResource>();
            }
            catch (JsonException)
            {
                return new List<MovieFilterResource>();
            }
        }

        private static bool Matches(MovieResource movie, MovieFilterResource filter)
        {
            var itemValue = GetValue(movie, filter.Key);
            var filterValues = filter.Value.ValueKind == JsonValueKind.Array
                ? filter.Value.EnumerateArray().ToList()
                : new List<JsonElement> { filter.Value };
            var isNegative = filter.Type is "notContains" or "notEqual" or "notStartsWith" or "notEndsWith";

            return isNegative
                ? filterValues.All(value => Compare(itemValue, value, filter.Type))
                : filterValues.Any(value => Compare(itemValue, value, filter.Type));
        }

        private static bool Compare(object itemValue, JsonElement filterValue, string type)
        {
            if (itemValue == null)
            {
                return false;
            }

            if (itemValue is DateTime dateTime)
            {
                return CompareDate(dateTime, filterValue, type);
            }

            if (itemValue is IEnumerable enumerable && itemValue is not string)
            {
                var values = enumerable.Cast<object>().ToList();
                var contains = values.Any(value => CompareScalar(value, filterValue) == 0);
                return type is "notContains" or "notEqual" ? !contains : contains;
            }

            var comparison = CompareScalar(itemValue, filterValue);

            return type switch
            {
                "contains" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.Contains(GetString(filterValue), StringComparison.OrdinalIgnoreCase) == true,
                "notContains" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.Contains(GetString(filterValue), StringComparison.OrdinalIgnoreCase) != true,
                "startsWith" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.StartsWith(GetString(filterValue), StringComparison.OrdinalIgnoreCase) == true,
                "notStartsWith" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.StartsWith(GetString(filterValue), StringComparison.OrdinalIgnoreCase) != true,
                "endsWith" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.EndsWith(GetString(filterValue), StringComparison.OrdinalIgnoreCase) == true,
                "notEndsWith" => Convert.ToString(itemValue, CultureInfo.InvariantCulture)?.EndsWith(GetString(filterValue), StringComparison.OrdinalIgnoreCase) != true,
                "notEqual" => comparison != 0,
                "greaterThan" => comparison > 0,
                "greaterThanOrEqual" => comparison >= 0,
                "lessThan" => comparison < 0,
                "lessThanOrEqual" => comparison <= 0,
                _ => comparison == 0
            };
        }

        private static bool CompareDate(DateTime itemValue, JsonElement filterValue, string type)
        {
            if (filterValue.ValueKind == JsonValueKind.Object && filterValue.TryGetProperty("value", out var value) && filterValue.TryGetProperty("time", out var time))
            {
                var amount = value.GetInt32();
                var boundary = AddTime(DateTime.UtcNow, time.GetString(), type is "inLast" or "notInLast" ? -amount : amount);
                return type switch
                {
                    "inLast" => itemValue > boundary && itemValue < DateTime.UtcNow,
                    "notInLast" => itemValue < boundary,
                    "inNext" => itemValue > DateTime.UtcNow && itemValue < boundary,
                    "notInNext" => itemValue > boundary,
                    _ => false
                };
            }

            if (!DateTime.TryParse(GetString(filterValue), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
            {
                return false;
            }

            return type switch
            {
                "lessThan" => itemValue < date,
                "greaterThan" => itemValue > date,
                _ => itemValue == date
            };
        }

        private static DateTime AddTime(DateTime date, string unit, int amount)
        {
            return unit switch
            {
                "years" => date.AddYears(amount),
                "months" => date.AddMonths(amount),
                "weeks" => date.AddDays(amount * 7),
                "days" => date.AddDays(amount),
                "hours" => date.AddHours(amount),
                _ => date.AddDays(amount)
            };
        }

        private static int CompareScalar(object itemValue, JsonElement filterValue)
        {
            if (itemValue is bool boolean && filterValue.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return boolean.CompareTo(filterValue.GetBoolean());
            }

            if (IsNumber(itemValue) && filterValue.TryGetDouble(out var number))
            {
                return Convert.ToDouble(itemValue, CultureInfo.InvariantCulture).CompareTo(number);
            }

            return string.Compare(Convert.ToString(itemValue, CultureInfo.InvariantCulture), GetString(filterValue), StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareValues(object left, object right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            if (IsNumber(left) && IsNumber(right))
            {
                return Convert.ToDouble(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(right, CultureInfo.InvariantCulture));
            }

            return left is IComparable comparable ? comparable.CompareTo(right) : string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNumber(object value) => value is byte or short or int or long or float or double or decimal;

        private static string GetString(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();

        private static object GetValue(MovieResource movie, string key)
        {
            return key switch
            {
                "added" => movie.Added,
                "certification" => movie.Certification,
                "collection" => movie.Collection?.Title,
                "digitalRelease" => movie.DigitalRelease,
                "genres" => movie.Genres,
                "hasFile" => movie.HasFile,
                "imdbRating" => movie.Ratings?.Imdb?.Value ?? 0,
                "imdbVotes" => movie.Ratings?.Imdb?.Votes ?? 0,
                "inCinemas" => movie.InCinemas,
                "isAvailable" => movie.IsAvailable,
                "keywords" => movie.Keywords,
                "minimumAvailability" => movie.MinimumAvailability.ToString(),
                "monitored" => movie.Monitored,
                "movieStatus" => GetMovieStatus(movie),
                "originalLanguage" => movie.OriginalLanguage?.Name,
                "originalTitle" => movie.OriginalTitle,
                "path" => movie.Path,
                "physicalRelease" => movie.PhysicalRelease,
                "popularity" => movie.Popularity,
                "qualityProfileId" => movie.QualityProfileId,
                "qualityCutoffNotMet" => movie.MovieFile?.QualityCutoffNotMet ?? false,
                "releaseDate" => movie.ReleaseDate,
                "releaseGroups" => movie.Statistics?.ReleaseGroups,
                "rottenTomatoesRating" => movie.Ratings?.RottenTomatoes?.Value ?? -1,
                "runtime" => movie.Runtime,
                "sizeOnDisk" => movie.Statistics?.SizeOnDisk ?? 0,
                "sortTitle" => movie.SortTitle,
                "status" => movie.Status.ToString(),
                "studio" => movie.Studio,
                "tags" => movie.Tags,
                "title" => movie.Title,
                "tmdbRating" => movie.Ratings?.Tmdb?.Value ?? 0,
                "tmdbVotes" => movie.Ratings?.Tmdb?.Votes ?? 0,
                "traktRating" => movie.Ratings?.Trakt?.Value ?? 0,
                "traktVotes" => movie.Ratings?.Trakt?.Votes ?? 0,
                "year" => movie.Year,
                _ => movie.SortTitle
            };
        }

        private static object GetSortValue(MovieResource movie, string key)
        {
            return key switch
            {
                "status" => (movie.Monitored ? 4 : 0) + (movie.Status.ToString() == "Announced" ? 1 : movie.Status.ToString() == "InCinemas" ? 2 : 3),
                "movieStatus" => GetMovieStatus(movie) switch
                {
                    "downloaded" => 4,
                    "cutoffNotMet" => 3,
                    "missing" => 2,
                    "notAvailable" => 1,
                    _ => 0
                },
                _ => GetValue(movie, key)
            };
        }

        private static string GetMovieStatus(MovieResource movie)
        {
            if (!movie.Monitored)
            {
                return "unmonitored";
            }

            if (movie.HasFile != true)
            {
                return movie.IsAvailable ? "missing" : "notAvailable";
            }

            return movie.MovieFile?.QualityCutoffNotMet == true ? "cutoffNotMet" : "downloaded";
        }
    }
}
