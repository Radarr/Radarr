using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Movies
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Tags = new HashSet<int>();
            MovieMetadata = new MovieMetadata();
        }

        public int MovieMetadataId { get; set; }

        public bool Monitored { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int ProfileId { get; set; }

        public string Path { get; set; }

        public LazyLoaded<MovieMetadata> MovieMetadata { get; set; }

        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public Profile Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MovieFile MovieFile { get; set; }
        public int MovieFileId { get; set; }

        public bool HasFile => MovieFileId > 0;

        //compatibility properties
        public string Title
        {
            get { return MovieMetadata.Value.Title; }
            set { MovieMetadata.Value.Title = value; }
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

        public int Year
        {
            get { return MovieMetadata.Value.Year; }
            set { MovieMetadata.Value.Year = value; }
        }

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
                    minimumAvailabilityDate = MovieMetadata.Value.InCinemas.HasValue ? MovieMetadata.Value.InCinemas.Value.AddDays(90) : DateTime.MaxValue;
                }
            }

            if (minimumAvailabilityDate == DateTime.MinValue || minimumAvailabilityDate == DateTime.MaxValue)
            {
                return DateTime.Now >= minimumAvailabilityDate;
            }

            return DateTime.Now >= minimumAvailabilityDate.AddDays((double)delay);
        }

        public override string ToString()
        {
            return string.Format("[{1} ({2})][{0}, {3}]", MovieMetadata.Value.ImdbId, MovieMetadata.Value.Title.NullSafe(), MovieMetadata.Value.Year.NullSafe(), MovieMetadata.Value.TmdbId);
        }

        public void ApplyChanges(Movie otherMovie)
        {
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
