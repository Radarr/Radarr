using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalTrack
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public ParsedTrackInfo FileTrackInfo { get; set; }
        public ParsedTrackInfo FolderTrackInfo { get; set; }
        public ParsedAlbumInfo DownloadClientAlbumInfo { get; set; }
        public List<string> AcoustIdResults { get; set; }
        public Author Artist { get; set; }
        public Book Album { get; set; }
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
