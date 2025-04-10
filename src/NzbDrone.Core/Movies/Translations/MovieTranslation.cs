using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Movies.Translations
{
    public class MovieTranslation : Entity<MovieTranslation>
    {
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string Overview { get; set; }
        public Language Language { get; set; }
    }
}
