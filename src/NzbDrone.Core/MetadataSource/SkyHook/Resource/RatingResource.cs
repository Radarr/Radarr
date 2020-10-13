namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class RatingResource
    {
        public RatingItem Tmdb { get; set; }
        public RatingItem Imdb { get; set; }
        public RatingItem Metacritic { get; set; }
        public RatingItem RottenTomatoes { get; set; }
    }

    public class RatingItem
    {
        public int Count { get; set; }
        public decimal Value { get; set; }
        public string Origin { get; set; }
        public string Type { get; set; }
    }
}
