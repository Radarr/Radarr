using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using SpotifyAPI.Web;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifySavedAlbumsSettings : SpotifySettingsBase<SpotifySavedAlbumsSettings>
    {
        public override string Scope => "user-library-read";
    }

    public class SpotifySavedAlbums : SpotifyImportListBase<SpotifySavedAlbumsSettings>
    {
        public SpotifySavedAlbums(IImportListStatusService importListStatusService,
                                  IImportListRepository importListRepository,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  HttpClient httpClient,
                                  Logger logger)
        : base(importListStatusService, importListRepository, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "Spotify Saved Albums";

        public override IList<ImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            var result = new List<ImportListItemInfo>();

            var albums = Execute(api, (x) => x.GetSavedAlbums(50));
            _logger.Trace($"Got {albums.Total} saved albums");

            while (true)
            {
                foreach (var album in albums.Items)
                {
                    var artistName = album.Album.Artists.FirstOrDefault()?.Name;
                    var albumName = album.Album.Name;
                    _logger.Trace($"Adding {artistName} - {albumName}");

                    if (artistName.IsNotNullOrWhiteSpace() && albumName.IsNotNullOrWhiteSpace())
                    {
                        result.AddIfNotNull(new ImportListItemInfo
                                            {
                                                Artist = artistName,
                                                Album = albumName
                                            });
                    }
                }
                if (!albums.HasNextPage())
                    break;
                albums = Execute(api, (x) => x.GetNextPage(albums));
            }

            return result;
        }
    }
}
