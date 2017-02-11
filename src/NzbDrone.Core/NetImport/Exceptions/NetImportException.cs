using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.NetImport.Exceptions
{
    public class NetImportException : NzbDroneException
    {
        private readonly NetImportResponse _netImportResponse;

        public NetImportException(NetImportResponse response, string message, params object[] args)
            : base(message, args)
        {
            _netImportResponse = response;
        }

        public NetImportException(NetImportResponse response, string message)
            : base(message)
        {
            _netImportResponse = response;
        }

        public NetImportResponse Response => _netImportResponse;
    }
}
