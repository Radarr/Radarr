using System.Collections;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport
{
    public class NetImportPageableRequest : IEnumerable<NetImportRequest>
    {
        private readonly IEnumerable<NetImportRequest> _enumerable;

        public NetImportPageableRequest(IEnumerable<NetImportRequest> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<NetImportRequest> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }
    }
}
