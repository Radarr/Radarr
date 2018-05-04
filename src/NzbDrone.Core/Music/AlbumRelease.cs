using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class AlbumRelease : IEmbeddedDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int TrackCount { get; set; }
        public int MediaCount { get; set; }
        public string Disambiguation { get; set; }
        public List<string> Country { get; set; }
        public string Format { get; set; }
        public List<string> Label { get; set; }
    }
}
