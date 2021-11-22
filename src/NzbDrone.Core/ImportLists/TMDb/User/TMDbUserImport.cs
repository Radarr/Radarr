using System.Collections.Generic;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.User
{
    public class TMDbUserImport : TMDbImportListBase<TMDbUserSettings>
    {
        public TMDbUserImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb User";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseImportListResponse GetParser()
        {
            return new TMDbParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbUserRequestGenerator()
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var requestBuilder = _requestBuilder.Create()
                    .SetSegment("api", "4")
                    .SetSegment("route", "auth")
                    .SetSegment("id", "")
                    .SetSegment("secondaryRoute", "request_token")
                    .AddQueryParam("redirect_to", query["callbackUrl"]);

                requestBuilder.Method = HttpMethod.Post;

                var request = requestBuilder.Build();

                var response = Json.Deserialize<AuthRefreshTokenResource>(_httpClient.Execute(request).Content);

                var oAuthRequest = new HttpRequestBuilder(Settings.OAuthUrl)
                    .AddQueryParam("request_token", response.RequestToken)
                    .Build();

                return new
                {
                    OauthUrl = oAuthRequest.Url.ToString(),
                    RequestToken = response.RequestToken
                };
            }
            else if (action == "getOAuthToken")
            {
                var requestBuilder = _requestBuilder.Create()
                                                    .SetSegment("api", "4")
                                                    .SetSegment("route", "auth")
                                                    .SetSegment("id", "")
                                                    .SetSegment("secondaryRoute", "access_token")
                                                    .AddQueryParam("request_token", query["requestToken"]);

                requestBuilder.Method = HttpMethod.Post;

                var request = requestBuilder.Build();

                var response = Json.Deserialize<AuthAccessTokenResource>(_httpClient.Execute(request).Content);

                return new
                {
                    accountId = response.AccountId,
                    accessToken = response.AccessToken,
                };
            }

            return new { };
        }
    }
}
