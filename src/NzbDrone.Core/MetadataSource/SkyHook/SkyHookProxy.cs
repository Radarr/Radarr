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
        private readonly IMetadataRequestBuilder _requestBuilder;
        private readonly IConfigService _configService;
        private readonly IMetadataProfileService _metadataProfileService;

        private static readonly List<string> nonAudioMedia = new List<string> { "DVD", "DVD-Video", "Blu-ray", "HD-DVD", "VCD", "SVCD", "UMD", "VHS" };
        private static readonly List<string> skippedTracks = new List<string> { "[data track]" };

        public SkyHookProxy(IHttpClient httpClient,
                            IMetadataRequestBuilder requestBuilder,
                            IArtistService artistService,
                            IAlbumService albumService,
                            Logger logger,
                            IConfigService configService,
                            IMetadataProfileService metadataProfileService)
        {
            _httpClient = httpClient;
            _configService = configService;
            _metadataProfileService = metadataProfileService;
            _requestBuilder = requestBuilder;
            _artistService = artistService;
            _albumService = albumService;
            _logger = logger;
        }

        public Artist GetArtistInfo(string foreignArtistId, int metadataProfileId)
        {

            _logger.Debug("Getting Artist with LidarrAPI.MetadataID of {0}", foreignArtistId);

            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                                             .SetSegment("route", "artist/" + foreignArtistId)
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

            var artist = new Artist();
            artist.Metadata = MapArtistMetadata(httpResponse.Resource);
            artist.CleanName = Parser.Parser.CleanArtistName(artist.Metadata.Value.Name);
            artist.SortName = Parser.Parser.NormalizeTitle(artist.Metadata.Value.Name);

            artist.Albums = FilterAlbums(httpResponse.Resource.Albums, metadataProfileId)
                .Select(x => MapAlbum(x, null)).ToList();

            return artist;
        }

        public IEnumerable<AlbumResource> FilterAlbums(IEnumerable<AlbumResource> albums, int metadataProfileId)
        {
            var metadataProfile = _metadataProfileService.Exists(metadataProfileId) ? _metadataProfileService.Get(metadataProfileId) : _metadataProfileService.All().First();
            var primaryTypes = new HashSet<string>(metadataProfile.PrimaryAlbumTypes.Where(s => s.Allowed).Select(s => s.PrimaryAlbumType.Name));
            var secondaryTypes = new HashSet<string>(metadataProfile.SecondaryAlbumTypes.Where(s => s.Allowed).Select(s => s.SecondaryAlbumType.Name));
            var releaseStatuses = new HashSet<string>(metadataProfile.ReleaseStatuses.Where(s => s.Allowed).Select(s => s.ReleaseStatus.Name));


            return albums.Where(album => primaryTypes.Contains(album.Type) &&
                                (!album.SecondaryTypes.Any() && secondaryTypes.Contains("Studio") ||
                                 album.SecondaryTypes.Any(x => secondaryTypes.Contains(x))) &&
                                album.ReleaseStatuses.Any(x => releaseStatuses.Contains(x)));
        }

        public Tuple<string, Album, List<ArtistMetadata>> GetAlbumInfo(string foreignAlbumId)
        {
            _logger.Debug("Getting Album with LidarrAPI.MetadataID of {0}", foreignAlbumId);
            
            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                .SetSegment("route", "album/" + foreignAlbumId)
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

            var artists = httpResponse.Resource.Artists.Select(MapArtistMetadata).ToList();
            var artistDict = artists.ToDictionary(x => x.ForeignArtistId, x => x);
            var album = MapAlbum(httpResponse.Resource, artistDict);
            album.ArtistMetadata = artistDict[httpResponse.Resource.ArtistId];

            return new Tuple<string, Album, List<ArtistMetadata>>(httpResponse.Resource.ArtistId, album, artists);
        }

        public List<Artist> SearchForNewArtist(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("lidarr:") || lowerTitle.StartsWith("lidarrid:") || lowerTitle.StartsWith("mbid:"))
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
                        var existingArtist = _artistService.FindById(searchGuid.ToString());
                        if (existingArtist != null)
                        {
                            return new List<Artist> { existingArtist };
                        }

                        var metadataProfile = _metadataProfileService.All().First().Id; //Change this to Use last Used profile?

                        return new List<Artist> { GetArtistInfo(searchGuid.ToString(), metadataProfile) };
                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Artist>();
                    }
                }

                var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "artist")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    .Build();



                var httpResponse = _httpClient.Get<List<ArtistResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearchResult);
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

                if (lowerTitle.StartsWith("lidarr:") || lowerTitle.StartsWith("lidarrid:") || lowerTitle.StartsWith("mbid:"))
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
                        var existingAlbum = _albumService.FindById(searchGuid.ToString());

                        if (existingAlbum == null)
                        {
                            var data = GetAlbumInfo(searchGuid.ToString());
                            var album = data.Item2;
                            album.Artist = _artistService.FindById(data.Item1) ?? new Artist {
                                Metadata = data.Item3.Single(x => x.ForeignArtistId == data.Item1)
                            };

                            return new List<Album> { album };
                        }

                        existingAlbum.Artist = _artistService.GetArtist(existingAlbum.ArtistId);
                        return new List<Album>{existingAlbum};

                    }
                    catch (ArtistNotFoundException)
                    {
                        return new List<Album>();
                    }
                }

                var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                                    .SetSegment("route", "search")
                                    .AddQueryParam("type", "album")
                                    .AddQueryParam("query", title.ToLower().Trim())
                                    .AddQueryParam("artist", artist.IsNotNullOrWhiteSpace() ? artist.ToLower().Trim() : string.Empty)
                                    .Build();



                var httpResponse = _httpClient.Get<List<AlbumResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearchResult);
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

        private Artist MapSearchResult(ArtistResource resource)
        {
            var artist = _artistService.FindById(resource.Id);
            if (artist == null)
            {
                artist = new Artist();
                artist.Metadata = MapArtistMetadata(resource);
            }

            return artist;
        }

        private Album MapSearchResult(AlbumResource resource)
        {
            var album = _albumService.FindById(resource.Id) ?? MapAlbum(resource, null);

            var artist = _artistService.FindById(resource.ArtistId);
            if (artist == null)
            {
                artist = new Artist();
                artist.Metadata = MapArtistMetadata(resource.Artists.Single(x => x.Id == resource.ArtistId));
            }
            album.Artist = artist;
            album.ArtistMetadata = artist.Metadata;

            return album;
        }

        private static Album MapAlbum(AlbumResource resource, Dictionary<string, ArtistMetadata> artistDict)
        {
            Album album = new Album();
            album.ForeignAlbumId = resource.Id;
            album.OldForeignAlbumIds = resource.OldIds;
            album.Title = resource.Title;
            album.Overview = resource.Overview;
            album.Disambiguation = resource.Disambiguation;
            album.ReleaseDate = resource.ReleaseDate;

            if (resource.Images != null)
            {
                album.Images = resource.Images.Select(MapImage).ToList();
            }

            album.AlbumType = resource.Type;
            album.SecondaryTypes = resource.SecondaryTypes.Select(MapSecondaryTypes).ToList();
            album.Ratings = MapRatings(resource.Rating);
            album.Links = resource.Links?.Select(MapLink).ToList();
            album.Genres = resource.Genres;
            album.CleanTitle = Parser.Parser.CleanArtistName(album.Title);

            if (resource.Releases != null)
            {
                album.AlbumReleases = resource.Releases.Select(x => MapRelease(x, artistDict)).Where(x => x.TrackCount > 0).ToList();
            }

            album.AnyReleaseOk = true;

            return album;
        }

        private static AlbumRelease MapRelease(ReleaseResource resource, Dictionary<string, ArtistMetadata> artistDict)
        {
            AlbumRelease release = new AlbumRelease();
            release.ForeignReleaseId = resource.Id;
            release.OldForeignReleaseIds = resource.OldIds;
            release.Title = resource.Title;
            release.Status = resource.Status;
            release.Label = resource.Label;
            release.Disambiguation = resource.Disambiguation;
            release.Country = resource.Country;
            release.ReleaseDate = resource.ReleaseDate;

            // Get the complete set of media/tracks returned by the API, adding missing media if necessary
            var allMedia = resource.Media.Select(MapMedium).ToList();
            var allTracks = resource.Tracks.Select(x => MapTrack(x, artistDict));
            if (!allMedia.Any())
            {
                foreach(int n in allTracks.Select(x => x.MediumNumber).Distinct())
                {
                    allMedia.Add(new Medium { Name = "Unknown", Number = n, Format = "Unknown" });
                }
            }

            // Skip non-audio media
            var audioMediaNumbers = allMedia.Where(x => !nonAudioMedia.Contains(x.Format)).Select(x => x.Number);

            // Get tracks on the audio media and omit any that are skipped
            release.Tracks = allTracks.Where(x => audioMediaNumbers.Contains(x.MediumNumber) && !skippedTracks.Contains(x.Title)).ToList();
            release.TrackCount = release.Tracks.Value.Count;

            // Only include the media that contain the tracks we have selected
            var usedMediaNumbers = release.Tracks.Value.Select(track => track.MediumNumber);
            release.Media = allMedia.Where(medium => usedMediaNumbers.Contains(medium.Number)).ToList();
            
            release.Duration = release.Tracks.Value.Sum(x => x.Duration);

            return release;
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

        private static Track MapTrack(TrackResource resource, Dictionary<string, ArtistMetadata> artistDict)
        {
            Track track = new Track
            {
                ArtistMetadata = artistDict[resource.ArtistId],
                Title = resource.TrackName,
                ForeignTrackId = resource.Id,
                OldForeignTrackIds = resource.OldIds,
                ForeignRecordingId = resource.RecordingId,
                OldForeignRecordingIds = resource.OldRecordingIds,
                TrackNumber = resource.TrackNumber,
                AbsoluteTrackNumber = resource.TrackPosition,
                Duration = resource.DurationMs,
                MediumNumber = resource.MediumNumber
            };

            return track;
        }

        private static ArtistMetadata MapArtistMetadata(ArtistResource resource)
        {

            ArtistMetadata artist = new ArtistMetadata();

            artist.Name = resource.ArtistName;
            artist.Aliases = resource.ArtistAliases;
            artist.ForeignArtistId = resource.Id;
            artist.OldForeignArtistIds = resource.OldIds;
            artist.Genres = resource.Genres;
            artist.Overview = resource.Overview;
            artist.Disambiguation = resource.Disambiguation;
            artist.Type = resource.Type;
            artist.Status = MapArtistStatus(resource.Status);
            artist.Ratings = MapRatings(resource.Rating);
            artist.Images = resource.Images?.Select(MapImage).ToList();
            artist.Links = resource.Links?.Select(MapLink).ToList();
            return artist;
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

        public static SecondaryAlbumType MapSecondaryTypes(string albumType)
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
    }
}
