using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using RestSharp.Extensions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRssParser : RssParser
    {
        public const string ns = "{http://www.newznab.com/DTD/2010/feeds/attributes/}";

        private readonly NewznabSettings _settings;

        public NewznabRssParser(NewznabSettings settings)
        {
            PreferredEnclosureMimeType = "application/x-nzb";
            _settings = settings;
        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null) return true;

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

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            releaseInfo = base.ProcessItem(item, releaseInfo);
            releaseInfo.ImdbId = GetImdbId(item);


            //// This shouldn't be needed with changes to the DownloadDecisionMaker
            //var imdbMovieTitle = GetImdbTitle(item);
            //var imdbYear = GetImdbYear(item);

            //// Fun, lets try to add year to the releaseTitle for our foriegn friends :)
            //// if (!releaseInfo.Title.ContainsIgnoreCase(imdbMovieTitle + "." + imdbYear))
            //var isMatch = Regex.IsMatch(releaseInfo.Title, $@"^{imdbMovieTitle}.*{imdbYear}", RegexOptions.IgnoreCase);
            //if (!isMatch)
            //{
            //    if (imdbYear != 1900 && imdbMovieTitle != string.Empty)
            //    {
            //        // releaseInfo.Title = releaseInfo.Title.Replace(imdbMovieTitle, imdbMovieTitle + "." + imdbYear);
            //        releaseInfo.Title = Regex.Replace(releaseInfo.Title, imdbMovieTitle, imdbMovieTitle + "." + imdbYear, RegexOptions.IgnoreCase);
            //    }
            //}

            return releaseInfo;
        }

        protected override ReleaseInfo PostProcess(XElement item, ReleaseInfo releaseInfo)
        {
            var enclosureType = GetEnclosure(item).Attribute("type").Value;
            if (enclosureType.Contains("application/x-bittorrent"))
            {
                throw new UnsupportedFeedException("Feed contains {0}, did you intend to add a Torznab indexer?", enclosureType);
            }

            return base.PostProcess(item, releaseInfo);
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
                        Parser.Parser.NormalizeTitle(imdbTitle).Replace(" ", ".")
                    )
                 );
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
            var attr = item.Elements(ns + "attr").FirstOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (attr != null)
            {
                return attr.Attribute("value").Value;
            }

            return defaultValue;
        }
    }
}
