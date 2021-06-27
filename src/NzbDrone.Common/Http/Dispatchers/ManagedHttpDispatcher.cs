using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class ManagedHttpDispatcher : IHttpDispatcher
    {
        private readonly IHttpProxySettingsProvider _proxySettingsProvider;
        private readonly ICreateManagedWebProxy _createManagedWebProxy;
        private readonly IUserAgentBuilder _userAgentBuilder;
        private readonly IPlatformInfo _platformInfo;
        private readonly Logger _logger;

        public ManagedHttpDispatcher(IHttpProxySettingsProvider proxySettingsProvider, ICreateManagedWebProxy createManagedWebProxy, IUserAgentBuilder userAgentBuilder, IPlatformInfo platformInfo, Logger logger)
        {
            _proxySettingsProvider = proxySettingsProvider;
            _createManagedWebProxy = createManagedWebProxy;
            _userAgentBuilder = userAgentBuilder;
            _platformInfo = platformInfo;
            _logger = logger;
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create((Uri)request.Url);

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Method = request.Method.ToString();
            webRequest.UserAgent = _userAgentBuilder.GetUserAgent(request.UseSimplifiedUserAgent);
            webRequest.KeepAlive = request.ConnectionKeepAlive;
            webRequest.AllowAutoRedirect = false;
            webRequest.CookieContainer = cookies;

            if (request.RequestTimeout != TimeSpan.Zero)
            {
                webRequest.Timeout = (int)Math.Ceiling(request.RequestTimeout.TotalMilliseconds);
            }

            webRequest.Proxy = GetProxy(request.Url);

            if (request.Headers != null)
            {
                AddRequestHeaders(webRequest, request.Headers);
            }

            HttpWebResponse httpWebResponse;

            try
            {
                if (request.ContentData != null)
                {
                    webRequest.ContentLength = request.ContentData.Length;
                    using (var writeStream = webRequest.GetRequestStream())
                    {
                        writeStream.Write(request.ContentData, 0, request.ContentData.Length);
                    }
                }

                httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                httpWebResponse = (HttpWebResponse)e.Response;

                if (httpWebResponse == null)
                {
                    // The default messages for WebException on mono are pretty horrible.
                    if (e.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        throw new WebException($"DNS Name Resolution Failure: '{webRequest.RequestUri.Host}'", e.Status);
                    }
                    else if (e.ToString().Contains("TLS Support not"))
                    {
                        throw new TlsFailureException(webRequest, e);
                    }
                    else if (e.ToString().Contains("The authentication or decryption has failed."))
                    {
                        throw new TlsFailureException(webRequest, e);
                    }
                    else if (OsInfo.IsNotWindows)
                    {
                        throw new WebException($"{e.Message}: '{webRequest.RequestUri}'", e, e.Status, e.Response);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            byte[] data = null;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null && responseStream != Stream.Null)
                {
                    try
                    {
                        data = responseStream.ToBytes();
                    }
                    catch (Exception ex)
                    {
                        throw new WebException("Failed to read complete http response", ex, WebExceptionStatus.ReceiveFailure, httpWebResponse);
                    }
                }
            }

            return new HttpResponse(request, new HttpHeader(httpWebResponse.Headers), data, httpWebResponse.StatusCode);
        }

        public void DownloadFile(string url, string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                _logger.Debug("Downloading [{0}] to [{1}]", url, fileName);

                var stopWatch = Stopwatch.StartNew();
                var uri = new HttpUri(url);

                using (var webClient = new GZipWebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, _userAgentBuilder.GetUserAgent());
                    webClient.Proxy = GetProxy(uri);
                    webClient.DownloadFile(uri.FullUri, fileName);
                    stopWatch.Stop();
                    _logger.Debug("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
                }
            }
            catch (WebException e)
            {
                _logger.Warn("Failed to get response from: {0} {1}", url, e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Failed to get response from: " + url);
                throw;
            }
        }

        protected virtual IWebProxy GetProxy(HttpUri uri)
        {
            IWebProxy proxy = null;

            var proxySettings = _proxySettingsProvider.GetProxySettings(uri);

            if (proxySettings != null)
            {
                proxy = _createManagedWebProxy.GetWebProxy(proxySettings);
            }

            return proxy;
        }

        protected virtual void AddRequestHeaders(HttpWebRequest webRequest, HttpHeader headers)
        {
            foreach (var header in headers)
            {
                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Accept = header.Value;
                        break;
                    case "Connection":
                        webRequest.Connection = header.Value;
                        break;
                    case "Content-Length":
                        webRequest.ContentLength = Convert.ToInt64(header.Value);
                        break;
                    case "Content-Type":
                        webRequest.ContentType = header.Value;
                        break;
                    case "Date":
                        webRequest.Date = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Expect":
                        webRequest.Expect = header.Value;
                        break;
                    case "Host":
                        webRequest.Host = header.Value;
                        break;
                    case "If-Modified-Since":
                        webRequest.IfModifiedSince = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Range":
                        throw new NotImplementedException();
                    case "Referer":
                        webRequest.Referer = header.Value;
                        break;
                    case "Transfer-Encoding":
                        webRequest.TransferEncoding = header.Value;
                        break;
                    case "User-Agent":
                        throw new NotSupportedException("User-Agent other than Radarr not allowed.");
                    case "Proxy-Connection":
                        throw new NotImplementedException();
                    default:
                        webRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }
    }
}
