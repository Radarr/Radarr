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
using NzbDrone.Core.Music;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideArtistInfo, ISearchForNewArtist, IProvideAlbumInfo, ISearchForNewAlbum
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IConfigService _configService;
        private readonly IMetadataProfileService _metadataProfileService;

        private IHttpRequestBuilderFactory _customerRequestBuilder;

        public SkyHookProxy(IHttpClient httpClient,
                            ILidarrCloudRequestBuilder requestBuilder,
                            IArtistService artistService,
                            IAlbumService albumService,
                            Logger logger,
                            IConfigService configService,
                            IMetadataProfileService metadataProfileService)
        {
            _httpClient = httpClient;
            _configService = configService;
            _metadataProfileService = metadataProfileService;
            _requestBuilder = requestBuilder.Search;
            _artistService = artistService;
            _albumService = albumService;
            _logger = logger;
        }

        public Tuple<Artist, List<Album>> GetArtistInfo(string foreignArtistId, int metadataProfileId)
        {

            _logger.Debug("Getting Artist with LidarrAPI.MetadataID of {0}", foreignArtistId);

            SetCustomProvider();

            var metadataProfile = _metadataProfileService.Exists(metadataProfileId) ? _metadataProfileService.Get(metadataProfileId) : _metadataProfileService.All().First();

            var primaryTypes = metadataProfile.PrimaryAlbumTypes.Where(s => s.Allowed).Select(s => s.PrimaryAlbumType.Name);
            var secondaryTypes = metadataProfile.SecondaryAlbumTypes.Where(s => s.Allowed).Select(s => s.SecondaryAlbumType.Name);
            var releaseStatuses = metadataProfile.ReleaseStatuses.Where(s => s.Allowed).Select(s => s.ReleaseStatus.Name);

            var httpRequest = _customerRequestBuilder.Create()
                                            .SetSegment("route", "artist/" + foreignArtistId)
                                            .AddQueryParam("primTypes", string.Join("|", primaryTypes))
                                            .AddQueryParam("secTypes", string.Join("|", secondaryTypes))
                                            .AddQueryParam("releaseStatuses", string.Join("|", releaseStatuses))
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
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignArtistId);
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

        public Tuple<Album, List<Track>> GetAlbumInfo(string foreignAlbumId, string releaseId)
        {
            _logger.Debug("Getting Album with LidarrAPI.MetadataID of {0}", foreignAlbumId);

            SetCustomProvider();

            var httpRequest = _customerRequestBuilder.Create()
                .SetSegment("route", "album/" + foreignAlbumId)
                .AddQueryParam("release", releaseId ?? string.Empty)
                .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<AlbumResource>(httpRequest);


            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AlbumNotFoundException(foreignAlbumId);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(foreignAlbumId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var tracks = httpResponse.Resource.Tracks.Select(MapTrack);
            var album = MapAlbum(httpResponse.Resource);

            return new Tuple<Album, List<Track>>(album, tracks.ToList());
        }

        public List<Artist> SearchForNewArtist(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("lidarr:") || lowerTitle.StartsWith("lidarrid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    Guid searchGuid;

                    bool isValid = Guid.TryParse(slug, out searchGuid);

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || isValid == false)
                    {
                        return new List<Artist>();
                    }

                    try
                    {
                        var metadataProfile = _metadataProfileService.All().First().Id; //Change this to Use last Used profile?
                        return new List<Artist> { GetArtistInfo(searchGuid.ToString(), metadataProfile).Item1 };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Artist>();
                    }
                }

                SetCustomProvider();

                var httpRequest = _customerRequestBuilder.Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "artist")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    //.AddQueryParam("images","false") // Should pass these on import search to avoid looking to fanart and wiki 
                                    //.AddQueryParam("overview","false")
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

        public List<Album> SearchForNewAlbum(string title, string artist)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("lidarr:") || lowerTitle.StartsWith("lidarrid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    Guid searchGuid;

                    bool isValid = Guid.TryParse(slug, out searchGuid);

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || isValid == false)
                    {
                        return new List<Album>();
                    }

                    try
                    {
                        return new List<Album> { GetAlbumInfo(searchGuid.ToString(), null).Item1 };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Album>();
                    }
                }

                SetCustomProvider();

                var httpRequest = _customerRequestBuilder.Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "album")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    .AddQueryParam("artist", artist.IsNotNullOrWhiteSpace() ? artist.ToLower().Trim() : string.Empty)
                                    .Build();



                var httpResponse = _httpClient.Get<List<AlbumResource>>(httpRequest);

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
            var artist = _artistService.FindById(resource.Id) ?? MapArtist(resource);

            return artist;
        }

        private Album MapSearhResult(AlbumResource resource)
        {
            var album = _albumService.FindById(resource.Id) ?? MapAlbum(resource);

            return album;
        }

        private static Album MapAlbum(AlbumResource resource)
        {
            Album album = new Album();
            album.Title = resource.Title;
            album.ForeignAlbumId = resource.Id;
            album.ReleaseDate = resource.ReleaseDate;
            album.CleanTitle = Parser.Parser.CleanArtistName(album.Title);
            album.Ratings = MapRatings(resource.Rating);
            album.AlbumType = resource.Type;

            if (resource.Images != null)
            {
                album.Images = resource.Images.Select(MapImage).ToList();
            }

            album.Label = resource.Labels;
            album.Media = resource.Media.Select(MapMedium).ToList();
            album.SecondaryTypes = resource.SecondaryTypes.Select(MapSecondaryTypes).ToList();

            if (resource.Releases != null)
            {
                album.Releases = resource.Releases.Select(MapAlbumRelease).ToList();
                album.CurrentRelease = album.Releases.FirstOrDefault(s => s.Id == resource.SelectedRelease);
            }

            if (resource.Artist != null)
            {
                album.Artist = new Artist
                {
                    ForeignArtistId = resource.Artist.Id,
                    Name = resource.Artist.Name
                };
            }
            

            return album;
        }

        private static Medium MapMedium(MediumResource resource)
        {
            Medium medium = new Medium
            {
                Name = resource.Name,
                Number = resource.Position,
                Format = resource.Format
            };

            return medium;
        }

        private static AlbumRelease MapAlbumRelease(ReleaseResource resource)
        {
            AlbumRelease albumRelease = new AlbumRelease
            {
                Id = resource.Id,
                Title = resource.Title,
                ReleaseDate = resource.ReleaseDate,
                TrackCount = resource.TrackCount,
                Format = resource.Format,
                MediaCount = resource.MediaCount,
                Country = resource.Country,
                Disambiguation = resource.Disambiguation,
                Label = resource.Label
            };
            
            return albumRelease;
        }

        private static Track MapTrack(TrackResource resource)
        {
            Track track = new Track
            {
                Title = resource.TrackName,
                ForeignTrackId = resource.Id,
                TrackNumber = resource.TrackNumber,
                AbsoluteTrackNumber = resource.TrackPosition,
                Duration = resource.DurationMs,
                MediumNumber = resource.MediumNumber
            };

            return track;
        }

        private static Artist MapArtist(ArtistResource resource)
        {

            Artist artist = new Artist();

            artist.Name = resource.ArtistName;
            artist.ForeignArtistId = resource.Id;
            artist.Genres = resource.Genres;
            artist.Overview = resource.Overview;
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

        private static Links MapLink(LinkResource arg)
        {
            return new Links
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
                _customerRequestBuilder = new HttpRequestBuilder(_configService.MetadataSource.TrimEnd("/") + "/{route}").CreateFactory();
            }
            else
            {
                _customerRequestBuilder = _requestBuilder;
            }
        }
    }
}
