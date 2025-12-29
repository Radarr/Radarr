using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.MetadataSource.Music
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1075", Justification = "API base URLs are necessarily hardcoded")]
    public class MusicBrainzProxy : IProvideMusicInfo
    {
        private const string BaseUrl = "https://musicbrainz.org/ws/2";
        private const string CoverArtBaseUrl = "https://coverartarchive.org";
        private const string UserAgent = "Aletheia/1.0 (https://github.com/cheir-mneme/aletheia)";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MusicBrainzProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public ArtistMetadata GetArtistById(string musicBrainzId)
        {
            try
            {
                var request = BuildRequest($"/artist/{musicBrainzId}", "releases+url-rels+tags+genres");
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                return ParseArtist(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching artist {0} from MusicBrainz", musicBrainzId);
                return null;
            }
        }

        public ArtistMetadata GetArtistByName(string name)
        {
            var results = SearchArtists(name);
            return results.FirstOrDefault();
        }

        public List<ArtistMetadata> SearchArtists(string query)
        {
            try
            {
                var request = BuildSearchRequest("artist", query);
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<ArtistMetadata>();
                }

                var artists = response["artists"] as JArray;
                if (artists == null)
                {
                    return new List<ArtistMetadata>();
                }

                return artists.Select(ParseArtistSearchResult).Where(a => a != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching artists for '{0}'", query);
                return new List<ArtistMetadata>();
            }
        }

        public AlbumMetadata GetAlbumById(string musicBrainzId)
        {
            try
            {
                var request = BuildRequest($"/release-group/{musicBrainzId}", "artists+releases+tags+genres");
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                var album = ParseReleaseGroup(response);
                album.CoverUrl = GetCoverArtUrl(musicBrainzId);
                return album;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching album {0} from MusicBrainz", musicBrainzId);
                return null;
            }
        }

        public List<AlbumMetadata> GetAlbumsByArtist(string artistMusicBrainzId)
        {
            try
            {
                var request = BuildRequestBuilder($"/release-group")
                    .AddQueryParam("artist", artistMusicBrainzId)
                    .AddQueryParam("limit", "100")
                    .Build();

                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<AlbumMetadata>();
                }

                var releaseGroups = response["release-groups"] as JArray;
                if (releaseGroups == null)
                {
                    return new List<AlbumMetadata>();
                }

                return releaseGroups.Select(ParseReleaseGroup).Where(a => a != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching albums for artist {0}", artistMusicBrainzId);
                return new List<AlbumMetadata>();
            }
        }

        public List<AlbumMetadata> SearchAlbums(string query)
        {
            try
            {
                var request = BuildSearchRequest("release-group", query);
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<AlbumMetadata>();
                }

                var releaseGroups = response["release-groups"] as JArray;
                if (releaseGroups == null)
                {
                    return new List<AlbumMetadata>();
                }

                return releaseGroups.Select(ParseReleaseGroup).Where(a => a != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching albums for '{0}'", query);
                return new List<AlbumMetadata>();
            }
        }

        public TrackMetadata GetTrackById(string musicBrainzId)
        {
            try
            {
                var request = BuildRequest($"/recording/{musicBrainzId}", "artists+releases");
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return null;
                }

                return ParseRecording(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching track {0} from MusicBrainz", musicBrainzId);
                return null;
            }
        }

        public List<TrackMetadata> GetTracksByAlbum(string albumMusicBrainzId)
        {
            try
            {
                var request = BuildRequest($"/release-group/{albumMusicBrainzId}", "releases+media+recordings");
                var response = ExecuteRequest(request);

                if (response == null)
                {
                    return new List<TrackMetadata>();
                }

                var releases = response["releases"] as JArray;
                if (releases == null || releases.Count == 0)
                {
                    return new List<TrackMetadata>();
                }

                var firstRelease = releases[0];
                var releaseId = firstRelease["id"]?.ToString();
                if (string.IsNullOrEmpty(releaseId))
                {
                    return new List<TrackMetadata>();
                }

                var releaseRequest = BuildRequest($"/release/{releaseId}", "recordings+artist-credits");
                var releaseResponse = ExecuteRequest(releaseRequest);

                if (releaseResponse == null)
                {
                    return new List<TrackMetadata>();
                }

                return ParseTracksFromRelease(releaseResponse, albumMusicBrainzId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching tracks for album {0}", albumMusicBrainzId);
                return new List<TrackMetadata>();
            }
        }

        private static HttpRequest BuildRequest(string path, string includes)
        {
            var builder = new HttpRequestBuilder($"{BaseUrl}{path}")
                .AddQueryParam("fmt", "json")
                .SetHeader("User-Agent", UserAgent)
                .SetHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(includes))
            {
                builder.AddQueryParam("inc", includes);
            }

            return builder.Build();
        }

        private static HttpRequestBuilder BuildRequestBuilder(string path)
        {
            return new HttpRequestBuilder($"{BaseUrl}{path}")
                .AddQueryParam("fmt", "json")
                .SetHeader("User-Agent", UserAgent)
                .SetHeader("Accept", "application/json");
        }

        private static HttpRequest BuildSearchRequest(string entity, string query)
        {
            return new HttpRequestBuilder($"{BaseUrl}/{entity}")
                .AddQueryParam("query", query)
                .AddQueryParam("fmt", "json")
                .AddQueryParam("limit", "25")
                .SetHeader("User-Agent", UserAgent)
                .SetHeader("Accept", "application/json")
                .Build();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1168", Justification = "Null indicates not found, distinct from empty response")]
        private JObject ExecuteRequest(HttpRequest request)
        {
            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.HasHttpError)
            {
                return JObject.Parse(response.Content);
            }

            _logger.Warn("MusicBrainz request failed: {0}", response.StatusCode);
            return null;
        }

        private static string GetCoverArtUrl(string releaseGroupId)
        {
            return $"{CoverArtBaseUrl}/release-group/{releaseGroupId}/front-250";
        }

        private static ArtistMetadata ParseArtist(JObject json)
        {
            var artist = new ArtistMetadata
            {
                MusicBrainzId = json["id"]?.ToString(),
                Name = json["name"]?.ToString(),
                SortName = json["sort-name"]?.ToString(),
                Disambiguation = json["disambiguation"]?.ToString(),
                Type = json["type"]?.ToString(),
                Country = json["country"]?.ToString(),
                Genres = new List<string>(),
                Tags = new List<string>(),
                Links = new List<ArtistLink>(),
                Albums = new List<AlbumMetadata>()
            };

            var lifeSpan = json["life-span"];
            if (lifeSpan != null)
            {
                artist.BeginDate = ParseDate(lifeSpan["begin"]?.ToString());
                artist.EndDate = ParseDate(lifeSpan["end"]?.ToString());
                artist.Ended = lifeSpan["ended"]?.Value<bool>() ?? false;
            }

            var genres = json["genres"] as JArray;
            if (genres != null)
            {
                artist.Genres = genres.Select(g => g["name"]?.ToString()).Where(g => !string.IsNullOrEmpty(g)).ToList();
            }

            var tags = json["tags"] as JArray;
            if (tags != null)
            {
                artist.Tags = tags.Select(t => t["name"]?.ToString()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }

            var relations = json["relations"] as JArray;
            if (relations != null)
            {
                artist.Links = relations
                    .Where(r => r["url"] != null)
                    .Select(r => new ArtistLink
                    {
                        Type = r["type"]?.ToString(),
                        Url = r["url"]?["resource"]?.ToString()
                    })
                    .Where(l => !string.IsNullOrEmpty(l.Url))
                    .ToList();
            }

            return artist;
        }

        private static ArtistMetadata ParseArtistSearchResult(JToken json)
        {
            return new ArtistMetadata
            {
                MusicBrainzId = json["id"]?.ToString(),
                Name = json["name"]?.ToString(),
                SortName = json["sort-name"]?.ToString(),
                Disambiguation = json["disambiguation"]?.ToString(),
                Type = json["type"]?.ToString(),
                Country = json["country"]?.ToString(),
                Genres = new List<string>(),
                Tags = new List<string>(),
                Links = new List<ArtistLink>(),
                Albums = new List<AlbumMetadata>()
            };
        }

        private static AlbumMetadata ParseReleaseGroup(JToken json)
        {
            var album = new AlbumMetadata
            {
                MusicBrainzId = json["id"]?.ToString(),
                Title = json["title"]?.ToString(),
                ReleaseType = json["primary-type"]?.ToString(),
                Genres = new List<string>(),
                Tracks = new List<TrackMetadata>()
            };

            album.ReleaseDate = ParseDate(json["first-release-date"]?.ToString());

            var artistCredits = json["artist-credit"] as JArray;
            if (artistCredits != null && artistCredits.Count > 0)
            {
                var primaryArtist = artistCredits[0]["artist"];
                album.ArtistMusicBrainzId = primaryArtist?["id"]?.ToString();
                album.ArtistName = primaryArtist?["name"]?.ToString();
            }

            var genres = json["genres"] as JArray;
            if (genres != null)
            {
                album.Genres = genres.Select(g => g["name"]?.ToString()).Where(g => !string.IsNullOrEmpty(g)).ToList();
            }

            return album;
        }

        private static TrackMetadata ParseRecording(JObject json)
        {
            var track = new TrackMetadata
            {
                MusicBrainzId = json["id"]?.ToString(),
                Title = json["title"]?.ToString(),
                DurationMs = json["length"]?.Value<int>()
            };

            var artistCredits = json["artist-credit"] as JArray;
            if (artistCredits != null && artistCredits.Count > 0)
            {
                var primaryArtist = artistCredits[0]["artist"];
                track.ArtistMusicBrainzId = primaryArtist?["id"]?.ToString();
                track.ArtistName = primaryArtist?["name"]?.ToString();
            }

            return track;
        }

        private static List<TrackMetadata> ParseTracksFromRelease(JObject json, string albumMusicBrainzId)
        {
            var tracks = new List<TrackMetadata>();

            var media = json["media"] as JArray;
            if (media == null)
            {
                return tracks;
            }

            foreach (var disc in media)
            {
                var discNumber = disc["position"]?.Value<int>() ?? 1;
                var tracksArray = disc["tracks"] as JArray;

                if (tracksArray == null)
                {
                    continue;
                }

                foreach (var trackJson in tracksArray)
                {
                    var recording = trackJson["recording"];
                    var track = new TrackMetadata
                    {
                        MusicBrainzId = recording?["id"]?.ToString(),
                        Title = trackJson["title"]?.ToString() ?? recording?["title"]?.ToString(),
                        AlbumMusicBrainzId = albumMusicBrainzId,
                        TrackNumber = trackJson["position"]?.Value<int>() ?? 0,
                        DiscNumber = discNumber,
                        DurationMs = trackJson["length"]?.Value<int>() ?? recording?["length"]?.Value<int>()
                    };

                    var artistCredits = recording?["artist-credit"] as JArray;
                    if (artistCredits != null && artistCredits.Count > 0)
                    {
                        var primaryArtist = artistCredits[0]["artist"];
                        track.ArtistMusicBrainzId = primaryArtist?["id"]?.ToString();
                        track.ArtistName = primaryArtist?["name"]?.ToString();
                    }

                    tracks.Add(track);
                }
            }

            return tracks;
        }

        private static DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
            {
                return null;
            }

            if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            if (dateStr.Length == 4 && int.TryParse(dateStr, out var year))
            {
                return new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
