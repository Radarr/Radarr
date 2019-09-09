using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifyPlaylist : SpotifyImportListBase<SpotifyPlaylistSettings>
    {
        public SpotifyPlaylist(ISpotifyProxy spotifyProxy,
                               IImportListStatusService importListStatusService,
                               IImportListRepository importListRepository,
                               IConfigService configService,
                               IParsingService parsingService,
                               IHttpClient httpClient,
                               Logger logger)
        : base(spotifyProxy, importListStatusService, importListRepository, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "Spotify Playlists";

        public override IList<ImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            return Settings.PlaylistIds.SelectMany(x => Fetch(api, x)).ToList();
        }

        public IList<ImportListItemInfo> Fetch(SpotifyWebAPI api, string playlistId)
        {
            var result = new List<ImportListItemInfo>();

            _logger.Trace($"Processing playlist {playlistId}");

            var playlistTracks = _spotifyProxy.GetPlaylistTracks(this, api, playlistId, "next, items(track(name, album(name,artists)))");

            while (true)
            {
                if (playlistTracks?.Items == null)
                {
                    return result;
                }

                foreach (var playlistTrack in playlistTracks.Items)
                {
                    result.AddIfNotNull(ParsePlaylistTrack(playlistTrack));
                }
                        
                if (!playlistTracks.HasNextPage())
                {
                    break;
                }

                playlistTracks = _spotifyProxy.GetNextPage(this, api, playlistTracks);
            }

            return result;
        }

        private ImportListItemInfo ParsePlaylistTrack(PlaylistTrack playlistTrack)
        {
            // From spotify docs: "Note, a track object may be null. This can happen if a track is no longer available."
            if (playlistTrack?.Track?.Album != null)
            {
                var album = playlistTrack.Track.Album;
                var albumName = album.Name;
                var artistName = album.Artists?.FirstOrDefault()?.Name ?? playlistTrack.Track?.Artists?.FirstOrDefault()?.Name;

                if (albumName.IsNotNullOrWhiteSpace() && artistName.IsNotNullOrWhiteSpace())
                {
                    return new ImportListItemInfo {
                        Artist = artistName,
                        Album = albumName,
                        ReleaseDate = ParseSpotifyDate(album.ReleaseDate, album.ReleaseDatePrecision)
                    };
                }
            }

            return null;
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getPlaylists")
            {
                if (Settings.AccessToken.IsNullOrWhiteSpace())
                {
                    return new
                        {
                            playlists = new List<object>()
                        };
                }

                Settings.Validate().Filter("AccessToken").ThrowOnError();

                using (var api = GetApi())
                {
                    try
                    {
                        var profile = _spotifyProxy.GetPrivateProfile(this, api);
                        var playlistPage = _spotifyProxy.GetUserPlaylists(this, api, profile.Id);
                        _logger.Trace($"Got {playlistPage.Total} playlists");

                        var playlists = new List<SimplePlaylist>(playlistPage.Total);
                        while (true)
                        {
                            if (playlistPage == null)
                            {
                                break;
                            }

                            playlists.AddRange(playlistPage.Items);

                            if (!playlistPage.HasNextPage())
                            {
                                break;
                            }

                            playlistPage = _spotifyProxy.GetNextPage(this, api, playlistPage);
                        }

                        return new
                            {
                                options = new {
                                    user = profile.DisplayName,
                                    playlists = playlists.OrderBy(p => p.Name)
                                    .Select(p => new
                                        {
                                            id = p.Id,
                                            name = p.Name
                                        })
                                }
                            };
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Error fetching playlists from Spotify");
                        return new { };
                    }
                }
            }
            else
            {
                return base.RequestAction(action, query);
            }
        }
    }
}
