using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.TMDb.User
{
    public class TMDbUserImport : TMDbNetImportBase<TMDbUserSettings>
    {
        public TMDbUserImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb User";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseNetImportResponse GetParser()
        {
            return new TMDbParser(_skyhookProxy);
        }

        public override INetImportRequestGenerator GetRequestGenerator()
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

                requestBuilder.Method = HttpMethod.POST;

                var request = requestBuilder.Build();

                var response = Json.Deserialize<AuthRefreshTokenResponse>(_httpClient.Execute(request).Content);

                var oAuthRequest = new HttpRequestBuilder(Settings.OAuthUrl)
                    .AddQueryParam("request_token", response.request_token)
                    .Build();

                return new
                {
                    OauthUrl = oAuthRequest.Url.ToString(),
                    RequestToken = response.request_token
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

                requestBuilder.Method = HttpMethod.POST;

                var request = requestBuilder.Build();

                var response = Json.Deserialize<AuthAccessTokenResponse>(_httpClient.Execute(request).Content);

                return new
                {
                    accountId = response.account_id,
                    accessToken = response.access_token,
                };
            }

            return new { };
        }
    }
}
