using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public interface INetImportRequestGenerator
    {
        NetImportPageableRequestChain GetMovies();
        void Clean(Movie movie);
    }
}