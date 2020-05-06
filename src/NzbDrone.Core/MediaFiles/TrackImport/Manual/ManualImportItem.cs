using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public class ManualImportItem : ModelBase
    {
        public ManualImportItem()
        {
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Author Artist { get; set; }
        public Book Album { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public ParsedTrackInfo Tags { get; set; }
        public bool AdditionalFile { get; set; }
        public bool ReplaceExistingFiles { get; set; }
    }
}
