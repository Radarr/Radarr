using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public class ManualImportItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public Movie Movie { get; set; }

        public ManualImportItem()
        {
            CustomFormats = new List<CustomFormat>();
        }
    }
}
