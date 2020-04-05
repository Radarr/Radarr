using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.PreDB;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideMovieInfo, ISearchForNewMovie, IDiscoverNewMovies
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _movieBuilder;
        private readonly ITmdbConfigService _tmdbConfigService;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IPreDBService _predbService;
        private readonly IImportExclusionsService _exclusionService;
        private readonly IRadarrAPIClient _radarrAPI;

        public SkyHookProxy(IHttpClient httpClient,
            IRadarrCloudRequestBuilder requestBuilder,
            ITmdbConfigService tmdbConfigService,
            IConfigService configService,
            IMovieService movieService,
            IPreDBService predbService,
            IImportExclusionsService exclusionService,
            IRadarrAPIClient radarrAPI,
            Logger logger)
        {
            _httpClient = httpClient;
            _movieBuilder = requestBuilder.TMDB;
            _tmdbConfigService = tmdbConfigService;
            _configService = configService;
            _movieService = movieService;
            _predbService = predbService;
            _exclusionService = exclusionService;
            _radarrAPI = radarrAPI;

            _logger = logger;
        }

        public HashSet<int> GetChangedMovies(DateTime startTime)
        {
            var startDate = startTime.ToString("o");

            var request = _movieBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "movie")
                .SetSegment("id", "")
                .SetSegment("secondaryRoute", "changes")
                .AddQueryParam("start_date", startDate)
                .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<MovieSearchRoot>(request);

            return new HashSet<int>(response.Resource.results.Select(c => c.id));
        }

        public Tuple<Movie, List<Credit>> GetMovieInfo(int tmdbId, Profile profile, bool hasPreDBEntry)
        {
            var langCode = profile != null ? IsoLanguages.Get(profile.Language)?.TwoLetterCode ?? "en" : "en";
            var wantedTitleLanguages = GetWantedTitleLanguages(langCode, profile);

            var request = _movieBuilder.Create()
               .SetSegment("api", "3")
               .SetSegment("route", "movie")
               .SetSegment("id", tmdbId.ToString())
               .SetSegment("secondaryRoute", "")
               .AddQueryParam("append_to_response", "alternative_titles,release_dates,videos,credits")
               .AddQueryParam("language", langCode.ToUpper())

               // .AddQueryParam("country", "US")
               .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<MovieResourceRoot>(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MovieNotFoundException(tmdbId);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException(request, response);
            }

            if (response.Headers.ContentType != HttpAccept.JsonCharset.Value)
            {
                throw new HttpException(request, response);
            }

            // The dude abides, so should us, Lets be nice to TMDb
            // var allowed = int.Parse(response.Headers.GetValues("X-RateLimit-Limit").First()); // get allowed
            // var reset = long.Parse(response.Headers.GetValues("X-RateLimit-Reset").First()); // get time when it resets
            if (response.Headers.ContainsKey("X-RateLimit-Remaining"))
            {
                var remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
                if (remaining <= 5)
                {
                    _logger.Trace("Waiting 5 seconds to get information for the next 35 movies");
                    Thread.Sleep(5000);
                }
            }

            var resource = response.Resource;
            if (resource.status_message != null)
            {
                if (resource.status_code == 34)
                {
                    _logger.Warn("Movie with TmdbId {0} could not be found. This is probably the case when the movie was deleted from TMDB.", tmdbId);
                    return null;
                }

                _logger.Warn(resource.status_message);
                return null;
            }

            var movie = new Movie();
            var altTitles = new List<AlternativeTitle>();

            if (langCode != "en")
            {
                var iso = IsoLanguages.Find(resource.original_language);
                if (iso != null)
                {
                    altTitles.Add(new AlternativeTitle(resource.original_title, SourceType.TMDB, tmdbId, iso.Language));
                }
            }

            foreach (var alternativeTitle in resource.alternative_titles.titles)
            {
                if (wantedTitleLanguages.Contains(alternativeTitle.iso_3166_1.ToLower()))
                {
                    altTitles.Add(new AlternativeTitle(alternativeTitle.title, SourceType.TMDB, tmdbId, IsoLanguages.Find(alternativeTitle.iso_3166_1.ToLower())?.Language ?? Language.English));
                }
                else if (alternativeTitle.iso_3166_1.ToLower() == "us")
                {
                    altTitles.Add(new AlternativeTitle(alternativeTitle.title, SourceType.TMDB, tmdbId, Language.English));
                }
            }

            movie.TmdbId = tmdbId;
            movie.ImdbId = resource.imdb_id;
            movie.Title = resource.title;
            movie.TitleSlug = Parser.Parser.ToUrlSlug(resource.title);
            movie.CleanTitle = resource.title.CleanSeriesTitle();
            movie.SortTitle = Parser.Parser.NormalizeTitle(resource.title);
            movie.Overview = resource.overview;
            movie.Website = resource.homepage;

            if (resource.release_date.IsNotNullOrWhiteSpace())
            {
                movie.InCinemas = DateTime.Parse(resource.release_date);

                movie.Year = movie.InCinemas.Value.Year;
            }

            movie.TitleSlug += "-" + movie.TmdbId.ToString();

            movie.Images.AddIfNotNull(MapImage(resource.poster_path, MediaCoverTypes.Poster)); //TODO: Update to load image specs from tmdb page!
            movie.Images.AddIfNotNull(MapImage(resource.backdrop_path, MediaCoverTypes.Fanart));
            movie.Runtime = resource.runtime;

            foreach (var releaseDates in resource.release_dates.results)
            {
                foreach (var releaseDate in releaseDates.release_dates)
                {
                    if (releaseDate.type == 5 || releaseDate.type == 4)
                    {
                        if (movie.PhysicalRelease.HasValue)
                        {
                            if (movie.PhysicalRelease.Value.After(DateTime.Parse(releaseDate.release_date)))
                            {
                                movie.PhysicalRelease = DateTime.Parse(releaseDate.release_date); //Use oldest release date available.
                                movie.PhysicalReleaseNote = releaseDate.note;
                            }
                        }
                        else
                        {
                            movie.PhysicalRelease = DateTime.Parse(releaseDate.release_date);
                            movie.PhysicalReleaseNote = releaseDate.note;
                        }
                    }

                    // Set Certification from Theatrical Release
                    if (releaseDate.type == 3 && releaseDates.iso_3166_1 == _configService.CertificationCountry.ToString())
                    {
                        movie.Certification = releaseDate.certification;
                    }
                }
            }

            movie.Ratings = new Ratings();
            movie.Ratings.Votes = resource.vote_count;
            movie.Ratings.Value = (decimal)resource.vote_average;

            foreach (var genre in resource.genres)
            {
                movie.Genres.Add(genre.name);
            }

            var now = DateTime.Now;

            //handle the case when we have both theatrical and physical release dates
            if (movie.InCinemas.HasValue && movie.PhysicalRelease.HasValue)
            {
                if (now < movie.InCinemas)
                {
                    movie.Status = MovieStatusType.Announced;
                }
                else if (now >= movie.InCinemas)
                {
                    movie.Status = MovieStatusType.InCinemas;
                }

                if (now >= movie.PhysicalRelease)
                {
                    movie.Status = MovieStatusType.Released;
                }
            }

            //handle the case when we have theatrical release dates but we dont know the physical release date
            else if (movie.InCinemas.HasValue && (now >= movie.InCinemas))
            {
                movie.Status = MovieStatusType.InCinemas;
            }

            //handle the case where we only have a physical release date
            else if (movie.PhysicalRelease.HasValue && (now >= movie.PhysicalRelease))
            {
                movie.Status = MovieStatusType.Released;
            }

            //otherwise the title has only been announced
            else
            {
                movie.Status = MovieStatusType.Announced;
            }

            //since TMDB lacks alot of information lets assume that stuff is released if its been in cinemas for longer than 3 months.
            if (!movie.PhysicalRelease.HasValue && (movie.Status == MovieStatusType.InCinemas) && (DateTime.Now.Subtract(movie.InCinemas.Value).TotalSeconds > 60 * 60 * 24 * 30 * 3))
            {
                movie.Status = MovieStatusType.Released;
            }

            if (!hasPreDBEntry)
            {
                if (_predbService.HasReleases(movie))
                {
                    movie.HasPreDBEntry = true;
                }
                else
                {
                    movie.HasPreDBEntry = false;
                }
            }

            if (resource.videos != null)
            {
                foreach (Video video in resource.videos.results)
                {
                    if (video.type == "Trailer" && video.site == "YouTube")
                    {
                        if (video.key != null)
                        {
                            movie.YouTubeTrailerId = video.key;
                            break;
                        }
                    }
                }
            }

            if (resource.production_companies != null)
            {
                if (resource.production_companies.Any())
                {
                    movie.Studio = resource.production_companies[0].name;
                }
            }

            movie.AlternativeTitles.AddRange(altTitles);

            var people = new List<Credit>();

            people.AddRange(resource.credits.Cast.Select(MapCast).ToList());
            people.AddRange(resource.credits.Crew.Select(MapCrew).ToList());

            if (resource.belongs_to_collection != null)
            {
                movie.Collection = MapCollection(resource.belongs_to_collection);

                movie.Collection.Images.AddIfNotNull(MapImage(resource.belongs_to_collection.poster_path, MediaCoverTypes.Poster));
                movie.Collection.Images.AddIfNotNull(MapImage(resource.belongs_to_collection.backdrop_path, MediaCoverTypes.Fanart));
            }

            return new Tuple<Movie, List<Credit>>(movie, people);
        }

        private List<string> GetWantedTitleLanguages(string langCode, Profile profile = null)
        {
            if (profile == null)
            {
                _logger.Trace("Profile is null! Defaulting to language code {}", langCode);
                return new List<string> { langCode };
            }

            _logger.Trace("Profile formatItems: {}", profile.FormatItems.ToArray());

            var wantedTitleLanguages = profile.FormatItems.Select(item => item.Format)
                .SelectMany(format => format.Specifications)
                .Where(specification => specification is LanguageSpecification && !specification.Negate)
                .Cast<LanguageSpecification>()
                .Where(specification => specification.Value != -1)
                .Select(specification => (Language)specification.Value)
                .Select(language => IsoLanguages.Get(language).TwoLetterCode)
                .Distinct()
                .ToList();

            if (!wantedTitleLanguages.Contains(langCode))
            {
                wantedTitleLanguages.Add(langCode);
            }

            _logger.Debug("WantedTitleLanguages {}", wantedTitleLanguages.ToArray());

            return wantedTitleLanguages;
        }

        public Movie GetMovieInfo(string imdbId)
        {
            var request = _movieBuilder.Create()
                .SetSegment("api", "3")
                .SetSegment("route", "find")
                .SetSegment("id", imdbId)
                .SetSegment("secondaryRoute", "")
                .AddQueryParam("external_source", "imdb_id")
                .Build();

            request.AllowAutoRedirect = true;

            // request.SuppressHttpError = true;
            var response = _httpClient.Get<FindRoot>(request);

            if (response.HasHttpError)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(imdbId);
                }
                else
                {
                    throw new HttpException(request, response);
                }
            }

            // The dude abides, so should us, Lets be nice to TMDb
            // var allowed = int.Parse(response.Headers.GetValues("X-RateLimit-Limit").First()); // get allowed
            // var reset = long.Parse(response.Headers.GetValues("X-RateLimit-Reset").First()); // get time when it resets
            if (response.Headers.ContainsKey("X-RateLimit-Remaining"))
            {
                var remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
                if (remaining <= 5)
                {
                    _logger.Trace("Waiting 5 seconds to get information for the next 35 movies");
                    Thread.Sleep(5000);
                }
            }

            if (!response.Resource.movie_results.Any())
            {
                throw new MovieNotFoundException(imdbId);
            }

            return MapMovie(response.Resource.movie_results.First());
        }

        public List<Movie> DiscoverNewMovies(string action)
        {
            var allMovies = _movieService.GetAllMovies();
            var allExclusions = _exclusionService.GetAllExclusions();
            string allIds = string.Join(",", allMovies.Select(m => m.TmdbId));
            string ignoredIds = string.Join(",", allExclusions.Select(ex => ex.TmdbId));

            List<MovieResult> results = new List<MovieResult>();

            try
            {
                results = _radarrAPI.DiscoverMovies(action, (request) =>
                {
                    request.AllowAutoRedirect = true;
                    request.Method = HttpMethod.POST;
                    request.Headers.ContentType = "application/x-www-form-urlencoded";
                    request.SetContent($"tmdbIds={allIds}&ignoredIds={ignoredIds}");
                    return request;
                });

                results = results.Where(m => allMovies.None(mo => mo.TmdbId == m.id) && allExclusions.None(ex => ex.TmdbId == m.id)).ToList();
            }
            catch (RadarrAPIException exception)
            {
                _logger.Error(exception, "Failed to discover movies for action {0}!", action);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to discover movies for action {0}!", action);
            }

            return results.SelectList(MapMovie);
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

        public List<Movie> SearchForNewMovie(string title)
        {
            try
            {
                var lowerTitle = title.ToLower();

                lowerTitle = lowerTitle.Replace(".", "");

                var parserResult = Parser.Parser.ParseMovieTitle(title, true, true);

                var yearTerm = "";

                if (parserResult != null && parserResult.MovieTitle != title)
                {
                    //Parser found something interesting!
                    lowerTitle = parserResult.MovieTitle.ToLower().Replace(".", " "); //TODO Update so not every period gets replaced (e.g. R.I.P.D.)
                    if (parserResult.Year > 1800)
                    {
                        yearTerm = parserResult.Year.ToString();
                    }

                    if (parserResult.ImdbId.IsNotNullOrWhiteSpace())
                    {
                        try
                        {
                            return new List<Movie> { GetMovieInfo(parserResult.ImdbId) };
                        }
                        catch (Exception)
                        {
                            return new List<Movie>();
                        }
                    }
                }

                lowerTitle = StripTrailingTheFromTitle(lowerTitle);

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
                        return new List<Movie> { GetMovieInfo(imdbid) };
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
                        return new List<Movie> { GetMovieInfo(tmdbid, null, false).Item1 };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                var searchTerm = lowerTitle.Replace("_", "+").Replace(" ", "+").Replace(".", "+");

                var firstChar = searchTerm.First();

                var request = _movieBuilder.Create()
                    .SetSegment("api", "3")
                    .SetSegment("route", "search")
                    .SetSegment("id", "movie")
                    .SetSegment("secondaryRoute", "")
                    .AddQueryParam("query", searchTerm)
                    .AddQueryParam("year", yearTerm)
                    .AddQueryParam("include_adult", false)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var response = _httpClient.Get<MovieSearchRoot>(request);

                var movieResults = response.Resource.results;

                return movieResults.SelectList(MapSearchResult);
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

        private Movie MapSearchResult(MovieResult result)
        {
            var movie = _movieService.FindByTmdbId(result.id);

            if (movie == null)
            {
                movie = MapMovie(result);
            }

            return movie;
        }

        public Movie MapMovie(MovieResult result)
        {
            var imdbMovie = new Movie();
            imdbMovie.TmdbId = result.id;
            try
            {
                imdbMovie.SortTitle = Parser.Parser.NormalizeTitle(result.title);
                imdbMovie.Title = result.title;
                imdbMovie.TitleSlug = Parser.Parser.ToUrlSlug(result.title);

                try
                {
                    if (result.release_date.IsNotNullOrWhiteSpace())
                    {
                        imdbMovie.InCinemas = DateTime.ParseExact(result.release_date, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                        imdbMovie.Year = imdbMovie.InCinemas.Value.Year;
                    }

                    if (result.physical_release.IsNotNullOrWhiteSpace())
                    {
                        imdbMovie.PhysicalRelease = DateTime.ParseExact(result.physical_release, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                        if (result.physical_release_note.IsNotNullOrWhiteSpace())
                        {
                            imdbMovie.PhysicalReleaseNote = result.physical_release_note;
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Debug("Not a valid date time.");
                }

                var now = DateTime.Now;

                //handle the case when we have both theatrical and physical release dates
                if (imdbMovie.InCinemas.HasValue && imdbMovie.PhysicalRelease.HasValue)
                {
                    if (now < imdbMovie.InCinemas)
                    {
                        imdbMovie.Status = MovieStatusType.Announced;
                    }
                    else if (now >= imdbMovie.InCinemas)
                    {
                        imdbMovie.Status = MovieStatusType.InCinemas;
                    }

                    if (now >= imdbMovie.PhysicalRelease)
                    {
                        imdbMovie.Status = MovieStatusType.Released;
                    }
                }

                //handle the case when we have theatrical release dates but we dont know the physical release date
                else if (imdbMovie.InCinemas.HasValue && (now >= imdbMovie.InCinemas))
                {
                    imdbMovie.Status = MovieStatusType.InCinemas;
                }

                //handle the case where we only have a physical release date
                else if (imdbMovie.PhysicalRelease.HasValue && (now >= imdbMovie.PhysicalRelease))
                {
                    imdbMovie.Status = MovieStatusType.Released;
                }

                //otherwise the title has only been announced
                else
                {
                    imdbMovie.Status = MovieStatusType.Announced;
                }

                //since TMDB lacks alot of information lets assume that stuff is released if its been in cinemas for longer than 3 months.
                if (!imdbMovie.PhysicalRelease.HasValue && (imdbMovie.Status == MovieStatusType.InCinemas) && (DateTime.Now.Subtract(imdbMovie.InCinemas.Value).TotalSeconds > 60 * 60 * 24 * 30 * 3))
                {
                    imdbMovie.Status = MovieStatusType.Released;
                }

                imdbMovie.TitleSlug += "-" + imdbMovie.TmdbId;

                imdbMovie.Images = new List<MediaCover.MediaCover>();
                imdbMovie.Overview = result.overview;
                imdbMovie.Ratings = new Ratings { Value = (decimal)result.vote_average, Votes = result.vote_count };

                try
                {
                    imdbMovie.Images.AddIfNotNull(MapImage(result.poster_path, MediaCoverTypes.Poster));
                }
                catch (Exception)
                {
                    _logger.Debug(result);
                }

                if (result.trailer_key.IsNotNullOrWhiteSpace() && result.trailer_site.IsNotNullOrWhiteSpace())
                {
                    if (result.trailer_site == "youtube")
                    {
                        imdbMovie.YouTubeTrailerId = result.trailer_key;
                    }
                }

                return imdbMovie;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error occured while searching for new movies.");
            }

            return null;
        }

        private static Credit MapCast(CastResource arg)
        {
            var newActor = new Credit
            {
                Name = arg.Name,
                Character = arg.Character,
                Order = arg.Order,
                CreditTmdbId = arg.Credit_Id,
                PersonTmdbId = arg.Id,
                Type = CreditType.Cast
            };

            if (arg.Profile_Path != null)
            {
                newActor.Images = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover(MediaCoverTypes.Headshot, "https://image.tmdb.org/t/p/original" + arg.Profile_Path)
                };
            }

            return newActor;
        }

        private static Credit MapCrew(CrewResource arg)
        {
            var newActor = new Credit
            {
                Name = arg.Name,
                Department = arg.Department,
                Job = arg.Job,
                CreditTmdbId = arg.Credit_Id,
                PersonTmdbId = arg.Id,
                Type = CreditType.Crew
            };

            if (arg.Profile_Path != null)
            {
                newActor.Images = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover(MediaCoverTypes.Headshot, "https://image.tmdb.org/t/p/original" + arg.Profile_Path)
                };
            }

            return newActor;
        }

        private static MovieCollection MapCollection(CollectionResource arg)
        {
            var newCollection = new MovieCollection
            {
                Name = arg.name,
                TmdbId = arg.id,
            };

            return newCollection;
        }

        private MediaCover.MediaCover MapImage(string path, MediaCoverTypes type)
        {
            if (path.IsNotNullOrWhiteSpace())
            {
                return _tmdbConfigService.GetCoverForURL(path, type);
            }

            return null;
        }

        public Movie MapMovieToTmdbMovie(Movie movie)
        {
            try
            {
                Movie newMovie = movie;
                if (movie.TmdbId > 0)
                {
                    newMovie = GetMovieInfo(movie.TmdbId, null, false).Item1;
                }
                else if (movie.ImdbId.IsNotNullOrWhiteSpace())
                {
                    newMovie = GetMovieInfo(movie.ImdbId);
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
    }
}
