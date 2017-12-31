using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.MediaFiles;
using System.IO;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;

namespace NzbDrone.Core.Tv
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Actors = new List<Actor>();
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
        public string FlatFileName { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public List<Actor> Actors { get; set; }
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public MoviePathState PathState { get; set; }
        public DateTime Added { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public String PhysicalReleaseNote { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MovieFile MovieFile { get; set; }
		public bool HasPreDBEntry { get; set; }
        public int MovieFileId { get; set; }
        //Get Loaded via a Join Query
        public List<AlternativeTitle> AlternativeTitles { get; set; }
        public int? SecondaryYear { get; set; }
        public int SecondaryYearSourceId { get; set; }
        public string YouTubeTrailerId{ get; set; }
        public string Studio { get; set; }

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
            DateTime MinimumAvailabilityDate;
            switch (MinimumAvailability)
            {
                case MovieStatusType.TBA:
                case MovieStatusType.Announced:
                    MinimumAvailabilityDate = DateTime.MinValue;
                    break;
                case MovieStatusType.InCinemas:
                    if (InCinemas.HasValue)
                        MinimumAvailabilityDate = InCinemas.Value;
                    else
                        MinimumAvailabilityDate = DateTime.MaxValue;
                    break;
                
                case MovieStatusType.Released:
                case MovieStatusType.PreDB:
                default:
                    MinimumAvailabilityDate = PhysicalRelease.HasValue ? PhysicalRelease.Value : (InCinemas.HasValue ? InCinemas.Value.AddDays(90) : DateTime.MaxValue);
                    break;
            }

			if (HasPreDBEntry && MinimumAvailability == MovieStatusType.PreDB)
			{
				return true;
			}

            if (MinimumAvailabilityDate == DateTime.MinValue || MinimumAvailabilityDate == DateTime.MaxValue)
            {
                return DateTime.Now >= MinimumAvailabilityDate;
            }


            return DateTime.Now >= MinimumAvailabilityDate.AddDays((double)delay);
        }

        public DateTime PhysicalReleaseDate()
        {
            return PhysicalRelease ?? (InCinemas?.AddDays(90) ?? DateTime.MaxValue);
        }

        public override string ToString()
        {
            return string.Format("[{0}][{1} ({2})]", ImdbId, Title.NullSafe(), Year.NullSafe());
        }
    }

    public class AddMovieOptions : MonitoringOptions
    {
        public bool SearchForMovie { get; set; }
    }

    public enum MoviePathState
    {
        Dynamic,
        StaticOnce,
        Static,
    }
}
