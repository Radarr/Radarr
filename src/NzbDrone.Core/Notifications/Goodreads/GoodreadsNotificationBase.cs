using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.OAuth;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.Goodreads;
using NzbDrone.Core.MetadataSource.Goodreads;

namespace NzbDrone.Core.Notifications.Goodreads
{
    public abstract class GoodreadsNotificationBase<TSettings> : NotificationBase<TSettings>
    where TSettings : GoodreadsSettingsBase<TSettings>, new()
    {
        protected readonly IHttpClient _httpClient;
        protected readonly Logger _logger;

        protected GoodreadsNotificationBase(IHttpClient httpClient,
                                            Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public override string Link => "https://goodreads.com/";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestConnection());

            return new ValidationResult(failures);
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

        protected HttpRequestBuilder RequestBuilder()
        {
            return new HttpRequestBuilder("https://www.goodreads.com/{route}").KeepAlive();
        }

        protected Common.Http.HttpResponse OAuthExecute(HttpRequestBuilder builder)
        {
            var auth = OAuthRequest.ForProtectedResource(builder.Method.ToString(), null, null, Settings.AccessToken, Settings.AccessTokenSecret);

            var request = builder.Build();
            request.LogResponseContent = true;

            // we need the url without the query to sign
            auth.RequestUrl = request.Url.SetQuery(null).FullUri;

            if (builder.Method == HttpMethod.GET)
            {
                auth.Parameters = builder.QueryParams.ToDictionary(x => x.Key, x => x.Value);
            }
            else if (builder.Method == HttpMethod.POST)
            {
                auth.Parameters = builder.FormData.ToDictionary(x => x.Name, x => Encoding.UTF8.GetString(x.ContentData));
            }

            var header = GetAuthorizationHeader(auth);
            request.Headers.Add("Authorization", header);

            return _httpClient.Execute(request);
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

                return new ValidationFailure(string.Empty, "Unable to connect to Goodreads, check the log for more details");
            }
        }

        private Tuple<string, string> GetUser()
        {
            var builder = RequestBuilder().SetSegment("route", "api/auth_user");

            var httpResponse = OAuthExecute(builder);

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
    }
}
