using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameTracks { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public string StandardTrackFormat { get; set; }
        public string ArtistFolderFormat { get; set; }
        public bool IncludeArtistName { get; set; }
        public bool IncludeAlbumTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}
