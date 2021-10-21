using Microsoft.AspNetCore.Mvc;

namespace Radarr.Http.Frontend.Mappers
{
    public interface IMapHttpRequestsToDisk
    {
        string Map(string resourceUrl);
        bool CanHandle(string resourceUrl);
        IActionResult GetResponse(string resourceUrl);
    }
}
