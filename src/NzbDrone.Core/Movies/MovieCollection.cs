using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies
{
    public class MovieCollection : IEmbeddedDocument
    {
        public MovieCollection()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public int TmdbId { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
