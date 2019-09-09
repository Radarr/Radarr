using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public class CandidateAlbumRelease
    {
        public CandidateAlbumRelease()
        {
        }
        
        public CandidateAlbumRelease(AlbumRelease release)
        {
            AlbumRelease = release;
            ExistingTracks = new List<TrackFile>();
        }
        
        public AlbumRelease AlbumRelease { get; set; }
        public List<TrackFile> ExistingTracks { get; set; }
    }
}

