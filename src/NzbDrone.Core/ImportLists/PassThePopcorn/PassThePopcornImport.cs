using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.PassThePopcorn;

public class PassThePopcornImport : ImportListBase<PassThePopcornCollectionSettings>
{
    public override string Name => "PassThePopcorn Collection";

    public override ImportListType ListType => ImportListType.Other;
    public override bool Enabled => true;
    public override bool EnableAuto => false;

    private readonly IHttpClient _httpClient;

    public PassThePopcornImport(
        IHttpClient httpClient,
        IImportListStatusService importListStatusService,
        IConfigService configService,
        IParsingService parsingService,
        Logger logger)
        : base(importListStatusService, configService, parsingService, logger)
    {
        _httpClient = httpClient;
    }

    private HttpRequestBuilder GetRequestBuilder()
    {
        return new HttpRequestBuilder($"{Settings.BaseUrl.Trim().TrimEnd('/')}/collages.php")
            .SetHeader("ApiUser", Settings.APIUser).SetHeader("ApiKey", Settings.APIKey)
            .AddQueryParam("id", Settings.Id).AddQueryParam("action", "get_page");
    }

    private static CollectionPage ParseCollectionPage(HttpResponse response)
    {
        return JsonConvert.DeserializeObject<CollectionPage>(response.Content);
    }

    public override ImportListFetchResult Fetch()
    {
        var requestBuilder = GetRequestBuilder();

        _logger.Info($"Importing PTP movies from collection: {Settings.Id}");

        var firstPage = ParseCollectionPage(_httpClient.Execute(requestBuilder.AddQueryParam("page", 1).Build()));

        var pageCount = firstPage.Pages.Split(" | ").Length + 1;

        _logger.Debug($"Page count: {pageCount}");

        var pages = new List<CollectionPage>() { firstPage };

        for (var pageNumber = 2; pageNumber <= pageCount; pageNumber++)
        {
            requestBuilder.AddQueryParam("page", pageNumber, true);

            pages.Add(ParseCollectionPage(_httpClient.Execute(requestBuilder.Build())));

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        var movies = new List<ImportListMovie>();

        foreach (var page in pages)
        {
            foreach (var movie in page.CoverView.Movies)
            {
                movies.Add(new ImportListMovie()
                {
                    Title = WebUtility.HtmlDecode(movie.Title),
                    ImdbId = $"tt{movie.ImdbId}"
                });
            }
        }

        return new ImportListFetchResult() { Movies = movies };
    }

    protected override void Test(List<ValidationFailure> failures)
    {
        var requestBuilder = GetRequestBuilder();

        var response = _httpClient.Execute(requestBuilder.Build());

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var message = $"PTP returned a status code of {response.StatusCode}.";

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                message += " Maybe your API credentials are invalid?";
            }

            throw new IndexerException(new IndexerResponse(new IndexerRequest(requestBuilder.Build()), response), message);
        }

        // Ensure the response is valid JSON for a collection page.
        ParseCollectionPage(response);
    }
}
