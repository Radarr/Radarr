using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies.AlternativeTitles;
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
        public MoviePathState PathState { get; set; }
        public DateTime Added { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public string PhysicalReleaseNote { get; set; }
        public Profile Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MovieFile MovieFile { get; set; }
        public bool HasPreDBEntry { get; set; }
        public int MovieFileId { get; set; }

        //Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public int? SecondaryYear { get; set; }
        public int SecondaryYearSourceId { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }

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
            switch (MinimumAvailability)
            {
                case MovieStatusType.TBA:
                case MovieStatusType.Announced:
                    minimumAvailabilityDate = DateTime.MinValue;
                    break;
                case MovieStatusType.InCinemas:
                    if (InCinemas.HasValue)
                    {
                        minimumAvailabilityDate = InCinemas.Value;
                    }
                    else
                    {
                        minimumAvailabilityDate = DateTime.MaxValue;
                    }

                    break;

                case MovieStatusType.Released:
                case MovieStatusType.PreDB:
                default:
                    minimumAvailabilityDate = PhysicalRelease.HasValue ? PhysicalRelease.Value : (InCinemas.HasValue ? InCinemas.Value.AddDays(90) : DateTime.MaxValue);
                    break;
            }

            if (HasPreDBEntry && MinimumAvailability == MovieStatusType.PreDB)
            {
                return true;
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
            PathState = otherMovie.PathState;

            Monitored = otherMovie.Monitored;
            MinimumAvailability = otherMovie.MinimumAvailability;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }

    public enum MoviePathState
    {
        Dynamic,
        StaticOnce,
        Static,
    }
}
