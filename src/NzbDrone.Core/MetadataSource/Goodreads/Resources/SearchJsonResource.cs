using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    public class SearchJsonResource
    {
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("bookId")]
        public int BookId { get; set; }

        [JsonProperty("workId")]
        public int WorkId { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        [JsonProperty("from_search")]
        public bool FromSearch { get; set; }

        [JsonProperty("from_srp")]
        public bool FromSrp { get; set; }

        [JsonProperty("qid")]
        public string Qid { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("bookTitleBare")]
        public string BookTitleBare { get; set; }

        [JsonProperty("numPages")]
        public int PageCount { get; set; }

        [JsonProperty("avgRating")]
        public decimal AverageRating { get; set; }

        [JsonProperty("ratingsCount")]
        public int RatingsCount { get; set; }

        [JsonProperty("author")]
        public AuthorJsonResource Author { get; set; }

        [JsonProperty("kcrPreviewUrl")]
        public string KcrPreviewUrl { get; set; }

        [JsonProperty("description")]
        public DescriptionJsonResource Description { get; set; }
    }

    public class AuthorJsonResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isGoodreadsAuthor")]
        public bool IsGoodreadsAuthor { get; set; }

        [JsonProperty("profileUrl")]
        public string ProfileUrl { get; set; }

        [JsonProperty("worksListUrl")]
        public string WorksListUrl { get; set; }
    }

    public class DescriptionJsonResource
    {
        [JsonProperty("html")]
        public string Html { get; set; }

        [JsonProperty("truncated")]
        public bool Truncated { get; set; }

        [JsonProperty("fullContentUrl")]
        public string FullContentUrl { get; set; }
    }
}
