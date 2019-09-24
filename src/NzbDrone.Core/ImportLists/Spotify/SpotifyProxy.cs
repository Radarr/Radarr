using System;
using NLog;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public interface ISpotifyProxy
    {
        PrivateProfile GetPrivateProfile<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        Paging<SimplePlaylist> GetUserPlaylists<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, string id)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        FollowedArtists GetFollowedArtists<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        Paging<SavedAlbum> GetSavedAlbums<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        Paging<PlaylistTrack> GetPlaylistTracks<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, string id, string fields)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        Paging<T> GetNextPage<T, TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, Paging<T> item)
            where TSettings : SpotifySettingsBase<TSettings>, new();
        FollowedArtists GetNextPage<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, FollowedArtists item)
            where TSettings : SpotifySettingsBase<TSettings>, new();
    }

    public class SpotifyProxy : ISpotifyProxy
    {
        private readonly Logger _logger;

        public SpotifyProxy(Logger logger)
        {
            _logger = logger;
        }

        public PrivateProfile GetPrivateProfile<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, x => x.GetPrivateProfile());
        }

        public Paging<SimplePlaylist> GetUserPlaylists<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, string id)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, x => x.GetUserPlaylists(id));
        }

        public FollowedArtists GetFollowedArtists<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, x => x.GetFollowedArtists(FollowType.Artist, 50));
        }

        public Paging<SavedAlbum> GetSavedAlbums<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, x => x.GetSavedAlbums(50));
        }

        public Paging<PlaylistTrack> GetPlaylistTracks<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, string id, string fields)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, x => x.GetPlaylistTracks(id, fields: fields));
        }

        public Paging<T> GetNextPage<T, TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, Paging<T> item)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, (x) => x.GetNextPage(item));
        }

        public FollowedArtists GetNextPage<TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, FollowedArtists item)
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            return Execute(list, api, (x) => x.GetNextPage<FollowedArtists, FullArtist>(item.Artists));
        }

        public T Execute<T, TSettings>(SpotifyImportListBase<TSettings> list, SpotifyWebAPI api, Func<SpotifyWebAPI, T> method, bool allowReauth = true)
            where T : BasicModel
            where TSettings : SpotifySettingsBase<TSettings>, new()
        {
            T result = method(api);
            if (result.HasError())
            {
                // If unauthorized, refresh token and try again
                if (result.Error.Status == 401)
                {
                    if (allowReauth)
                    {
                        _logger.Debug("Spotify authorization error, refreshing token and retrying");
                        list.RefreshToken();
                        api.AccessToken = list.AccessToken;
                        return Execute(list, api, method, false);
                    }
                    else
                    {
                        throw new SpotifyAuthorizationException(result.Error.Message);
                    }
                }
                else
                {
                    throw new SpotifyException("[{0}] {1}", result.Error.Status, result.Error.Message);
                }
            }

            return result;
        }
    }
}
