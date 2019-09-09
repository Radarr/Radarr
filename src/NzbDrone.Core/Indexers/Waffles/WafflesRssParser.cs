using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;


namespace NzbDrone.Core.Indexers.Waffles
{
    public class WafflesRssParser : TorrentRssParser 
    {
        public const string ns = "{http://purl.org/rss/1.0/}";
        public const string dc = "{http://purl.org/dc/elements/1.1/}";

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null) return true;

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199) throw new ApiKeyException("Invalid Pass key");

            if (!indexerResponse.Request.Url.FullUri.Contains("passkey=") && errorMessage == "Missing parameter")
            {
                throw new ApiKeyException("Indexer requires an Pass key");
            }

            if (errorMessage == "Request limit reached")
            {
                throw new RequestLimitReachedException("API limit reached");
            }

            throw new IndexerException(indexerResponse, errorMessage);
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            var torrentInfo = base.ProcessItem(item, releaseInfo) as TorrentInfo;

            return torrentInfo;
        }

        protected override string GetInfoUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments").TrimEnd("#comments"));
        }

        protected override string GetCommentUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments"));
        }

         private static readonly Regex ParseSizeRegex = new Regex(@"(?:Size: )(?<value>\d+)<",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override long GetSize(XElement item)
        {
            var match = ParseSizeRegex.Matches(item.Element("description").Value);

            if (match.Count != 0)
            {
                var value = decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), CultureInfo.InvariantCulture);
                return (long)value;
            }

            return 0;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue(dc + "date");

            if (dateString.IsNullOrWhiteSpace())
            {
                throw new UnsupportedFeedException("Rss feed must have a pubDate element with a valid publish date.");
            }

            return XElementExtensions.ParseDate(dateString);
        }
    }
}
