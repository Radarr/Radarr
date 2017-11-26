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
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Music;
using Newtonsoft.Json;
using NzbDrone.Core.Configuration;
using System.Text.RegularExpressions;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideArtistInfo, ISearchForNewArtist
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IArtistService _artistService;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IConfigService _configService;
        private readonly IMetadataProfileService _metadataProfileService;

        private IHttpRequestBuilderFactory customerRequestBuilder;

        public SkyHookProxy(IHttpClient httpClient, ILidarrCloudRequestBuilder requestBuilder, IArtistService artistService, Logger logger, IConfigService configService, IMetadataProfileService metadataProfileService)
        {
            _httpClient = httpClient;
            _configService = configService;
            _metadataProfileService = metadataProfileService;
            _requestBuilder = requestBuilder.Search;
            _artistService = artistService;
            _logger = logger;
        }

        public Tuple<Artist, List<Album>> GetArtistInfo(string foreignArtistId, int metadataProfileId)
        {

            _logger.Debug("Getting Artist with LidarrAPI.MetadataID of {0}", foreignArtistId);

            SetCustomProvider();

            var metadataProfile = _metadataProfileService.Get(metadataProfileId);

            var primaryTypes = metadataProfile.PrimaryAlbumTypes.Where(s => s.Allowed).Select(s => s.PrimaryAlbumType.Name);
            var secondaryTypes = metadataProfile.SecondaryAlbumTypes.Where(s => s.Allowed).Select(s => s.SecondaryAlbumType.Name);

            var httpRequest = customerRequestBuilder.Create()
                                            .SetSegment("route", "artists/" + foreignArtistId)
                                            .AddQueryParam("primTypes", string.Join("|", primaryTypes))
                                            .AddQueryParam("secTypes", string.Join("|", secondaryTypes))
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
                        var metadataProfile = _metadataProfileService.All().First().Id; //Change this to Use last Used profile?
                        return new List<Artist> { GetArtistInfo(slug, metadataProfile).Item1 };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Artist>();
                    }
                }

                SetCustomProvider();

                var httpRequest = customerRequestBuilder.Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "artist")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    .Build();



                var httpResponse = _httpClient.Get<List<ArtistResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearhResult);
            }
            catch (HttpException)
            {
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with LidarrAPI.", title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, ex.Message);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from LidarrAPI.", title);
            }
        }

        private Artist MapSearhResult(ArtistResource resource)
        {
            var artist = _artistService.FindById(resource.Id);

            if (artist == null)
            {
                artist = MapArtist(resource);
            }

            return artist;
        }

        private static Album MapAlbum(AlbumResource resource)
        {
            Album album = new Album();
            album.Title = resource.Title;
            album.ForeignAlbumId = resource.Id;
            album.ReleaseDate = resource.ReleaseDate;
            album.CleanTitle = Parser.Parser.CleanArtistName(album.Title);
            album.AlbumType = resource.Type;
            album.Images = resource.Images.Select(MapImage).ToList();
            album.Label = resource.Labels;

            album.Media = resource.Media.Select(MapMedium).ToList();
            album.Tracks = resource.Tracks.Select(MapTrack).ToList();
            album.SecondaryTypes = resource.SecondaryTypes.Select(MapSecondaryTypes).ToList();


            return album;
        }

        private static Medium MapMedium(MediumResource resource)
        {
            Medium medium = new Medium();
            medium.Name = resource.Name;
            medium.Number = resource.Position;
            medium.Format = resource.Format;

            return medium;
        }

        private static Track MapTrack(TrackResource resource)
        {
            Track track = new Track();
            track.Title = resource.TrackName;
            track.ForeignTrackId = resource.Id;
            track.TrackNumber = resource.TrackNumber;
            track.AbsoluteTrackNumber = resource.TrackPosition;
            track.Duration = resource.DurationMs;
            track.MediumNumber = resource.MediumNumber;
            
            return track;
        }

        private static Artist MapArtist(ArtistResource resource)
        {

            Artist artist = new Artist();

            artist.Name = resource.ArtistName;
            artist.ForeignArtistId = resource.Id;
            artist.Genres = resource.Genres;
            artist.Overview = resource.Overview;
            artist.NameSlug = Parser.Parser.CleanArtistName(artist.Name) + "-" + resource.Id.Substring(resource.Id.Length - 6);
            artist.CleanName = Parser.Parser.CleanArtistName(artist.Name);
            artist.SortName = Parser.Parser.NormalizeTitle(artist.Name);
            artist.Disambiguation = resource.Disambiguation;
            artist.ArtistType = resource.Type;
            artist.Images = resource.Images.Select(MapImage).ToList();
            artist.Status = MapArtistStatus(resource.Status);
            artist.Ratings = MapRatings(resource.Rating);
            artist.Links = resource.Links.Select(MapLink).ToList();

            return artist;
        }

        private static Member MapMembers(MemberResource arg)
        {
            var newMember = new Member
            {
                Name = arg.Name,
                Instrument = arg.Instrument
            };

            if (arg.Image != null)
            {
                newMember.Images = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover(MediaCoverTypes.Headshot, arg.Image)
                };
            }

            return newMember;
        }

        private static ArtistStatusType MapArtistStatus(string status)
        {
            if (status == null)
            {
                return ArtistStatusType.Continuing;
            }

            if (status.Equals("ended", StringComparison.InvariantCultureIgnoreCase))
            {
                return ArtistStatusType.Ended;
            }

            return ArtistStatusType.Continuing;
        }

        private static Music.Ratings MapRatings(RatingResource rating)
        {
            if (rating == null)
            {
                return new Music.Ratings();
            }

            return new Music.Ratings
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

        private static Music.Links MapLink(LinkResource arg)
        {
            return new Music.Links
            {
                Url = arg.Target,
                Name = arg.Type
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
                case "logo":
                    return MediaCoverTypes.Logo;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }

        private static SecondaryAlbumType MapSecondaryTypes(string albumType)
        {
            switch (albumType.ToLowerInvariant())
            {
                case "compilation":
                    return SecondaryAlbumType.Compilation;
                case "soundtrack":
                    return SecondaryAlbumType.Soundtrack;
                case "spokenword":
                    return SecondaryAlbumType.Spokenword;
                case "interview":
                    return SecondaryAlbumType.Interview;
                case "audiobook":
                    return SecondaryAlbumType.Audiobook;
                case "live":
                    return SecondaryAlbumType.Live;
                case "remix":
                    return SecondaryAlbumType.Remix;
                case "dj-mix":
                    return SecondaryAlbumType.DJMix;
                case "mixtape/street":
                    return SecondaryAlbumType.Mixtape;
                case "demo":
                    return SecondaryAlbumType.Demo;
                default:
                    return SecondaryAlbumType.Studio;
            }
        }

        private void SetCustomProvider()
        {
            if (_configService.MetadataSource.IsNotNullOrWhiteSpace())
            {
                customerRequestBuilder = new HttpRequestBuilder(_configService.MetadataSource.TrimEnd("/") + "/{route}/").CreateFactory();
            }
            else
            {
                customerRequestBuilder = _requestBuilder;
            }
        }
    }
}
