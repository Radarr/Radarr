namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class AuthorSummaryResource
    {
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

        public int ReviewCount { get; set; }
        public int RatingsCount { get; set; }
        public double AverageRating { get; set; }
    }
}
