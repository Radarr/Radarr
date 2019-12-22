namespace NzbDrone.Core.NetImport
{
    public interface INetImportRequestGenerator
    {
        NetImportPageableRequestChain GetMovies();
    }
}
