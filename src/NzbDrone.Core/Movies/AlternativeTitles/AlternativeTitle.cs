using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public class AlternativeTitle : ModelBase
    {
        public SourceType SourceType { get; set; }
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public int SourceId { get; set; }
        public int Votes { get; set; }
        public int VoteCount { get; set; }
        public Language Language { get; set; }

        public AlternativeTitle()
        {
        }

        public AlternativeTitle(string title, SourceType sourceType = SourceType.TMDB, int sourceId = 0, Language language = null)
        {
            Title = title;
            CleanTitle = title.CleanMovieTitle();
            SourceType = sourceType;
            SourceId = sourceId;
            Language = language ?? Language.English;
        }

        public bool IsTrusted(int minVotes = 4)
        {
            switch (SourceType)
            {
                case SourceType.Mappings:
                    return Votes >= minVotes;
                default:
                    return true;
            }
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
