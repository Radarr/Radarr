using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.ImportLists.RSSImport
{
    public class RSSImportParser : IParseImportListResponse
    {
        private readonly RSSImportSettings _settings;
        private readonly Logger _logger;
        private ImportListResponse _importResponse;

        private static readonly Regex ReplaceEntities = new Regex("&[a-z]+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public RSSImportParser(RSSImportSettings settings,
                               Logger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var document = LoadXmlDocument(importResponse);
            var items = GetItems(document);

            foreach (var item in items)
            {
                try
                {
                    var reportInfo = ProcessItem(item);

                    movies.AddIfNotNull(reportInfo);
                }
                catch (Exception itemEx)
                {
                    //itemEx.Data.Add("Item", item.Title());
                    _logger.Error(itemEx, "An error occurred while processing list feed item from {0}", importResponse.Request.Url);
                }
            }

            return movies;
        }

        protected virtual XDocument LoadXmlDocument(ImportListResponse indexerResponse)
        {
            try
            {
                var content = indexerResponse.Content;
                content = ReplaceEntities.Replace(content, ReplaceEntity);

                using (var xmlTextReader = XmlReader.Create(new StringReader(content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
                {
                    return XDocument.Load(xmlTextReader);
                }
            }
            catch (XmlException ex)
            {
                var contentSample = indexerResponse.Content.Substring(0, Math.Min(indexerResponse.Content.Length, 512));
                _logger.Debug("Truncated response content (originally {0} characters): {1}", indexerResponse.Content.Length, contentSample);

                ex.Data.Add("ContentLength", indexerResponse.Content.Length);
                ex.Data.Add("ContentSample", contentSample);

                throw;
            }
        }

        protected virtual string ReplaceEntity(Match match)
        {
            try
            {
                var character = WebUtility.HtmlDecode(match.Value);
                return string.Concat("&#", (int)character[0], ";");
            }
            catch
            {
                return match.Value;
            }
        }

        protected virtual ImportListMovie CreateNewMovie()
        {
            return new ImportListMovie();
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "List API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/html") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/html"))
            {
                throw new ImportListException(importListResponse, "List responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

        protected ImportListMovie ProcessItem(XElement item)
        {
            var releaseInfo = CreateNewMovie();

            releaseInfo = ProcessItem(item, releaseInfo);

            //_logger.Trace("Parsed: {0}", releaseInfo.Title);
            return PostProcess(item, releaseInfo);
        }

        protected virtual ImportListMovie ProcessItem(XElement item, ImportListMovie releaseInfo)
        {
            var title = GetTitle(item);

            // Loosely allow movies (will work with IMDB)
            if (title.ContainsIgnoreCase("TV Series") || title.ContainsIgnoreCase("Mini-Series") || title.ContainsIgnoreCase("TV Episode"))
            {
                return null;
            }

            releaseInfo.Title = title;
            var result = Parser.Parser.ParseMovieTitle(title); //Depreciated anyways

            if (result != null)
            {
                releaseInfo.Title = result.PrimaryMovieTitle;
                releaseInfo.Year = result.Year;
                releaseInfo.ImdbId = result.ImdbId;
            }

            try
            {
                if (releaseInfo.ImdbId.IsNullOrWhiteSpace())
                {
                    releaseInfo.ImdbId = GetImdbId(item);
                }
            }
            catch (Exception)
            {
                _logger.Debug("Unable to extract Imdb Id :(.");
            }

            return releaseInfo;
        }

        protected virtual ImportListMovie PostProcess(XElement item, ImportListMovie releaseInfo)
        {
            return releaseInfo;
        }

        protected virtual string GetTitle(XElement item)
        {
            return item.TryGetValue("title", "Unknown");
        }

        protected virtual DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue("pubDate");

            if (dateString.IsNullOrWhiteSpace())
            {
                throw new UnsupportedFeedException("Rss feed must have a pubDate element with a valid publish date.");
            }

            return XElementExtensions.ParseDate(dateString);
        }

        protected virtual string GetImdbId(XElement item)
        {
            var url = item.TryGetValue("link");
            if (url.IsNullOrWhiteSpace())
            {
                return "";
            }

            return Parser.Parser.ParseImdbId(url);
        }

        protected IEnumerable<XElement> GetItems(XDocument document)
        {
            var root = document.Root;

            if (root == null)
            {
                return Enumerable.Empty<XElement>();
            }

            var channel = root.Element("channel");

            if (channel == null)
            {
                return Enumerable.Empty<XElement>();
            }

            return channel.Elements("item");
        }

        protected virtual string ParseUrl(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                var url = _importResponse.HttpRequest.Url + new HttpUri(value);

                return url.FullUri;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, string.Format("Failed to parse Url {0}, ignoring.", value));
                return null;
            }
        }
    }
}
