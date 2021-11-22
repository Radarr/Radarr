using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TinyTwitter
{
    public class OAuthInfo
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
    }

    public class Tweet
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public string ScreenName { get; set; }
        public string Text { get; set; }
    }

    public class TinyTwitter
    {
        private readonly OAuthInfo _oauth;

        public TinyTwitter(OAuthInfo oauth)
        {
            _oauth = oauth;
        }

        public void UpdateStatus(string message)
        {
            new RequestBuilder(_oauth, HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update.json")
                .AddParameter("status", message)
                .Execute();
        }

        /**
         *
         * As of June 26th 2015 Direct Messaging is not part of TinyTwitter.
         * I have added it to Sonarr's copy to make our implementation easier
         * and added this banner so it's not blindly updated.
         *
         **/
        public void DirectMessage(string message, string screenName)
        {
            new RequestBuilder(_oauth, HttpMethod.Post, "https://api.twitter.com/1.1/direct_messages/new.json")
                .AddParameter("text", message)
                .AddParameter("screen_name", screenName)
                .Execute();
        }

        public class RequestBuilder
        {
            private const string VERSION = "1.0";
            private const string SIGNATURE_METHOD = "HMAC-SHA1";

            private readonly OAuthInfo _oauth;
            private readonly HttpMethod _method;
            private readonly IDictionary<string, string> _customParameters;
            private readonly string _url;
            private readonly HttpClient _httpClient;

            public RequestBuilder(OAuthInfo oauth, HttpMethod method, string url)
            {
                _oauth = oauth;
                _method = method;
                _url = url;
                _customParameters = new Dictionary<string, string>();
                _httpClient = new ();
            }

            public RequestBuilder AddParameter(string name, string value)
            {
                _customParameters.Add(name, value.EncodeRFC3986());
                return this;
            }

            public string Execute()
            {
                var timespan = GetTimestamp();
                var nonce = CreateNonce();

                var parameters = new Dictionary<string, string>(_customParameters);
                AddOAuthParameters(parameters, timespan, nonce);

                var signature = GenerateSignature(parameters);
                var headerValue = GenerateAuthorizationHeaderValue(parameters, signature);

                var request = new HttpRequestMessage(_method, _url);
                request.Content = new FormUrlEncodedContent(_customParameters);

                request.Headers.Add("Authorization", headerValue);

                var response = _httpClient.Send(request);
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            private string GenerateAuthorizationHeaderValue(IEnumerable<KeyValuePair<string, string>> parameters, string signature)
            {
                return new StringBuilder("OAuth ")
                    .Append(parameters.Concat(new KeyValuePair<string, string>("oauth_signature", signature))
                                .Where(x => x.Key.StartsWith("oauth_"))
                                .Select(x => string.Format("{0}=\"{1}\"", x.Key, x.Value.EncodeRFC3986()))
                                .Join(","))
                    .ToString();
            }

            private string GenerateSignature(IEnumerable<KeyValuePair<string, string>> parameters)
            {
                var dataToSign = new StringBuilder()
                    .Append(_method).Append('&')
                    .Append(_url.EncodeRFC3986()).Append('&')
                    .Append(parameters
                                .OrderBy(x => x.Key)
                                .Select(x => string.Format("{0}={1}", x.Key, x.Value))
                                .Join("&")
                                .EncodeRFC3986());

                var signatureKey = string.Format("{0}&{1}", _oauth.ConsumerSecret.EncodeRFC3986(), _oauth.AccessSecret.EncodeRFC3986());
                var sha1 = new HMACSHA1(Encoding.ASCII.GetBytes(signatureKey));

                var signatureBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(dataToSign.ToString()));
                return Convert.ToBase64String(signatureBytes);
            }

            private void AddOAuthParameters(IDictionary<string, string> parameters, string timestamp, string nonce)
            {
                parameters.Add("oauth_version", VERSION);
                parameters.Add("oauth_consumer_key", _oauth.ConsumerKey);
                parameters.Add("oauth_nonce", nonce);
                parameters.Add("oauth_signature_method", SIGNATURE_METHOD);
                parameters.Add("oauth_timestamp", timestamp);
                parameters.Add("oauth_token", _oauth.AccessToken);
            }

            private static string GetTimestamp()
            {
                return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            }

            private static string CreateNonce()
            {
                return new Random().Next(0x0000000, 0x7fffffff).ToString("X8");
            }
        }
    }

    public static class TinyTwitterHelperExtensions
    {
        public static string Join<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items.ToArray());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T value)
        {
            return items.Concat(new[] { value });
        }

        public static string EncodeRFC3986(this string value)
        {
            // From Twitterizer http://www.twitterizer.net/
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var encoded = Uri.EscapeDataString(value);

            return Regex
                .Replace(encoded, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper())
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("$", "%24")
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27")
                .Replace("%7E", "~");
        }
    }
}
