using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideMovieInfo, ISearchForNewMovie
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _radarrMetadata;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IMovieTranslationService _movieTranslationService;

        public SkyHookProxy(IHttpClient httpClient,
            IRadarrCloudRequestBuilder requestBuilder,
            IConfigService configService,
            IMovieService movieService,
            IMovieTranslationService movieTranslationService,
            Logger logger)
        {
            _httpClient = httpClient;
            _radarrMetadata = requestBuilder.RadarrMetadata;
            _configService = configService;
            _movieService = movieService;
            _movieTranslationService = movieTranslationService;

            _logger = logger;
        }

        public HashSet<int> GetChangedMovies(DateTime startTime)
        {
            // Round down to the hour to ensure we cover gap and don't kill cache every call
            var cacheAdjustedStart = startTime.AddMinutes(-15);
            var startDate = cacheAdjustedStart.Date.AddHours(cacheAdjustedStart.Hour).ToString("s");

            var request = _radarrMetadata.Create()
                .SetSegment("route", "movie/changed")
                .AddQueryParam("since", startDate)
                .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<List<int>>(request);

            return new HashSet<int>(response.Resource);
        }

        public Tuple<Movie, List<Credit>> GetMovieInfo(int tmdbId)
        {
            var httpRequest = _radarrMetadata.Create()
                                             .SetSegment("route", "movie")
                                             .Resource(tmdbId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<MovieResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(tmdbId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var credits = new List<Credit>();
            credits.AddRange(httpResponse.Resource.Credits.Cast.Select(MapCast));
            credits.AddRange(httpResponse.Resource.Credits.Crew.Select(MapCrew));

            var movie = MapMovie(httpResponse.Resource);

            return new Tuple<Movie, List<Credit>>(movie, credits.ToList());
        }

        public List<Movie> GetBulkMovieInfo(List<int> tmdbIds)
        {
            var httpRequest = _radarrMetadata.Create()
                                             .SetSegment("route", "movie/bulk")
                                             .Build();

            httpRequest.Headers.ContentType = "application/json";

            httpRequest.SetContent(tmdbIds.ToJson());

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Post<List<MovieResource>>(httpRequest);

            if (httpResponse.HasHttpError || httpResponse.Resource.Count == 0)
            {
                throw new HttpException(httpRequest, httpResponse);
            }

            var movies = httpResponse.Resource.Select(MapMovie).ToList();

            return movies;
        }

        public Movie GetMovieByImdbId(string imdbId)
        {
            var httpRequest = _radarrMetadata.Create()
                                             .SetSegment("route", "movie/imdb")
                                             .Resource(imdbId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<List<MovieResource>>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(imdbId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var movie = httpResponse.Resource.SelectList(MapMovie).FirstOrDefault();

            return movie;
        }

        public Movie MapMovie(MovieResource resource)
        {
            var movie = new Movie();
            var altTitles = new List<AlternativeTitle>();

            movie.TmdbId = resource.TmdbId;
            movie.ImdbId = resource.ImdbId;
            movie.Title = resource.Title;
            movie.OriginalTitle = resource.OriginalTitle;
            movie.TitleSlug = resource.TitleSlug;
            movie.CleanTitle = resource.Title.CleanMovieTitle();
            movie.SortTitle = Parser.Parser.NormalizeTitle(resource.Title);
            movie.Overview = resource.Overview;

            movie.AlternativeTitles.AddRange(resource.AlternativeTitles.Select(MapAlternativeTitle));

            movie.Translations.AddRange(resource.Translations.Select(MapTranslation));

            movie.OriginalLanguage = IsoLanguages.Find(resource.OriginalLanguage.ToLower())?.Language ?? Language.English;

            movie.Website = resource.Homepage;
            movie.InCinemas = resource.InCinema;
            movie.PhysicalRelease = resource.PhysicalRelease;
            movie.DigitalRelease = resource.DigitalRelease;

            movie.Year = resource.Year;

            //If the premier differs from the TMDB year, use it as a secondary year.
            if (resource.Premier.HasValue && resource.Premier?.Year != movie.Year)
            {
                movie.SecondaryYear = resource.Premier?.Year;
            }

            movie.Images = resource.Images.Select(MapImage).ToList();

            if (resource.Runtime != null)
            {
                movie.Runtime = resource.Runtime.Value;
            }

            var certificationCountry = _configService.CertificationCountry.ToString();

            movie.Certification = resource.Certifications.FirstOrDefault(m => m.Country == certificationCountry)?.Certification;
            movie.Ratings = resource.Ratings.Select(MapRatings).FirstOrDefault() ?? new Ratings();
            movie.Genres = resource.Genres;
            movie.Recommendations = resource.Recommendations?.Select(r => r.TmdbId).ToList() ?? new List<int>();

            var now = DateTime.Now;

            movie.Status = MovieStatusType.Announced;

            if (resource.InCinema.HasValue && now > resource.InCinema)
            {
                movie.Status = MovieStatusType.InCinemas;

                if (!resource.PhysicalRelease.HasValue && !resource.DigitalRelease.HasValue && now > resource.InCinema.Value.AddDays(90))
                {
                    movie.Status = MovieStatusType.Released;
                }
            }

            if (resource.PhysicalRelease.HasValue && now >= resource.PhysicalRelease)
            {
                movie.Status = MovieStatusType.Released;
            }

            if (resource.DigitalRelease.HasValue && now >= resource.DigitalRelease)
            {
                movie.Status = MovieStatusType.Released;
            }

            movie.YouTubeTrailerId = resource.YoutubeTrailerId;
            movie.Studio = resource.Studio;

            if (resource.Collection != null)
            {
                movie.Collection = new MovieCollection { Name = resource.Collection.Name, TmdbId = resource.Collection.TmdbId };
            }

            return movie;
        }

        private string StripTrailingTheFromTitle(string title)
        {
            if (title.EndsWith(",the"))
            {
                title = title.Substring(0, title.Length - 4);
            }
            else if (title.EndsWith(", the"))
            {
                title = title.Substring(0, title.Length - 5);
            }

            return title;
        }

        public Movie MapMovieToTmdbMovie(Movie movie)
        {
            try
            {
                var newMovie = movie;

                if (movie.TmdbId > 0)
                {
                    newMovie = _movieService.FindByTmdbId(movie.TmdbId);

                    if (newMovie == null)
                    {
                        newMovie = GetMovieInfo(movie.TmdbId).Item1;
                    }
                }
                else if (movie.ImdbId.IsNotNullOrWhiteSpace())
                {
                    newMovie = GetMovieByImdbId(movie.ImdbId);
                }
                else
                {
                    var yearStr = "";
                    if (movie.Year > 1900)
                    {
                        yearStr = $" {movie.Year}";
                    }

                    newMovie = SearchForNewMovie(movie.Title + yearStr).FirstOrDefault();
                }

                if (newMovie == null)
                {
                    _logger.Warn("Couldn't map movie {0} to a movie on The Movie DB. It will not be added :(", movie.Title);
                    return null;
                }

                newMovie.Path = movie.Path;
                newMovie.RootFolderPath = movie.RootFolderPath;
                newMovie.ProfileId = movie.ProfileId;
                newMovie.Monitored = movie.Monitored;
                newMovie.MovieFile = movie.MovieFile;
                newMovie.MinimumAvailability = movie.MinimumAvailability;
                newMovie.Tags = movie.Tags;

                return newMovie;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Couldn't map movie {0} to a movie on The Movie DB. It will not be added :(", movie.Title);
                return null;
            }
        }

        public List<Movie> SearchForNewMovie(string title)
        {
            try
            {
                var lowerTitle = title.ToLower();

                lowerTitle = lowerTitle.Replace(".", "");

                var parserTitle = lowerTitle;

                var parserResult = Parser.Parser.ParseMovieTitle(title, true);

                var yearTerm = "";

                if (parserResult != null && parserResult.MovieTitle != title)
                {
                    //Parser found something interesting!
                    parserTitle = parserResult.MovieTitle.ToLower().Replace(".", " "); //TODO Update so not every period gets replaced (e.g. R.I.P.D.)
                    if (parserResult.Year > 1800)
                    {
                        yearTerm = parserResult.Year.ToString();
                    }

                    if (parserResult.ImdbId.IsNotNullOrWhiteSpace())
                    {
                        try
                        {
                            var movieLookup = GetMovieByImdbId(parserResult.ImdbId);
                            return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? movieLookup };
                        }
                        catch (Exception)
                        {
                            return new List<Movie>();
                        }
                    }
                }

                parserTitle = StripTrailingTheFromTitle(parserTitle);

                if (lowerTitle.StartsWith("imdb:") || lowerTitle.StartsWith("imdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    string imdbid = slug;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var movieLookup = GetMovieByImdbId(imdbid);
                        return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? movieLookup };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    int tmdbid = -1;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out tmdbid))
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var movieLookup = GetMovieInfo(tmdbid).Item1;
                        return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? movieLookup };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                var searchTerm = parserTitle.Replace("_", "+").Replace(" ", "+").Replace(".", "+");

                var firstChar = searchTerm.First();

                var request = _radarrMetadata.Create()
                    .SetSegment("route", "search")
                    .AddQueryParam("q", searchTerm)
                    .AddQueryParam("year", yearTerm)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var httpResponse = _httpClient.Get<List<MovieResource>>(request);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException)
            {
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with TMDb.", title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from TMDb.", title);
            }
        }

        private Movie MapSearchResult(MovieResource result)
        {
            var movie = _movieService.FindByTmdbId(result.TmdbId);

            if (movie == null)
            {
                movie = MapMovie(result);
            }
            else
            {
                movie.Translations = _movieTranslationService.GetAllTranslationsForMovie(movie.Id);
            }

            return movie;
        }

        private static Credit MapCast(CastResource arg)
        {
            var newActor = new Credit
            {
                Name = arg.Name,
                Character = arg.Character,
                Order = arg.Order,
                CreditTmdbId = arg.CreditId,
                PersonTmdbId = arg.TmdbId,
                Type = CreditType.Cast,
                Images = arg.Images.Select(MapImage).ToList()
            };

            return newActor;
        }

        private static Credit MapCrew(CrewResource arg)
        {
            var newActor = new Credit
            {
                Name = arg.Name,
                Department = arg.Department,
                Job = arg.Job,
                CreditTmdbId = arg.CreditId,
                PersonTmdbId = arg.TmdbId,
                Type = CreditType.Crew,
                Images = arg.Images.Select(MapImage).ToList()
            };

            return newActor;
        }

        private static AlternativeTitle MapAlternativeTitle(AlternativeTitleResource arg)
        {
            var newAlternativeTitle = new AlternativeTitle
            {
                Title = arg.Title,
                SourceType = SourceType.TMDB,
                CleanTitle = arg.Title.CleanMovieTitle(),
                Language = IsoLanguages.Find(arg.Language.ToLower())?.Language ?? Language.English
            };

            return newAlternativeTitle;
        }

        private static MovieTranslation MapTranslation(TranslationResource arg)
        {
            var newAlternativeTitle = new MovieTranslation
            {
                Title = arg.Title,
                Overview = arg.Overview,
                CleanTitle = arg.Title.CleanMovieTitle(),
                Language = IsoLanguages.Find(arg.Language.ToLower())?.Language
            };

            return newAlternativeTitle;
        }

        private static Ratings MapRatings(RatingResource rating)
        {
            if (rating == null)
            {
                return new Ratings();
            }

            return new Ratings
            {
                Votes = rating.Count,
                Value = rating.Value
            };
        }

        private static MediaCover.MediaCover MapImage(ImageResource arg)
        {
            return new MediaCover.MediaCover
            {
                Url = arg.Url,
                CoverType = MapCoverType(arg.CoverType)
            };
        }

        private static MediaCoverTypes MapCoverType(string coverType)
        {
            switch (coverType.ToLower())
            {
                case "poster":
                    return MediaCoverTypes.Poster;
                case "headshot":
                    return MediaCoverTypes.Headshot;
                case "fanart":
                    return MediaCoverTypes.Fanart;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }
    }
}
