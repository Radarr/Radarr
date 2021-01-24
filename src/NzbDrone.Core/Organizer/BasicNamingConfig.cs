namespace NzbDrone.Core.Organizer
{
    public class BasicNamingConfig
    {
        public bool IncludeAuthorName { get; set; }
        public bool IncludeBookTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}
