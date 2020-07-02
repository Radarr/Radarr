using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.OAuth;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource.Goodreads;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Goodreads
{
    public abstract class GoodreadsImportListBase<TSettings> : ImportListBase<TSettings>
        where TSettings : GoodreadsSettingsBase<TSettings>, new()
    {
        protected readonly IHttpClient _httpClient;

        protected GoodreadsImportListBase(IImportListStatusService importListStatusService,
                                          IConfigService configService,
                                          IParsingService parsingService,
                                          IHttpClient httpClient,
                                          Logger logger)
        : base(importListStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override ImportListType ListType => ImportListType.Goodreads;

        public string AccessToken => Settings.AccessToken;

        protected HttpRequestBuilder RequestBuilder() => new HttpRequestBuilder("https://www.goodreads.com/{route}")
            .AddQueryParam("_nc", "1")
            .KeepAlive();

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                GetUser();
                return null;
            }
            catch (Common.Http.HttpException ex)
            {
                _logger.Warn(ex, "Goodreads Authentication Error");
                return new ValidationFailure(string.Empty, $"Goodreads authentication error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to Goodreads");

                return new ValidationFailure(string.Empty, "Unable to connect to import list, check the log for more details");
            }
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                var oAuthRequest = OAuthRequest.ForRequestToken(null, null, query["callbackUrl"]);
                oAuthRequest.RequestUrl = Settings.OAuthRequestTokenUrl;
                var qscoll = OAuthQuery(oAuthRequest);

                var url = string.Format("{0}?oauth_token={1}&oauth_callback={2}", Settings.OAuthUrl, qscoll["oauth_token"], query["callbackUrl"]);

                return new
                {
                    OauthUrl = url,
                    RequestTokenSecret = qscoll["oauth_token_secret"]
                };
            }
            else if (action == "getOAuthToken")
            {
                if (query["oauth_token"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam oauth_token invalid.");
                }

                if (query["requestTokenSecret"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("Missing requestTokenSecret.");
                }

                var oAuthRequest = OAuthRequest.ForAccessToken(null, null, query["oauth_token"], query["requestTokenSecret"], "");
                oAuthRequest.RequestUrl = Settings.OAuthAccessTokenUrl;
                var qscoll = OAuthQuery(oAuthRequest);

                Settings.AccessToken = qscoll["oauth_token"];
                Settings.AccessTokenSecret = qscoll["oauth_token_secret"];

                var user = GetUser();

                return new
                {
                    Settings.AccessToken,
                    Settings.AccessTokenSecret,
                    RequestTokenSecret = "",
                    UserId = user.Item1,
                    UserName = user.Item2
                };
            }

            return new { };
        }

        protected Common.Http.HttpResponse OAuthGet(HttpRequestBuilder builder)
        {
            var auth = OAuthRequest.ForProtectedResource(builder.Method.ToString(), null, null, Settings.AccessToken, Settings.AccessTokenSecret);

            var request = builder.Build();
            request.LogResponseContent = true;

            // we need the url without the query to sign
            auth.RequestUrl = request.Url.SetQuery(null).FullUri;
            auth.Parameters = builder.QueryParams.ToDictionary(x => x.Key, x => x.Value);

            var header = GetAuthorizationHeader(auth);
            request.Headers.Add("Authorization", header);

            return _httpClient.Get(request);
        }

        private string GetAuthorizationHeader(OAuthRequest oAuthRequest)
        {
            var request = new Common.Http.HttpRequest(Settings.SigningUrl)
            {
                Method = HttpMethod.POST,
            };
            request.Headers.Set("Content-Type", "application/json");

            var payload = oAuthRequest.ToJson();
            _logger.Trace(payload);
            request.SetContent(payload);

            var response = _httpClient.Post<AuthorizationHeader>(request).Resource;

            return response.Authorization;
        }

        private NameValueCollection OAuthQuery(OAuthRequest oAuthRequest)
        {
            var auth = GetAuthorizationHeader(oAuthRequest);
            var request = new Common.Http.HttpRequest(oAuthRequest.RequestUrl);
            request.Headers.Add("Authorization", auth);
            var response = _httpClient.Get(request);

            return HttpUtility.ParseQueryString(response.Content);
        }

        private Tuple<string, string> GetUser()
        {
            var builder = RequestBuilder().SetSegment("route", $"api/auth_user");

            var httpResponse = OAuthGet(builder);

            string userId = null;
            string userName = null;

            var content = httpResponse.Content;

            if (!string.IsNullOrWhiteSpace(content))
            {
                var user = XDocument.Parse(content).XPathSelectElement("GoodreadsResponse/user");
                userId = user.AttributeAsString("id");
                userName = user.ElementAsString("name");
            }

            return Tuple.Create(userId, userName);
        }
    }

    public class AuthorizationHeader
    {
        public string Authorization { get; set; }
    }
}
