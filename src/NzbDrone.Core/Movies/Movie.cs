using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Movies
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Tags = new HashSet<int>();
            AlternativeTitles = new List<AlternativeTitle>();
            Translations = new List<MovieTranslation>();
            Recommendations = new List<int>();
            OriginalLanguage = Language.English;
            Ratings = new Ratings();
        }

        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int ProfileId { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public string TitleSlug { get; set; }
        public string Website { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }

        public MovieCollection Collection { get; set; }

        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public Profile Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MovieFile MovieFile { get; set; }
        public int MovieFileId { get; set; }

        //Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public List<MovieTranslation> Translations { get; set; }
        public int? SecondaryYear { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }
        public string OriginalTitle { get; set; }
        public Language OriginalLanguage { get; set; }
        public List<int> Recommendations { get; set; }

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

        public bool HasFile => MovieFileId > 0;

        public string FolderName()
        {
            if (Path.IsNullOrWhiteSpace())
            {
                return "";
            }

            //Well what about Path = Null?
            //return new DirectoryInfo(Path).Name;
            return Path;
        }

        public bool IsAvailable(int delay = 0)
        {
            //the below line is what was used before delay was implemented, could still be used for cases when delay==0
            //return (Status >= MinimumAvailability || (MinimumAvailability == MovieStatusType.PreDB && Status >= MovieStatusType.Released));

            //This more complex sequence handles the delay
            DateTime minimumAvailabilityDate;

            if ((MinimumAvailability == MovieStatusType.TBA) || (MinimumAvailability == MovieStatusType.Announced))
            {
                minimumAvailabilityDate = DateTime.MinValue;
            }
            else if (MinimumAvailability == MovieStatusType.InCinemas && InCinemas.HasValue)
            {
                minimumAvailabilityDate = InCinemas.Value;
            }
            else
            {
                if (PhysicalRelease.HasValue && DigitalRelease.HasValue)
                {
                    minimumAvailabilityDate = new DateTime(Math.Min(PhysicalRelease.Value.Ticks, DigitalRelease.Value.Ticks));
                }
                else if (PhysicalRelease.HasValue)
                {
                    minimumAvailabilityDate = PhysicalRelease.Value;
                }
                else if (DigitalRelease.HasValue)
                {
                    minimumAvailabilityDate = DigitalRelease.Value;
                }
                else
                {
                    minimumAvailabilityDate = InCinemas.HasValue ? InCinemas.Value.AddDays(90) : DateTime.MaxValue;
                }
            }

            if (minimumAvailabilityDate == DateTime.MinValue || minimumAvailabilityDate == DateTime.MaxValue)
            {
                return DateTime.Now >= minimumAvailabilityDate;
            }

            return DateTime.Now >= minimumAvailabilityDate.AddDays((double)delay);
        }

        public DateTime PhysicalReleaseDate()
        {
            return PhysicalRelease ?? (InCinemas?.AddDays(90) ?? DateTime.MaxValue);
        }

        public override string ToString()
        {
            return string.Format("[{1} ({2})][{0}, {3}]", ImdbId, Title.NullSafe(), Year.NullSafe(), TmdbId);
        }

        public void ApplyChanges(Movie otherMovie)
        {
            TmdbId = otherMovie.TmdbId;

            Path = otherMovie.Path;
            ProfileId = otherMovie.ProfileId;

            Monitored = otherMovie.Monitored;
            MinimumAvailability = otherMovie.MinimumAvailability;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }
}
