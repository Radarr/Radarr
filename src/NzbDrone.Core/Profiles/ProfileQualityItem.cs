using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles
{
    public class ProfileQualityItem : IEmbeddedDocument
    {

        public Quality Quality { get; set; } //Stale property used for legacy handling, will be removed in the future.
        public QualityDefinition QualityDefinition { get; set; }
        public bool Allowed { get; set; }
    }
}
