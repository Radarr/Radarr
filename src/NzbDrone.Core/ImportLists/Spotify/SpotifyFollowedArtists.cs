using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifyFollowedArtistsSettings : SpotifySettingsBase<SpotifyFollowedArtistsSettings>
    {
        public override string Scope => "user-follow-read";
    }

    public class SpotifyFollowedArtists : SpotifyImportListBase<SpotifyFollowedArtistsSettings>
    {
        public SpotifyFollowedArtists(IImportListStatusService importListStatusService,
                                      IImportListRepository importListRepository,
                                      IConfigService configService,
                                      IParsingService parsingService,
                                      HttpClient httpClient,
                                      Logger logger)
        : base(importListStatusService, importListRepository, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "Spotify Followed Artists";

        public override IList<ImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            var result = new List<ImportListItemInfo>();
            
            var followed = Execute(api, (x) => x.GetFollowedArtists(FollowType.Artist, 50));
            var artists = followed.Artists;
            while (true)
            {
                foreach (var artist in artists.Items)
                {
                    if (artist.Name.IsNotNullOrWhiteSpace())
                    {
                        result.AddIfNotNull(new ImportListItemInfo
                                            {
                                                Artist = artist.Name,
                                            });
                    }
                }
                if (!artists.HasNext())
                    break;
                artists = Execute(api, (x) => x.GetNextPage(artists));
            }

            return result;
        }
    }
}
