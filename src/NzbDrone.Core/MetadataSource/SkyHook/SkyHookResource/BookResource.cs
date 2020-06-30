using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class BookResource
    {
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Asin { get; set; }
        public string Description { get; set; }
        public string Isbn13 { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }
        public string EditionInformation { get; set; }
        public string Publisher { get; set; }
        public string ImageUrl { get; set; }
        public bool IsEbook { get; set; }
        public int? NumPages { get; set; }
        public int ReviewsCount { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
        public string Url { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public List<ContributorResource> Contributors { get; set; } = new List<ContributorResource>();
    }
}
