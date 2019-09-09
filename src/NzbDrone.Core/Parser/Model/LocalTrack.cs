using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using System;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalTrack
    {
        public LocalTrack()
        {
            Tracks = new List<Track>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public ParsedTrackInfo FileTrackInfo { get; set; }
        public ParsedTrackInfo FolderTrackInfo { get; set; }
        public ParsedAlbumInfo DownloadClientAlbumInfo { get; set; }
        public List<string> AcoustIdResults { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public AlbumRelease Release { get; set; }
        public List<Track> Tracks { get; set; }
        public Distance Distance { get; set; }
        public QualityModel Quality { get; set; }
        public bool ExistingFile { get; set; }
        public bool AdditionalFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
