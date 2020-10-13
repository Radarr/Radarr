using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class MovieResource
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string TitleSlug { get; set; }

        //Depricated but left in place until cache fills new object (MovieRatings)
        public List<RatingItem> Ratings { get; set; }
        public RatingResource MovieRatings { get; set; }
        public int? Runtime { get; set; }
        public List<ImageResource> Images { get; set; }
        public List<string> Genres { get; set; }

        public int Year { get; set; }
        public DateTime? Premier { get; set; }
        public DateTime? InCinema { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }

        public List<AlternativeTitleResource> AlternativeTitles { get; set; }
        public List<TranslationResource> Translations { get; set; }

        public Credits Credits { get; set; }
        public string Studio { get; set; }
        public string YoutubeTrailerId { get; set; }

        public List<CertificationResource> Certifications { get; set; }
        public string Status { get; set; }
        public CollectionResource Collection { get; set; }
        public string OriginalLanguage { get; set; }
        public string Homepage { get; set; }
        public List<RecommendationResource> Recommendations { get; set; }
    }
}
