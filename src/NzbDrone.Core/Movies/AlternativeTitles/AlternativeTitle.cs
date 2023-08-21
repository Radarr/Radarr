using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public class AlternativeTitle : ModelBase
    {
        public SourceType SourceType { get; set; }
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }

        public AlternativeTitle()
        {
        }

        public AlternativeTitle(string title, SourceType sourceType = SourceType.TMDB, int sourceId = 0)
        {
            Title = title;
            CleanTitle = title.CleanMovieTitle();
            SourceType = sourceType;
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

    public enum SourceType
    {
        TMDB = 0,
        Mappings = 1,
        User = 2,
        Indexer = 3
    }

    public class AlternativeYear
    {
        public int Year { get; set; }
        public int SourceId { get; set; }
    }
}
