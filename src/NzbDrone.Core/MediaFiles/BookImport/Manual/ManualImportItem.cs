using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.BookImport.Manual
{
    public class ManualImportItem : ModelBase
    {
        public ManualImportItem()
        {
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Author Author { get; set; }
        public Book Book { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public ParsedTrackInfo Tags { get; set; }
        public bool AdditionalFile { get; set; }
        public bool ReplaceExistingFiles { get; set; }
    }
}
