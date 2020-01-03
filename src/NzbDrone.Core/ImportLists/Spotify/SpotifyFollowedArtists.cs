using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifyFollowedArtistsSettings : SpotifySettingsBase<SpotifyFollowedArtistsSettings>
    {
        public override string Scope => "user-follow-read";
    }

    public class SpotifyFollowedArtists : SpotifyImportListBase<SpotifyFollowedArtistsSettings>
    {
        public SpotifyFollowedArtists(ISpotifyProxy spotifyProxy,
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

        public override string Name => "Spotify Followed Artists";

        public override IList<SpotifyImportListItemInfo> Fetch(SpotifyWebAPI api)
        {
            var result = new List<SpotifyImportListItemInfo>();

            var followedArtists = _spotifyProxy.GetFollowedArtists(this, api);
            var artists = followedArtists?.Artists;

            while (true)
            {
                if (artists?.Items == null)
                {
                    return result;
                }

                foreach (var artist in artists.Items)
                {
                    result.AddIfNotNull(ParseFullArtist(artist));
                }

                if (!artists.HasNext())
                {
                    break;
                }

                followedArtists = _spotifyProxy.GetNextPage(this, api, followedArtists);
                artists = followedArtists?.Artists;
            }

            return result;
        }

        private SpotifyImportListItemInfo ParseFullArtist(FullArtist artist)
        {
            if (artist?.Name.IsNotNullOrWhiteSpace() ?? false)
            {
                return new SpotifyImportListItemInfo
                {
                    Artist = artist.Name,
                    ArtistSpotifyId = artist.Id
                };
            }

            return null;
        }
    }
}
