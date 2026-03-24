using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public class ImportDecision
    {
        public LocalMovie LocalMovie { get; private set; }
        public IEnumerable<ImportRejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalMovie localMovie, params ImportRejection[] rejections)
        {
            LocalMovie = localMovie;
            Rejections = rejections.ToList();
        }
    }
}
