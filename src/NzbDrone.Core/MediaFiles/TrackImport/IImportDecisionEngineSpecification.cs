using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IImportDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(LocalTrack localTrack);
    }
}
