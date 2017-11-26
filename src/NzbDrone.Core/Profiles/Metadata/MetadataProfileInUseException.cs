using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class MetadataProfileInUseException : NzbDroneException
    {
        public MetadataProfileInUseException(int profileId)
            : base("Metadata profile [{0}] is in use.", profileId)
        {

        }
    }
}
