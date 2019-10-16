using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifySavedAlbumsSettings : SpotifySettingsBase<SpotifySavedAlbumsSettings>
    {
        public override string Scope => "user-library-read";
    }

    public class SpotifySavedAlbums : SpotifyImportListBase<SpotifySavedAlbumsSettings>
    {
        public SpotifySavedAlbums(ISpotifyProxy spotifyProxy,
                                  IMetadataRequestBuilder requestBuilder,
                                  IImportListStatusService importListStatusService,
                                  IImportListRepository importListRepository,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  IHttpClient httpClient,
                                  Logger logger)
        : base(spotifyProxy, requestBuilder, importListStatusService, importListRepository, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "Spotify Saved Albums";

        public override IList<SpotifyImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            var result = new List<SpotifyImportListItemInfo>();

            var savedAlbums = _spotifyProxy.GetSavedAlbums(this, api);

            _logger.Trace($"Got {savedAlbums?.Total ?? 0} saved albums");

            while (true)
            {
                if (savedAlbums?.Items == null)
                {
                    return result;
                }

                foreach (var savedAlbum in savedAlbums.Items)
                {
                    result.AddIfNotNull(ParseSavedAlbum(savedAlbum));
                }

                if (!savedAlbums.HasNextPage())
                {
                    break;
                }

                savedAlbums = _spotifyProxy.GetNextPage(this, api, savedAlbums);
            }

            return result;
        }

        private SpotifyImportListItemInfo ParseSavedAlbum(SavedAlbum savedAlbum)
        {
            var artistName = savedAlbum?.Album?.Artists?.FirstOrDefault()?.Name;
            var albumName = savedAlbum?.Album?.Name;
            _logger.Trace($"Adding {artistName} - {albumName}");

            if (artistName.IsNotNullOrWhiteSpace() && albumName.IsNotNullOrWhiteSpace())
            {
                return new SpotifyImportListItemInfo
                {
                    Artist = artistName,
                    Album = albumName,
                    AlbumSpotifyId = savedAlbum?.Album?.Id,
                    ReleaseDate = ParseSpotifyDate(savedAlbum?.Album?.ReleaseDate, savedAlbum?.Album?.ReleaseDatePrecision)
                };
            }

            return null;
        }
    }
}
