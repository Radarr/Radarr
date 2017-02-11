using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.NetImport
{
    public interface INetImportRequestGenerator
    {
        NetImportPageableRequestChain GetMovies();
    }
}