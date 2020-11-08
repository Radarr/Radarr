using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.ImportLists.Exceptions
{
    public class ImportListException : NzbDroneException
    {
        public ImportListException(ImportListResponse response, string message, params object[] args)
            : base(message, args)
        {
            Response = response;
        }

        public ImportListException(ImportListResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        public ImportListResponse Response { get; private set; }
    }
}
