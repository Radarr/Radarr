using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.MediaTypes;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Movies
{
    public class Movie : MediaItem
    {
        public Movie()
        {
            MovieMetadata = new MovieMetadata();
            MediaType = MediaType.Movie;
        }

        public int MovieMetadataId { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }

        public LazyLoaded<MovieMetadata> MovieMetadata { get; set; }

        public QualityProfile QualityProfile { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MovieFile MovieFile { get; set; }
        public int MovieFileId { get; set; }

        public bool HasFile => MovieFileId > 0;

        // MediaItem abstract method implementations
        public override string GetTitle() => MovieMetadata.Value.Title;
        public override int GetYear() => MovieMetadata.Value.Year;

        // Backward-compatible property accessors
        public string Title
        {
            get => MovieMetadata.Value.Title;
            set => MovieMetadata.Value.Title = value;
        }

        public int Year
        {
            get => MovieMetadata.Value.Year;
            set => MovieMetadata.Value.Year = value;
        }

        public int TmdbId
        {
            get { return MovieMetadata.Value.TmdbId; }
            set { MovieMetadata.Value.TmdbId = value; }
        }

        public string ImdbId
        {
            get { return MovieMetadata.Value.ImdbId; }
            set { MovieMetadata.Value.ImdbId = value; }
        }

        public string FolderName()
        {
            if (Path.IsNullOrWhiteSpace())
            {
                return "";
            }

            // Well what about Path = Null?
            // return new DirectoryInfo(Path).Name;
            return Path;
        }

        public bool IsAvailable(int delay = 0)
        {
            // the below line is what was used before delay was implemented, could still be used for cases when delay==0
            // return (Status >= MinimumAvailability || (MinimumAvailability == MovieStatusType.PreDB && Status >= MovieStatusType.Released));

            // This more complex sequence handles the delay
            DateTime minimumAvailabilityDate;

            if (MinimumAvailability is MovieStatusType.TBA or MovieStatusType.Announced)
            {
                minimumAvailabilityDate = DateTime.MinValue;
            }
            else if (MinimumAvailability == MovieStatusType.InCinemas && MovieMetadata.Value.InCinemas.HasValue)
            {
                minimumAvailabilityDate = MovieMetadata.Value.InCinemas.Value;
            }
            else
            {
                if (MovieMetadata.Value.PhysicalRelease.HasValue && MovieMetadata.Value.DigitalRelease.HasValue)
                {
                    minimumAvailabilityDate = new DateTime(Math.Min(MovieMetadata.Value.PhysicalRelease.Value.Ticks, MovieMetadata.Value.DigitalRelease.Value.Ticks));
                }
                else if (MovieMetadata.Value.PhysicalRelease.HasValue)
                {
                    minimumAvailabilityDate = MovieMetadata.Value.PhysicalRelease.Value;
                }
                else if (MovieMetadata.Value.DigitalRelease.HasValue)
                {
                    minimumAvailabilityDate = MovieMetadata.Value.DigitalRelease.Value;
                }
                else
                {
                    minimumAvailabilityDate = MovieMetadata.Value.InCinemas?.AddDays(90) ?? DateTime.MaxValue;
                }
            }

            if (minimumAvailabilityDate == DateTime.MinValue || minimumAvailabilityDate == DateTime.MaxValue)
            {
                return DateTime.UtcNow >= minimumAvailabilityDate;
            }

            return DateTime.UtcNow >= minimumAvailabilityDate.AddDays(delay);
        }

        public DateTime? GetReleaseDate()
        {
            if (MinimumAvailability is MovieStatusType.TBA or MovieStatusType.Announced)
            {
                return new[] { MovieMetadata.Value.InCinemas, MovieMetadata.Value.DigitalRelease, MovieMetadata.Value.PhysicalRelease }
                    .Where(x => x.HasValue)
                    .Min();
            }

            if (MinimumAvailability == MovieStatusType.InCinemas && MovieMetadata.Value.InCinemas.HasValue)
            {
                return MovieMetadata.Value.InCinemas.Value;
            }

            if (MovieMetadata.Value.DigitalRelease.HasValue || MovieMetadata.Value.PhysicalRelease.HasValue)
            {
                return new[] { MovieMetadata.Value.DigitalRelease, MovieMetadata.Value.PhysicalRelease }
                    .Where(x => x.HasValue)
                    .Min();
            }

            return MovieMetadata.Value.InCinemas?.AddDays(90);
        }

        public override string ToString()
        {
            return string.Format("[{1} ({2})][{0}, {3}]", MovieMetadata.Value.ImdbId, MovieMetadata.Value.Title.NullSafe(), MovieMetadata.Value.Year.NullSafe(), MovieMetadata.Value.TmdbId);
        }

        public void ApplyChanges(Movie otherMovie)
        {
            Path = otherMovie.Path;
            QualityProfileId = otherMovie.QualityProfileId;

            Monitored = otherMovie.Monitored;
            MinimumAvailability = otherMovie.MinimumAvailability;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }
}
