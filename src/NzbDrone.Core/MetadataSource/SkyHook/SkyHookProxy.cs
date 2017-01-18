using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, ISearchForNewSeries, IProvideMovieInfo, ISearchForNewMovie
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IHttpRequestBuilderFactory _movieBuilder;
        private readonly ITmdbConfigService _configService;
        private readonly IMovieService _movieService;

        public SkyHookProxy(IHttpClient httpClient, ISonarrCloudRequestBuilder requestBuilder, ITmdbConfigService configService, IMovieService movieService, Logger logger)
        {
            _httpClient = httpClient;
             _requestBuilder = requestBuilder.SkyHookTvdb;
            _movieBuilder = requestBuilder.TMDB;
            _configService = configService;
            _movieService = movieService;
            _logger = logger;
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId)
        {
            var httpRequest = _requestBuilder.Create()
                                             .SetSegment("route", "shows")
                                             .Resource(tvdbSeriesId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<ShowResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SeriesNotFoundException(tvdbSeriesId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var episodes = httpResponse.Resource.Episodes.Select(MapEpisode);
            var series = MapSeries(httpResponse.Resource);

            return new Tuple<Series, List<Episode>>(series, episodes.ToList());
        }

        public Movie GetMovieInfo(int TmdbId)
        {
            var request = _movieBuilder.Create()
               .SetSegment("route", "movie")
               .SetSegment("id", TmdbId.ToString())
               .SetSegment("secondaryRoute", "")
               .AddQueryParam("append_to_response", "alternative_titles,release_dates,videos")
               .AddQueryParam("country", "US")
               .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<MovieResourceRoot>(request);

            var resource = response.Resource;

            var movie = new Movie();

            movie.TmdbId = TmdbId;
            movie.ImdbId = resource.imdb_id;
            movie.Title = resource.title;
            movie.TitleSlug = ToUrlSlug(resource.title);
            movie.CleanTitle = Parser.Parser.CleanSeriesTitle(resource.title);
            movie.SortTitle = Parser.Parser.NormalizeTitle(resource.title);
            movie.Overview = resource.overview;
            movie.Website = resource.homepage;
            if (resource.release_date.IsNotNullOrWhiteSpace())
            {
                movie.InCinemas = DateTime.Parse(resource.release_date);
                movie.Year = movie.InCinemas.Value.Year;
            }

            movie.TitleSlug += "-" + movie.Year.ToString();

            movie.Images.Add(_configService.GetCoverForURL(resource.poster_path, MediaCoverTypes.Poster));//TODO: Update to load image specs from tmdb page!
            movie.Images.Add(_configService.GetCoverForURL(resource.backdrop_path, MediaCoverTypes.Banner));
            movie.Runtime = resource.runtime;

            foreach(Title title in resource.alternative_titles.titles)
            {
                movie.AlternativeTitles.Add(title.title);
            }

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
                            }
                        }
                        else
                        {
                            movie.PhysicalRelease = DateTime.Parse(releaseDate.release_date);
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

            if (resource.status == "Released")
            {
                movie.Status = MovieStatusType.Released;
            }
            else
            {
                movie.Status = MovieStatusType.Announced;
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

            return movie;
        }

        public Movie GetMovieInfo(string ImdbId)
        {
            var request = _movieBuilder.Create()
                .SetSegment("route", "find")
                .SetSegment("id", ImdbId)
                .SetSegment("secondaryRoute", "")
                .AddQueryParam("external_source", "imdb_id")
                .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var resources = _httpClient.Get<FindRoot>(request).Resource;

            return resources.movie_results.SelectList(MapMovie).FirstOrDefault();
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

            var parserResult = Parser.Parser.ParseMovieTitle(title, true);

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
                    return new List<Movie> { GetMovieInfo(parserResult.ImdbId) };
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
                catch (SeriesNotFoundException)
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

            ImdbResource json = JsonConvert.DeserializeObject<ImdbResource>(responseCleaned);

            _logger.Warn("Json object: " + json);

            _logger.Warn("Crash ahead.");*/

            var response = _httpClient.Get<MovieSearchRoot>(request);

            var movieResults = response.Resource.results;

            return movieResults.SelectList(MapMovie);
        }

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    int tvdbId;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out tvdbId) || tvdbId <= 0)
                    {
                        return new List<Series>();
                    }

                    try
                    {
                        return new List<Series> { GetSeriesInfo(tvdbId).Item1 };
                    }
                    catch (SeriesNotFoundException)
                    {
                        return new List<Series>();
                    }
                }

               

                var httpRequest = _requestBuilder.Create()
                                                 .SetSegment("route", "search")
                                                 .AddQueryParam("term", title.ToLower().Trim())
                                                 .Build();

                

                var httpResponse = _httpClient.Get<List<ShowResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSeries);
            }
            catch (HttpException)
            {
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook.", title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from SkyHook.", title);
            }
        }

        private Movie MapMovie(MovieResult result)
        {
            var imdbMovie = new Movie();
            imdbMovie.TmdbId = result.id;
            try
            {
                imdbMovie.SortTitle = Parser.Parser.NormalizeTitle(result.title);
                imdbMovie.Title = result.title;
                imdbMovie.TitleSlug = ToUrlSlug(result.title);

                if (result.release_date.IsNotNullOrWhiteSpace())
                {
                    imdbMovie.Year = DateTime.Parse(result.release_date).Year;
                }

                imdbMovie.TitleSlug += "-" + imdbMovie.Year;

                imdbMovie.Images = new List<MediaCover.MediaCover>();
                imdbMovie.Overview = result.overview;
                try
                {
                    var imdbPoster = _configService.GetCoverForURL(result.poster_path, MediaCoverTypes.Poster);
                    imdbMovie.Images.Add(imdbPoster);
                }
                catch (Exception e)
                {
                    _logger.Debug(result);
                }

                return imdbMovie;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error occured while searching for new movies.");
            }

            return null;
        }

        private static Series MapSeries(ShowResource show)
        {
            var series = new Series();
            series.TvdbId = show.TvdbId;

            if (show.TvRageId.HasValue)
            {
                series.TvRageId = show.TvRageId.Value;
            }

            if (show.TvMazeId.HasValue)
            {
                series.TvMazeId = show.TvMazeId.Value;
            }

            series.ImdbId = show.ImdbId;
            series.Title = show.Title;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(show.Title);
            series.SortTitle = SeriesTitleNormalizer.Normalize(show.Title, show.TvdbId);

            if (show.FirstAired != null)
            {
                series.FirstAired = DateTime.Parse(show.FirstAired).ToUniversalTime();
                series.Year = series.FirstAired.Value.Year;
            }

            series.Overview = show.Overview;

            if (show.Runtime != null)
            {
                series.Runtime = show.Runtime.Value;
            }

            series.Network = show.Network;

            if (show.TimeOfDay != null)
            {
                series.AirTime = string.Format("{0:00}:{1:00}", show.TimeOfDay.Hours, show.TimeOfDay.Minutes);
            }

            series.TitleSlug = show.Slug;
            series.Status = MapSeriesStatus(show.Status);
            series.Ratings = MapRatings(show.Rating);
            series.Genres = show.Genres;

            if (show.ContentRating.IsNotNullOrWhiteSpace())
            {
                series.Certification = show.ContentRating.ToUpper();
            }
            
            series.Actors = show.Actors.Select(MapActors).ToList();
            series.Seasons = show.Seasons.Select(MapSeason).ToList();
            series.Images = show.Images.Select(MapImage).ToList();

            return series;
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

        private static Episode MapEpisode(EpisodeResource oracleEpisode)
        {
            var episode = new Episode();
            episode.Overview = oracleEpisode.Overview;
            episode.SeasonNumber = oracleEpisode.SeasonNumber;
            episode.EpisodeNumber = oracleEpisode.EpisodeNumber;
            episode.AbsoluteEpisodeNumber = oracleEpisode.AbsoluteEpisodeNumber;
            episode.Title = oracleEpisode.Title;

            episode.AirDate = oracleEpisode.AirDate;
            episode.AirDateUtc = oracleEpisode.AirDateUtc;

            episode.Ratings = MapRatings(oracleEpisode.Rating);

            //Don't include series fanart images as episode screenshot
            if (oracleEpisode.Image != null)
            {
                episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, oracleEpisode.Image));
            }

            return episode;
        }

        private static Season MapSeason(SeasonResource seasonResource)
        {
            return new Season
            {
                SeasonNumber = seasonResource.SeasonNumber,
                Images = seasonResource.Images.Select(MapImage).ToList()
            };
        }

        private static SeriesStatusType MapSeriesStatus(string status)
        {
            if (status.Equals("ended", StringComparison.InvariantCultureIgnoreCase))
            {
                return SeriesStatusType.Ended;
            }

            return SeriesStatusType.Continuing;
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

        public static string ToUrlSlug(string value)
        {
            //First to lower case
            value = value.ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
            value = Encoding.ASCII.GetString(bytes);

            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);

            //Trim dashes from end
            value = value.Trim('-', '_');

            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }
    }
}
