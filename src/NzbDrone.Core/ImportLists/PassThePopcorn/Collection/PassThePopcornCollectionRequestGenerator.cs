using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.PassThePopcorn.Collection;

public class PassThePopcornCollectionRequestGenerator : IImportListRequestGenerator
{
    private readonly PassThePopcornCollectionSettings _settings;

    public PassThePopcornCollectionRequestGenerator(PassThePopcornCollectionSettings settings)
    {
        _settings = settings;
    }

    public ImportListPageableRequestChain GetMovies()
    {
        var pageableRequests = new ImportListPageableRequestChain();

        pageableRequests.Add(GetPagedRequests());

        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetPagedRequests()
    {
        _settings.Validate().Filter("ApiUser", "ApiKey", "MaxPages").ThrowOnError();

        var requestBuilder = BuildRequest();

        for (var pageNumber = 1; pageNumber <= _settings.MaxPages; pageNumber++)
        {
            requestBuilder.AddQueryParam("page", pageNumber, true);

            var request = requestBuilder.Build();

            yield return new ImportListRequest(request);
        }
    }

    private HttpRequestBuilder BuildRequest()
    {
        return new HttpRequestBuilder(_settings.CollectionUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("ApiUser", _settings.ApiUser)
            .SetHeader("ApiKey", _settings.ApiKey)
            .AddQueryParam("action", "get_page")
            .AddQueryParam("filter_cat[1]", "1")
            .WithRateLimit(5);
    }
}
