using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.RadarrInstance
{
    public class RadarrInstanceSettings : IProviderConfig
    {

        public RadarrInstanceSettings()
        {
            URL = "";
            APIKey = "";
        }

        [FieldDefinition(0, Label = "Full URL")]
        public string URL { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string APIKey { get; set; }

        [FieldDefinition(2, Label = "Profile", Type = FieldType.Profile)]
        public int ProfileId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult();
        }
    }
}
