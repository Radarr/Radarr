using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IImportDecisionEngineSpecification<T>
    {
        Decision IsSatisfiedBy(T item);
    }
}
