using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Profiles.Metadata
{
    public class MetadataProfileInUseException : NzbDroneClientException
    {
        public MetadataProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "Metadata profile [{0}] is in use.", name)
        {
        }
    }
}
