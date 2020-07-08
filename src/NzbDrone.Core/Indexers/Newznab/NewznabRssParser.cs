using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRssParser : RssParser
    {
        public const string ns = "{http://www.newznab.com/DTD/2010/feeds/attributes/}";

        private readonly NewznabSettings _settings;

        public NewznabRssParser(NewznabSettings settings)
        {
            PreferredEnclosureMimeTypes = UsenetEnclosureMimeTypes;
            UseEnclosureUrl = true;
            _settings = settings;
        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null)
            {
                return true;
            }

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199)
            {
                _logger.Warn("Invalid API Key: {0}", errorMessage);
                throw new ApiKeyException("Invalid API key");
            }

            if (!indexerResponse.Request.Url.FullUri.Contains("apikey=") && (errorMessage == "Missing parameter" || errorMessage.Contains("apikey")))
            {
                throw new ApiKeyException("Indexer requires an API key");
            }

            if (errorMessage == "Request limit reached")
            {
                throw new RequestLimitReachedException("API limit reached");
            }

            throw new NewznabException(indexerResponse, errorMessage);
        }

        protected override bool PostProcess(IndexerResponse indexerResponse, List<XElement> items, List<ReleaseInfo> releases)
        {
            var enclosureTypes = items.SelectMany(GetEnclosures).Select(v => v.Type).Distinct().ToArray();
            if (enclosureTypes.Any() && enclosureTypes.Intersect(PreferredEnclosureMimeTypes).Empty())
            {
                if (enclosureTypes.Intersect(TorrentEnclosureMimeTypes).Any())
                {
                    _logger.Warn("Feed does not contain {0}, found {1}, did you intend to add a Torznab indexer?", NzbEnclosureMimeType, enclosureTypes[0]);
                }
                else
                {
                    _logger.Warn("Feed does not contain {0}, found {1}.", NzbEnclosureMimeType, enclosureTypes[0]);
                }
            }

            return true;
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            releaseInfo = base.ProcessItem(item, releaseInfo);
            releaseInfo.ImdbId = GetImdbId(item);

            return releaseInfo;
        }

        protected override string GetInfoUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments").TrimEnd("#comments"));
        }

        protected override string GetCommentUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments"));
        }

        protected override long GetSize(XElement item)
        {
            long size;

            var sizeString = TryGetNewznabAttribute(item, "size");
            if (!sizeString.IsNullOrWhiteSpace() && long.TryParse(sizeString, out size))
            {
                return size;
            }

            size = GetEnclosureLength(item);

            return size;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = TryGetNewznabAttribute(item, "usenetdate");
            if (!dateString.IsNullOrWhiteSpace())
            {
                return XElementExtensions.ParseDate(dateString);
            }

            return base.GetPublishDate(item);
        }

        protected override string GetDownloadUrl(XElement item)
        {
            var url = base.GetDownloadUrl(item);

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                url = ParseUrl((string)item.Element("enclosure").Attribute("url"));
            }

            return url;
        }

        protected virtual int GetImdbId(XElement item)
        {
            var imdbIdString = TryGetNewznabAttribute(item, "imdb");
            int imdbId;

            if (!imdbIdString.IsNullOrWhiteSpace() && int.TryParse(imdbIdString, out imdbId))
            {
                return imdbId;
            }

            return 0;
        }

        protected virtual string GetImdbTitle(XElement item)
        {
            var imdbTitle = TryGetNewznabAttribute(item, "imdbtitle");
            if (!imdbTitle.IsNullOrWhiteSpace())
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    Parser.Parser.ReplaceGermanUmlauts(
                        Parser.Parser.NormalizeTitle(imdbTitle).Replace(" ", ".")));
            }

            return string.Empty;
        }

        protected virtual int GetImdbYear(XElement item)
        {
            var imdbYearString = TryGetNewznabAttribute(item, "imdbyear");
            int imdbYear;

            if (!imdbYearString.IsNullOrWhiteSpace() && int.TryParse(imdbYearString, out imdbYear))
            {
                return imdbYear;
            }

            return 1900;
        }

        protected string TryGetNewznabAttribute(XElement item, string key, string defaultValue = "")
        {
            var attrElement = item.Elements(ns + "attr").FirstOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (attrElement != null)
            {
                var attrValue = attrElement.Attribute("value");
                if (attrValue != null)
                {
                    return attrValue.Value;
                }
            }

            return defaultValue;
        }
    }
}
