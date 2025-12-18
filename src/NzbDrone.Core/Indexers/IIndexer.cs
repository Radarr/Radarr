using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaTypes;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        DownloadProtocol Protocol { get; }
        IEnumerable<MediaType> SupportedMediaTypes { get; }

        Task<IList<ReleaseInfo>> FetchRecent();
        Task<IList<ReleaseInfo>> Fetch(MovieSearchCriteria searchCriteria);
        HttpRequest GetDownloadRequest(string link);
    }
}
