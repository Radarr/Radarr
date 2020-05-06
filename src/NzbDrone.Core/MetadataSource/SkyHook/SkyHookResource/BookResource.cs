using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class BookResource
    {
        public string ForeignId { get; set; }
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Asin { get; set; }
        public string Description { get; set; }
        public string Isbn13 { get; set; }
        public long Rvn { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public string DisplayGroup { get; set; }
        public string ImageUrl { get; set; }
        public string KindleMappingStatus { get; set; }
        public string Marketplace { get; set; }
        public int? NumPages { get; set; }
        public int ReviewsCount { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
        public IList<BookSeriesLinkResource> SeriesLinks { get; set; } = new List<BookSeriesLinkResource>();
        public string WebUrl { get; set; }
        public string WorkForeignId { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public List<ContributorResource> Contributors { get; set; } = new List<ContributorResource>();
        public List<AuthorSummaryResource> AuthorMetadata { get; set; } = new List<AuthorSummaryResource>();
    }

    public class BookSeriesLinkResource
    {
        public string SeriesId { get; set; }
        public string Position { get; set; }
    }
}
