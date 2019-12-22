using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.NetImport
{
    public class NetImportPageableRequestChain
    {
        private List<List<NetImportPageableRequest>> _chains;

        public NetImportPageableRequestChain()
        {
            _chains = new List<List<NetImportPageableRequest>>();
            _chains.Add(new List<NetImportPageableRequest>());
        }

        public int Tiers => _chains.Count;

        public IEnumerable<NetImportPageableRequest> GetAllTiers()
        {
            return _chains.SelectMany(v => v);
        }

        public IEnumerable<NetImportPageableRequest> GetTier(int index)
        {
            return _chains[index];
        }

        public void Add(IEnumerable<NetImportRequest> request)
        {
            if (request == null) return;

            _chains.Last().Add(new NetImportPageableRequest(request));
        }

        public void AddTier(IEnumerable<NetImportRequest> request)
        {
            AddTier();
            Add(request);
        }

        public void AddTier()
        {
            if (_chains.Last().Count == 0) return;

            _chains.Add(new List<NetImportPageableRequest>());
        }
    }
}
