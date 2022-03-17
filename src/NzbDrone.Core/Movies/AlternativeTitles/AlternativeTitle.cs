using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public class AlternativeTitle : ModelBase
    {
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public Language Language { get; set; }

        public AlternativeTitle()
        {
        }

        public AlternativeTitle(string title, int sourceId = 0, Language language = null)
        {
            Title = title;
            CleanTitle = title.CleanMovieTitle();
            Language = language ?? Language.English;
        }

        public override bool Equals(object obj)
        {
            var item = obj as AlternativeTitle;

            if (item == null)
            {
                return false;
            }

            return item.CleanTitle == CleanTitle;
        }

        public override int GetHashCode()
        {
            return CleanTitle.GetHashCode();
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
