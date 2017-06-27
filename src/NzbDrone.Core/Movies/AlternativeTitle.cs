namespace NzbDrone.Core.Movies
{
    public class AlternativeTitle
    {
        public SourceType SourceType { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public int SourceId { get; set; }
        public int Votes { get; set; }
        public int VoteCount { get; set; }

        public bool IsTrusted(int minVotes = 3)
        {
            switch (SourceType)
            {
                case SourceType.TMDB:
                    return Votes >= minVotes;
                default:
                    return true;
            }   
        }
    }

    public enum SourceType
    {
        TMDB = 0,
        Mappings = 1,
        User = 2,
        Indexer = 3
    }
}
