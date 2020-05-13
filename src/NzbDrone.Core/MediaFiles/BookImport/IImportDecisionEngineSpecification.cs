using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.MediaFiles.BookImport
{
    public interface IImportDecisionEngineSpecification<T>
    {
        Decision IsSatisfiedBy(T item, DownloadClientItem downloadClientItem);
    }
}
