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
        public SpotifyPlaylist(IImportListStatusService importListStatusService,
                               IImportListRepository importListRepository,
                               IConfigService configService,
                               IParsingService parsingService,
                               HttpClient httpClient,
                               Logger logger)
        : base(importListStatusService, importListRepository, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "Spotify Playlists";

        public override IList<ImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            var result = new List<ImportListItemInfo>();

            foreach (var id in Settings.PlaylistIds)
            {
                _logger.Trace($"Processing playlist {id}");

                var playlistTracks = Execute(api, (x) => x.GetPlaylistTracks(id, fields: "next, items(track(name, album(name,artists)))"));
                while (true)
                {
                    foreach (var track in playlistTracks.Items)
                    {
                        var fullTrack = track.Track;
                        // From spotify docs: "Note, a track object may be null. This can happen if a track is no longer available."
                        if (fullTrack != null)
                        {
                            var album = fullTrack.Album?.Name;
                            var artist = fullTrack.Album?.Artists?.FirstOrDefault()?.Name ?? fullTrack.Artists.FirstOrDefault()?.Name;

                            if (album.IsNotNullOrWhiteSpace() && artist.IsNotNullOrWhiteSpace())
                            {
                                result.AddIfNotNull(new ImportListItemInfo
                                                    {
                                                        Artist = artist,
                                                        Album = album,
                                                        ReleaseDate = ParseSpotifyDate(fullTrack.Album.ReleaseDate, fullTrack.Album.ReleaseDatePrecision)
                                                    });
                                
                            }
                        }
                    }
                        
                    if (!playlistTracks.HasNextPage())
                        break;
                    playlistTracks = Execute(api, (x) => x.GetNextPage(playlistTracks));
                }
            }

            return result;
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
                        var profile = Execute(api, (x) => x.GetPrivateProfile());
                        var playlistPage = Execute(api, (x) => x.GetUserPlaylists(profile.Id));
                        _logger.Trace($"Got {playlistPage.Total} playlists");

                        var playlists = new List<SimplePlaylist>(playlistPage.Total);
                        while (true)
                        {
                            playlists.AddRange(playlistPage.Items);

                            if (!playlistPage.HasNextPage())
                                break;
                            playlistPage = Execute(api, (x) => x.GetNextPage(playlistPage));
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
