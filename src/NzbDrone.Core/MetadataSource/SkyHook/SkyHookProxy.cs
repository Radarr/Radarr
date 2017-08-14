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
            // TODO: Remove this API
            var tempList = new List<Series>();
            var tempSeries = new Series();
            tempSeries.Title = "AFI";
            tempList.Add(tempSeries);
            return tempList;
        }


        public Tuple<Artist, List<Album>> GetArtistInfo(string foreignArtistId)
        {

            _logger.Debug("Getting Artist with LidarrAPI.MetadataID of {0}", foreignArtistId);

            // We need to perform a direct lookup of the artist
            var httpRequest = _requestBuilder.Create()
                                            .SetSegment("route", "artists/" + foreignArtistId)
                                            .Build();

            

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<ArtistResource>(httpRequest);
            

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ArtistNotFoundException(foreignArtistId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var albums = httpResponse.Resource.Albums.Select(MapAlbum);
            var artist = MapArtist(httpResponse.Resource);

            return new Tuple<Artist, List<Album>>(artist, albums.ToList());
        }

        public List<Artist> SearchForNewArtist(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();
                Console.WriteLine("Searching for " + lowerTitle);

                if (lowerTitle.StartsWith("lidarr:") || lowerTitle.StartsWith("lidarrid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                    {
                        return new List<Artist>();
                    }

                    try
                    {
                        return new List<Artist> { GetArtistInfo(slug).Item1 };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Artist>();
                    }
                }

                var httpRequest = _requestBuilder.Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "artist")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    .Build();



                var httpResponse = _httpClient.Get<List<ArtistResource>>(httpRequest);
                
                return httpResponse.Resource.SelectList(MapArtist);
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

        private static Album MapAlbum(AlbumResource resource)
        {
            Album album = new Album();
            album.Title = resource.Title;
            album.ForeignAlbumId = resource.Id;
            album.ReleaseDate = resource.ReleaseDate;
            album.CleanTitle = Parser.Parser.CleanArtistTitle(album.Title);
            album.AlbumType = resource.Type;
            album.Images = resource.Images.Select(MapImage).ToList();

            var tracks = resource.Tracks.Select(MapTrack);
            album.Tracks = tracks.ToList();
            

            return album;
        }

        private static Track MapTrack(TrackResource resource)
        {
            Track track = new Track();
            track.Title = resource.TrackName;
            track.ForeignTrackId = resource.Id;
            track.TrackNumber = resource.TrackNumber;
            track.Duration = resource.DurationMs;
            return track;
        }

        private static Artist MapArtist(ArtistResource resource)
        {
            
            Artist artist = new Artist();

            artist.Name = resource.ArtistName;
            artist.ForeignArtistId = resource.Id;
            artist.Genres = resource.Genres;
            artist.Overview = resource.Overview;
            artist.NameSlug = Parser.Parser.CleanArtistTitle(artist.Name);
            artist.CleanName = Parser.Parser.CleanArtistTitle(artist.Name);
            artist.SortName = SeriesTitleNormalizer.Normalize(artist.Name,0);
            artist.Images = resource.Images.Select(MapImage).ToList();

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
            //series.Ratings = MapRatings(show.Rating);
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

            //episode.Ratings = MapRatings(oracleEpisode.Rating);

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

        private static Core.Music.Ratings MapRatings(RatingResource rating)
        {
            if (rating == null)
            {
                return new Core.Music.Ratings();
            }

            return new Core.Music.Ratings
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
                case "cover":
                    return MediaCoverTypes.Cover;
                case "disc":
                    return MediaCoverTypes.Disc;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }
    }
}
