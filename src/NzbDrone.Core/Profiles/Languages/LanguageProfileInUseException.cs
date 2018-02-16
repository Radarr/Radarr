using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Profiles.Languages
{
    public class LanguageProfileInUseException : NzbDroneClientException
    {
        public LanguageProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "Language profile [{0}] is in use.", name)
        {

        }
    }
}
