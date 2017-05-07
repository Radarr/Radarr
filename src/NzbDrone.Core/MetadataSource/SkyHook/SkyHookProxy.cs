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
    public class SkyHookProxy : IProvideSeriesInfo, IProvideArtistInfo, ISearchForNewSeries
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IHttpRequestBuilderFactory _internalRequestBuilder;

        public SkyHookProxy(IHttpClient httpClient, ILidarrCloudRequestBuilder requestBuilder, Logger logger)
        {
            _httpClient = httpClient;
             _requestBuilder = requestBuilder.Search;
            _internalRequestBuilder = requestBuilder.InternalSearch;
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

        //public Artist GetArtistInfo(int itunesId)
        //{
        //    Console.WriteLine("[GetArtistInfo] id:" + itunesId);
        //    //https://itunes.apple.com/lookup?id=909253
        //    //var httpRequest = _requestBuilder.Create()
        //    //                                 .SetSegment("route", "lookup")
        //    //                                 .AddQueryParam("id", itunesId.ToString())
        //    //                                 .Build();

        //    // TODO: Add special header, add Overview to Artist model
        //    var httpRequest = _requestBuilder.Create()
        //                                     .SetSegment("route", "viewArtist")
        //                                     .AddQueryParam("id", itunesId.ToString())
        //                                     .Build();
        //    httpRequest.Headers.Add("X-Apple-Store-Front", "143459-2,32 t:music3");

        //    httpRequest.AllowAutoRedirect = true;
        //    httpRequest.SuppressHttpError = true;

        //    var httpResponse = _httpClient.Get<ArtistResource>(httpRequest);

        //    if (httpResponse.HasHttpError)
        //    {
        //        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        //        {
        //            throw new ArtistNotFoundException(itunesId);
        //        }
        //        else
        //        {
        //            throw new HttpException(httpRequest, httpResponse);
        //        }
        //    }

        //    Console.WriteLine("GetArtistInfo, GetArtistInfo");
        //    return MapArtists(httpResponse.Resource)[0];
        //}

        public Tuple<Artist, List<Track>> GetArtistInfo(int itunesId)
        {
            // TODO: [GetArtistInfo]: This needs to return a set of tracks from iTunes.
            // This call is expected to return information about an artist and the tracks that make up said artist. 
            // To do this, we need 2-3 API calls. 1st is to gather information about the artist and the albums the artist has. This is https://itunes.apple.com/search?entity=album&id=itunesId
            // Next call is to populate the overview field and calls the internal API
            // Finally, we need to, for each album, get all tracks, which means calling this N times: https://itunes.apple.com/search?entity=musicTrack&term=artistName (id will not work)
            _logger.Debug("Getting Artist with iTunesID of {0}", itunesId);
            var httpRequest1 = _requestBuilder.Create()
                                             .SetSegment("route", "lookup")
                                             .AddQueryParam("id", itunesId.ToString())
                                             .Build();

            var httpRequest2 = _internalRequestBuilder.Create()
                                             .SetSegment("route", "viewArtist")
                                             .AddQueryParam("id", itunesId.ToString())
                                             .Build();
            httpRequest2.Headers.Add("X-Apple-Store-Front", "143459-2,32 t:music3");
            httpRequest2.Headers.ContentType = "application/json";

            httpRequest1.AllowAutoRedirect = true;
            httpRequest1.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<ArtistResource>(httpRequest1);
            

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ArtistNotFoundException(itunesId);
                }
                else
                {
                    throw new HttpException(httpRequest1, httpResponse);
                }
            }

            List<Artist> artists = MapArtists(httpResponse.Resource);
            List<Artist> newArtists = new List<Artist>(artists.Count);
            int count = 0;
            foreach (var artist in artists)
            {
                newArtists.Add(AddOverview(artist));
                count++;
            }

            // I don't know how we are getting tracks from iTunes yet. 
            return new Tuple<Artist, List<Track>>(newArtists[0], new List<Track>());
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

                    try
                    {
                        return new List<Artist> { GetArtistInfo(itunesId).Item1 };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Artist>();
                    }
                }

                var httpRequest = _requestBuilder.Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("entity", "album")
                                    .AddQueryParam("term", title.ToLower().Trim())
                                    .Build();



                var httpResponse = _httpClient.Get<ArtistResource>(httpRequest);


                List<Artist> artists = MapArtists(httpResponse.Resource);
                List<Artist> newArtists = new List<Artist>(artists.Count);
                int count = 0;
                foreach (var artist in artists)
                {
                    newArtists.Add(AddOverview(artist));
                    count++;
                }


                return newArtists;
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

        private Artist AddOverview(Artist artist)
        {
            var httpRequest = _internalRequestBuilder.Create()
                                             .SetSegment("route", "viewArtist")
                                             .AddQueryParam("id", artist.ItunesId.ToString())
                                             .Build();
            httpRequest.Headers.Add("X-Apple-Store-Front", "143459-2,32 t:music3");
            httpRequest.Headers.ContentType = "application/json";
            var httpResponse = _httpClient.Get<ArtistResource>(httpRequest);

            if (!httpResponse.HasHttpError)
            {
                artist.Overview = httpResponse.Resource.StorePlatformData.Artist.Results[artist.ItunesId].artistBio;
            }

            return artist;
        }

        private Artist MapArtistInfo(ArtistInfoResource resource)
        {
            // This expects ArtistInfoResource, thus just need to populate one artist
            Artist artist = new Artist();
            artist.Overview = resource.artistBio;
            artist.ArtistName = resource.name;
            foreach(var genre in resource.genreNames)
            {
                artist.Genres.Add(genre);
            }

            return artist;
        }

        private List<Artist> MapArtists(ArtistResource resource)
        {
            Album tempAlbum;
            List<Artist> artists = new List<Artist>();
            foreach (var album in resource.Results)
            {
                int index = artists.FindIndex(a => a.ItunesId == album.ArtistId);
                tempAlbum = MapAlbum(album);

                if (index >= 0)
                {
                    artists[index].Albums.Add(tempAlbum);
                }
                else
                {
                    Artist tempArtist = new Artist();
                    tempArtist.ItunesId = album.ArtistId;
                    tempArtist.ArtistName = album.ArtistName;
                    tempArtist.Genres.Add(album.PrimaryGenreName);
                    tempArtist.Albums.Add(tempAlbum);
                    artists.Add(tempArtist);
                }

            }

            return artists;
        }

        private Album MapAlbum(AlbumResource albumQuery)
        {
            Album album = new Album();

            album.AlbumId = albumQuery.CollectionId;
            album.Title = albumQuery.CollectionName;
            album.Year = albumQuery.ReleaseDate.Year;
            album.ArtworkUrl = albumQuery.ArtworkUrl100;
            album.Explicitness = albumQuery.CollectionExplicitness;
            return album;
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
