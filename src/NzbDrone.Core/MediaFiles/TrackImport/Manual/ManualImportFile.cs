using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public class ManualImportFile : IEquatable<ManualImportFile>
    {
        public string Path { get; set; }
        public string FolderName { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public List<int> TrackIds { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public string DownloadId { get; set; }

        public bool Equals(ManualImportFile other)
        {
            if (other == null)
            {
                return false;
            }
            return Path.PathEquals(other.Path);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Path.PathEquals(((ManualImportFile)obj).Path);
        }
        public override int GetHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }
    }
}
