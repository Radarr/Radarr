using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IImportDecisionEngineSpecification
    {
        IEnumerable<Decision> IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem);
    }
}
