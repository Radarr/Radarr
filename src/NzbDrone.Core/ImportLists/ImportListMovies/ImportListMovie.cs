using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public class ImportListMovie : ModelBase
    {
        public ImportListMovie()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Translations = new List<MovieTranslation>();
            Ratings = new Ratings();
        }

        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public string Website { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }

        public MovieCollection Collection { get; set; }

        public string Certification { get; set; }
        public DateTime? InCinemas { get; set; }

        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public List<MovieTranslation> Translations { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }
        public string OriginalTitle { get; set; }
        public int ListId { get; set; }
    }
}
