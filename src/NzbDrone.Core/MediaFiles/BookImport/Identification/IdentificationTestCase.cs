using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.MediaFiles.BookImport.Identification
{
    public class BasicLocalTrack
    {
        public string Path { get; set; }
        public ParsedTrackInfo FileTrackInfo { get; set; }
    }

    public class AuthorTestCase
    {
        public string Author { get; set; }
        public MetadataProfile MetadataProfile { get; set; }
    }

    public class AcoustIdTestCase
    {
        public string Path { get; set; }
        public List<string> AcoustIdResults { get; set; }
    }

    public class IdTestCase
    {
        public List<string> ExpectedMusicBrainzReleaseIds { get; set; }
        public List<AuthorTestCase> LibraryAuthors { get; set; }
        public string Author { get; set; }
        public string Book { get; set; }
        public string Release { get; set; }
        public bool NewDownload { get; set; }
        public bool SingleRelease { get; set; }
        public List<BasicLocalTrack> Tracks { get; set; }
        public List<AcoustIdTestCase> Fingerprints { get; set; }
    }
}
