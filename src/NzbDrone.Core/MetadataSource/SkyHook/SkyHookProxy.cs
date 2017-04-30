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
using NzbDrone.Core.Tv;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Music;
using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, ISearchForNewSeries
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _requestBuilder;

        public SkyHookProxy(IHttpClient httpClient, ILidarrCloudRequestBuilder requestBuilder, Logger logger)
        {
            _httpClient = httpClient;
             _requestBuilder = requestBuilder.Search;
            _logger = logger;
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId)
        {
            Console.WriteLine("[GetSeriesInfo] id:" + tvdbSeriesId);
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

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();
                Console.WriteLine("Searching for " + lowerTitle);

                //if (lowerTitle.StartsWith("tvdb:") || lowerTitle.StartsWith("tvdbid:"))
                //{
                //    var slug = lowerTitle.Split(':')[1].Trim();

                //    int tvdbId;

                //    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out tvdbId) || tvdbId <= 0)
                //    {
                //        return new List<Series>();
                //    }

                //    try
                //    {
                //        return new List<Series> { GetSeriesInfo(tvdbId).Item1 };
                //    }
                //    catch (SeriesNotFoundException)
                //    {
                //        return new List<Series>();
                //    }
                //}

                // Majora: Temporarily, use iTunes to test.
                var httpRequest = _requestBuilder.Create()
                                    .AddQueryParam("entity", "album")
                                    .AddQueryParam("term", title.ToLower().Trim())
                                    .Build();



                Console.WriteLine("httpRequest: ", httpRequest);

                var httpResponse = _httpClient.Get<List<ShowResource>>(httpRequest);

                //Console.WriteLine("Response: ", httpResponse.GetType());
                //_logger.Info("Response: ", httpResponse.Resource.ResultCount);

                //_logger.Info("HTTP Response: ", httpResponse.Resource.ResultCount);
                var tempList = new List<Series>();
                var tempSeries = new Series();
                tempSeries.Title = "AFI";
                tempList.Add(tempSeries);
                return tempList;
                
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

        public List<Artist> SearchForNewArtist(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();
                Console.WriteLine("Searching for " + lowerTitle);

                if (lowerTitle.StartsWith("itunes:") || lowerTitle.StartsWith("itunesid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    int itunesId;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out itunesId) || itunesId <= 0)
                    {
                        return new List<Artist>();
                    }

                    //try
                    //{
                    //    return new List<Artist> { GetArtistInfo(itunesId).Item1 };
                    //}
                    //catch (ArtistNotFoundException)
                    //{
                    //    return new List<Artist>();
                    //}
                }

                var httpRequest = _requestBuilder.Create()
                                    .AddQueryParam("entity", "album")
                                    .AddQueryParam("term", title.ToLower().Trim())
                                    .Build();



                Console.WriteLine("httpRequest: ", httpRequest);

                var httpResponse = _httpClient.Get<ArtistResource>(httpRequest);


                //Console.WriteLine("Response: ", httpResponse.GetType());
                //_logger.Info("Response: ", httpResponse.Resource.ResultCount);

                //_logger.Info("HTTP Response: ", httpResponse.Resource.ResultCount);
                //var tempList = new List<Artist>();
                //var tempSeries = new Artist();
                //tempSeries.ArtistName = "AFI";
                //tempList.Add(tempSeries);
                //return tempList;


                Album tempAlbum;
                // TODO: This needs to handle multiple artists.
                Artist artist = new Artist();
                artist.ArtistName = httpResponse.Resource.Results[0].ArtistName;
                artist.ItunesId = httpResponse.Resource.Results[0].ArtistId;
                foreach (var album in httpResponse.Resource.Results)
                {
                    tempAlbum = new Album();
                    tempAlbum.AlbumId = album.CollectionId;
                    tempAlbum.Title = album.CollectionName;

                    artist.Albums.Add(tempAlbum);
                }

                var temp = new List<Artist>();
                temp.Add(artist);
                return temp;

                // I need to return a list of mapped artists. 

                //return httpResponse.Resource.Results.SelectList(MapArtist);
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

        private static Artist MapArtist(ArtistResource artistQuery)
        {
            var artist = new Artist();
            //artist.ItunesId = artistQuery.ItunesId; ;

           // artist.ArtistName = artistQuery.ArtistName;


            return artist;
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
            series.Monitored = true;

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
                Images = seasonResource.Images.Select(MapImage).ToList(),
                Monitored = seasonResource.SeasonNumber > 0
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
    }
}
