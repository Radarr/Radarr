using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifyImportListItemInfo : ImportListItemInfo
    {
        public string ArtistSpotifyId { get; set; }
        public string AlbumSpotifyId { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ArtistSpotifyId, AlbumSpotifyId);
        }
    }
}
