using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles.BookImport.Identification;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEdition
    {
        public LocalEdition()
        {
            LocalBooks = new List<LocalBook>();

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public LocalEdition(List<LocalBook> tracks)
        {
            LocalBooks = tracks;

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public List<LocalBook> LocalBooks { get; set; }
        public int TrackCount => LocalBooks.Count;

        public Distance Distance { get; set; }
        public Edition Edition { get; set; }
        public List<LocalBook> ExistingTracks { get; set; }
        public bool NewDownload { get; set; }

        public void PopulateMatch()
        {
            if (Edition != null)
            {
                LocalBooks = LocalBooks.Concat(ExistingTracks).DistinctBy(x => x.Path).ToList();
                foreach (var localTrack in LocalBooks)
                {
                    localTrack.Edition = Edition;
                    localTrack.Book = Edition.Book.Value;
                    localTrack.Author = Edition.Book.Value.Author.Value;
                }
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", LocalBooks.Select(x => Path.GetDirectoryName(x.Path)).Distinct()) + "]";
        }
    }
}
