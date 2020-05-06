using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalAlbumRelease
    {
        public LocalAlbumRelease()
        {
            LocalTracks = new List<LocalTrack>();

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public LocalAlbumRelease(List<LocalTrack> tracks)
        {
            LocalTracks = tracks;

            // A dummy distance, will be replaced
            Distance = new Distance();
            Distance.Add("album_id", 1.0);
        }

        public List<LocalTrack> LocalTracks { get; set; }
        public int TrackCount => LocalTracks.Count;

        public Distance Distance { get; set; }
        public Book Book { get; set; }
        public List<LocalTrack> ExistingTracks { get; set; }
        public bool NewDownload { get; set; }

        public void PopulateMatch()
        {
            if (Book != null)
            {
                LocalTracks = LocalTracks.Concat(ExistingTracks).DistinctBy(x => x.Path).ToList();
                foreach (var localTrack in LocalTracks)
                {
                    localTrack.Album = Book;
                    localTrack.Artist = Book.Author.Value;
                }
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", LocalTracks.Select(x => Path.GetDirectoryName(x.Path)).Distinct()) + "]";
        }
    }
}
