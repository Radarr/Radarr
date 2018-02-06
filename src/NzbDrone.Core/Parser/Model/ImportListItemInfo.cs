using System;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class ImportListItemInfo
    {
        public int ImportListId { get; set; }
        public string ImportList { get; set; }
        public string Artist { get; set; }
        public string ArtistMusicBrainzId { get; set; }
        public string Album { get; set; }
        public string AlbumMusicBrainzId { get; set; }
        public DateTime ReleaseDate { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1} [{2}]", ReleaseDate, Artist, Album);
        }
    }
}
