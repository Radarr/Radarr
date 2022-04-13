using System;
using System.Collections.Generic;
using Equ;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;

namespace NzbDrone.Core.Movies
{
    public class MovieMetadata : Entity<MovieMetadata>
    {
        public MovieMetadata()
        {
            AlternativeTitles = new List<AlternativeTitle>();
            Translations = new List<MovieTranslation>();
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            OriginalLanguage = Language.English;
            Recommendations = new List<int>();
            Ratings = new Ratings();
        }

        public int TmdbId { get; set; }

        public List<MediaCover.MediaCover> Images { get; set; }
        public List<string> Genres { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public string Certification { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }

        public int CollectionTmdbId { get; set; }
        public string CollectionTitle { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public string Website { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }

        //Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public List<MovieTranslation> Translations { get; set; }

        public int? SecondaryYear { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }
        public string OriginalTitle { get; set; }
        public string CleanOriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<int> Recommendations { get; set; }
        public float Popularity { get; set; }

        [MemberwiseEqualityIgnore]
        public bool IsRecentMovie
        {
            get
            {
                if (PhysicalRelease.HasValue)
                {
                    return PhysicalRelease.Value >= DateTime.UtcNow.AddDays(-21);
                }

                if (InCinemas.HasValue)
                {
                    return InCinemas.Value >= DateTime.UtcNow.AddDays(-120);
                }

                return true;
            }
        }

        public DateTime PhysicalReleaseDate()
        {
            return PhysicalRelease ?? (InCinemas?.AddDays(90) ?? DateTime.MaxValue);
        }
    }
}
