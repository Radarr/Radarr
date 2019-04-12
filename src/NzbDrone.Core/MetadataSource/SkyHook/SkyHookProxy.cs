using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
 using System.ServiceModel;
 using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.PreDB;
using NzbDrone.Core.Movies;
using System.Threading;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.RadarrAPI;
 using NzbDrone.Core.Movies.AlternativeTitles;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideMovieInfo, ISearchForNewMovie, IDiscoverNewMovies
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        
        private readonly IHttpRequestBuilderFactory _movieBuilder;
        private readonly ITmdbConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IPreDBService _predbService;
        private readonly IImportExclusionsService _exclusionService;
        private readonly IAlternativeTitleService _altTitleService;
        private readonly IRadarrAPIClient _radarrAPI;

        public SkyHookProxy(IHttpClient httpClient, IRadarrCloudRequestBuilder requestBuilder, ITmdbConfigService configService, IMovieService movieService,
                            IPreDBService predbService, IImportExclusionsService exclusionService, IAlternativeTitleService altTitleService, IRadarrAPIClient radarrAPI, Logger logger)
        {
            _httpClient = httpClient;
            _movieBuilder = requestBuilder.TMDB;
            _configService = configService;
            _movieService = movieService;
            _predbService = predbService;
            _exclusionService = exclusionService;
            _altTitleService = altTitleService;
            _radarrAPI = radarrAPI;

            _logger = logger;
        }

        public Movie GetMovieInfo(int TmdbId, Profile profile = null, bool hasPreDBEntry = false)
        {
            var langCode = profile != null ? IsoLanguages.Get(profile.Language)?.TwoLetterCode ?? "en" : "en";

            var request = _movieBuilder.Create()
               .SetSegment("route", "movie")
               .SetSegment("id", TmdbId.ToString())
               .SetSegment("secondaryRoute", "")
               .AddQueryParam("append_to_response", "alternative_titles,release_dates,videos")
               .AddQueryParam("language", langCode.ToUpper())
               // .AddQueryParam("country", "US")
               .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<MovieResourceRoot>(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MovieNotFoundException("Movie not found.");
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
            var remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
            if (remaining <= 5)
            {
                _logger.Trace("Waiting 5 seconds to get information for the next 35 movies");
                Thread.Sleep(5000);
            }

            var resource = response.Resource;
            if (resource.status_message != null)
            {
                if (resource.status_code == 34)
                {
                    _logger.Warn("Movie with TmdbId {0} could not be found. This is probably the case when the movie was deleted from TMDB.", TmdbId);
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
			        altTitles.Add(new AlternativeTitle(resource.original_title, SourceType.TMDB, TmdbId, iso.Language));
			    }

				//movie.AlternativeTitles.Add(resource.original_title);
			}

            foreach (var alternativeTitle in resource.alternative_titles.titles)
            {
                if (alternativeTitle.iso_3166_1.ToLower() == langCode)
                {
                    altTitles.Add(new AlternativeTitle(alternativeTitle.title, SourceType.TMDB, TmdbId, IsoLanguages.Find(alternativeTitle.iso_3166_1.ToLower())?.Language ?? Language.English));
                }
                else if (alternativeTitle.iso_3166_1.ToLower() == "us")
                {
                    altTitles.Add(new AlternativeTitle(alternativeTitle.title, SourceType.TMDB, TmdbId, Language.English));
                }
            }

            movie.TmdbId = TmdbId;
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

                // get the lowest year in all release date
                var lowestYear = new List<int>();
                foreach (ReleaseDates releaseDates in resource.release_dates.results)
                {
                    foreach (ReleaseDate releaseDate in releaseDates.release_dates)
                    {
                        lowestYear.Add(DateTime.Parse(releaseDate.release_date).Year);
                    }
                }
                movie.Year = lowestYear.Min();
            }

            movie.TitleSlug += "-" + movie.TmdbId.ToString();

            movie.Images.Add(_configService.GetCoverForURL(resource.poster_path, MediaCoverTypes.Poster));//TODO: Update to load image specs from tmdb page!
            movie.Images.Add(_configService.GetCoverForURL(resource.backdrop_path, MediaCoverTypes.Fanart));
            movie.Runtime = resource.runtime;

            //foreach(Title title in resource.alternative_titles.titles)
            //{
            //    movie.AlternativeTitles.Add(title.title);
            //}

            foreach(ReleaseDates releaseDates in resource.release_dates.results)
            {
                foreach(ReleaseDate releaseDate in releaseDates.release_dates)
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
                }
            }

            movie.Ratings = new Ratings();
            movie.Ratings.Votes = resource.vote_count;
            movie.Ratings.Value = (decimal)resource.vote_average;

            foreach(Genre genre in resource.genres)
            {
                movie.Genres.Add(genre.name);
            }

            //this is the way it should be handled
            //but unfortunately it seems
            //tmdb lacks alot of release date info
            //omdbapi is actually quite good for this info
            //except omdbapi has been having problems recently
            //so i will just leave this in as a comment
            //and use the 3 month logic that we were using before
            /*var now = DateTime.Now;
            if (now < movie.InCinemas)
                movie.Status = MovieStatusType.Announced;
            if (now >= movie.InCinemas)
                movie.Status = MovieStatusType.InCinemas;
            if (now >= movie.PhysicalRelease)
                movie.Status = MovieStatusType.Released;
            */


            var now = DateTime.Now;
            //handle the case when we have both theatrical and physical release dates
            if (movie.InCinemas.HasValue && movie.PhysicalRelease.HasValue)
            {
                if (now < movie.InCinemas)
                    movie.Status = MovieStatusType.Announced;
                else if (now >= movie.InCinemas)
                    movie.Status = MovieStatusType.InCinemas;
                if (now >= movie.PhysicalRelease)
                    movie.Status = MovieStatusType.Released;
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
            if (!movie.PhysicalRelease.HasValue && (movie.Status == MovieStatusType.InCinemas) && (((DateTime.Now).Subtract(movie.InCinemas.Value)).TotalSeconds > 60*60*24*30*3))
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

            //this matches with the old behavior before the creation of the MovieStatusType.InCinemas
            /*if (resource.status == "Released")
            {
                if (movie.InCinemas.HasValue && (((DateTime.Now).Subtract(movie.InCinemas.Value)).TotalSeconds <= 60 * 60 * 24 * 30 * 3))
                {
                    movie.Status = MovieStatusType.InCinemas;
                }
                else
                {
                    movie.Status = MovieStatusType.Released;
                }
            }
            else
            {
                movie.Status = MovieStatusType.Announced;
            }*/

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

            return movie;
        }

        public Movie GetMovieInfo(string imdbId)
        {
            var request = _movieBuilder.Create()
                .SetSegment("route", "find")
                .SetSegment("id", imdbId)
                .SetSegment("secondaryRoute", "")
                .AddQueryParam("external_source", "imdb_id")
                .Build();

            request.AllowAutoRedirect = true;
            // request.SuppressHttpError = true;

            var response = _httpClient.Get<FindRoot>(request);
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
            var remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
            if (remaining <= 5)
            {
                _logger.Trace("Waiting 5 seconds to get information for the next 35 movies");
                Thread.Sleep(5000);
            }

            var resources = response.Resource;

            return resources.movie_results.SelectList(MapMovie).FirstOrDefault();
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
            if(title.EndsWith(",the"))
            {
                title = title.Substring(0, title.Length - 4);
            } else if(title.EndsWith(", the"))
            {
                title = title.Substring(0, title.Length - 5);
            }
            return title;
        }

        public List<Movie> SearchForNewMovie(string title)
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
                    catch (Exception e)
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

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !(int.TryParse(slug, out tmdbid)))
                {
                    return new List<Movie>();
                }

                try
                {
                    return new List<Movie> { GetMovieInfo(tmdbid) };
                }
                catch (MovieNotFoundException)
                {
                    return new List<Movie>();
                }
            }

            var searchTerm = lowerTitle.Replace("_", "+").Replace(" ", "+").Replace(".", "+");

            var firstChar = searchTerm.First();

            var request = _movieBuilder.Create()
                .SetSegment("route", "search")
                .SetSegment("id", "movie")
                .SetSegment("secondaryRoute", "")
                .AddQueryParam("query", searchTerm)
                .AddQueryParam("year", yearTerm)
                .AddQueryParam("include_adult", false)
                .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            /*var imdbRequest = new HttpRequest("https://v2.sg.media-imdb.com/suggests/" + firstChar + "/" + searchTerm + ".json");

            var response = _httpClient.Get(imdbRequest);

            var imdbCallback = "imdb$" + searchTerm + "(";

            var responseCleaned = response.Content.Replace(imdbCallback, "").TrimEnd(")");

            _logger.Warn("Cleaned response: " + responseCleaned);

            ImdbResource json =Json Convert.DeserializeObject<ImdbResource>(responseCleaned);

            _logger.Warn("Json object: " + json);

            _logger.Warn("Crash ahead.");*/

            var response = _httpClient.Get<MovieSearchRoot>(request);

            var movieResults = response.Resource.results;

            return movieResults.SelectList(MapMovie);
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
                        imdbMovie.InCinemas = DateTime.Parse(result.release_date);
                        imdbMovie.Year = imdbMovie.InCinemas.Value.Year;
                    }

                    if (result.physical_release.IsNotNullOrWhiteSpace())
                    {
                        imdbMovie.PhysicalRelease = DateTime.Parse(result.physical_release);
                        if (result.physical_release_note.IsNotNullOrWhiteSpace())
                        {
                            imdbMovie.PhysicalReleaseNote = result.physical_release_note;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug("Not a valid date time.");
                }



                var now = DateTime.Now;
				//handle the case when we have both theatrical and physical release dates
				if (imdbMovie.InCinemas.HasValue && imdbMovie.PhysicalRelease.HasValue)
				{
					if (now < imdbMovie.InCinemas)
						imdbMovie.Status = MovieStatusType.Announced;
					else if (now >= imdbMovie.InCinemas)
						imdbMovie.Status = MovieStatusType.InCinemas;
					if (now >= imdbMovie.PhysicalRelease)
						imdbMovie.Status = MovieStatusType.Released;
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
				if (!imdbMovie.PhysicalRelease.HasValue && (imdbMovie.Status == MovieStatusType.InCinemas) && (((DateTime.Now).Subtract(imdbMovie.InCinemas.Value)).TotalSeconds > 60 * 60 * 24 * 30 * 3))
				{
					imdbMovie.Status = MovieStatusType.Released;
				}

                imdbMovie.TitleSlug += "-" + imdbMovie.TmdbId;

                imdbMovie.Images = new List<MediaCover.MediaCover>();
                imdbMovie.Overview = result.overview;
                imdbMovie.Ratings = new Ratings { Value = (decimal)result.vote_average, Votes = result.vote_count};

                try
                {
                    var imdbPoster = _configService.GetCoverForURL(result.poster_path, MediaCoverTypes.Poster);
                    imdbMovie.Images.Add(imdbPoster);
                }
                catch (Exception e)
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

        private static Actor MapActors(ActorResource arg)
        {
            var newActor = new Actor
            {
                Name = arg.Name,
                Character = arg.Character
            };

            if (arg.Image != null)
            {
                newActor.Images = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover(MediaCoverTypes.Headshot, arg.Image)
                };
            }

            return newActor;
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
                case "banner":
                    return MediaCoverTypes.Banner;
                case "fanart":
                    return MediaCoverTypes.Fanart;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }

        public Movie MapMovieToTmdbMovie(Movie movie)
        {
			try
			{
				 Movie newMovie = movie;
	            if (movie.TmdbId > 0)
	            {
	                newMovie = GetMovieInfo(movie.TmdbId);
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
