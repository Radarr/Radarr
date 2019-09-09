namespace NzbDrone.Core.Organizer
{
    public class BasicNamingConfig
    {
        public bool IncludeArtistName { get; set; }
        public bool IncludeAlbumTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}
