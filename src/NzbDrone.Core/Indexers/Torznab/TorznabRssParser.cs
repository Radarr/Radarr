﻿using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabRssParser : TorrentRssParser
    {
        public const string ns = "{http://torznab.com/schemas/2015/feed}";

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null) return true;

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199) throw new ApiKeyException("Invalid API key");

            if (!indexerResponse.Request.Url.FullUri.Contains("apikey=") && errorMessage == "Missing parameter")
            {
                throw new ApiKeyException("Indexer requires an API key");
            }

            if (errorMessage == "Request limit reached")
            {
                throw new RequestLimitReachedException("API limit reached");
            }

            throw new TorznabException("Torznab error detected: {0}", errorMessage);
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            var torrentInfo = base.ProcessItem(item, releaseInfo) as TorrentInfo;
            var imdbId = GetImdbId(item);
            int parsedImdbId;

            if (imdbId != null && int.TryParse(imdbId, out parsedImdbId))
            {
                torrentInfo.ImdbId = parsedImdbId;
            }

            torrentInfo.IndexerFlags = GetFlags(item);

            return torrentInfo;
        }

        protected override ReleaseInfo PostProcess(XElement item, ReleaseInfo releaseInfo)
        {
            var enclosureType = item.Element("enclosure").Attribute("type").Value;
            if (!enclosureType.Contains("application/x-bittorrent"))
            {
                throw new UnsupportedFeedException("Feed contains {0} instead of application/x-bittorrent", enclosureType);
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

            var sizeString = TryGetTorznabAttribute(item, "size");
            if (!sizeString.IsNullOrWhiteSpace() && long.TryParse(sizeString, out size))
            {
                return size;
            }

            size = GetEnclosureLength(item);

            return size;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
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

        protected virtual string GetImdbId(XElement item)
        {
            var imdbId = TryGetTorznabAttribute(item, "imdb");
            return (!imdbId.IsNullOrWhiteSpace() ? imdbId : null);
        }

        protected override string GetInfoHash(XElement item)
        {
            return TryGetTorznabAttribute(item, "infohash");
        }

        protected override string GetMagnetUrl(XElement item)
        {
            return TryGetTorznabAttribute(item, "magneturl");
        }

        protected override int? GetSeeders(XElement item)
        {
            var seeders = TryGetTorznabAttribute(item, "seeders");

            if (seeders.IsNotNullOrWhiteSpace())
            {
                return int.Parse(seeders);
            }

            return base.GetSeeders(item);
        }

        protected override int? GetPeers(XElement item)
        {
            var peers = TryGetTorznabAttribute(item, "peers");

            if (peers.IsNotNullOrWhiteSpace())
            {
                return int.Parse(peers);
            }

            var seeders = TryGetTorznabAttribute(item, "seeders");
            var leechers = TryGetTorznabAttribute(item, "leechers");

            if (seeders.IsNotNullOrWhiteSpace() && leechers.IsNotNullOrWhiteSpace())
            {
                return int.Parse(seeders) + int.Parse(leechers);
            }

            return base.GetPeers(item);
        }

		protected IndexerFlags GetFlags(XElement item)
		{
			IndexerFlags flags = 0;

			var downloadFactor = TryGetFloatTorznabAttribute(item, "downloadvolumefactor", 1);

            var uploadFactor = TryGetFloatTorznabAttribute(item, "uploadvolumefactor", 1);

            if (uploadFactor == 2)
            {
                flags |= IndexerFlags.G_DoubleUpload;
            }

            if (downloadFactor == 0.5)
            {
                flags |= IndexerFlags.G_Halfleech;
            }

            if (downloadFactor == 0.0)
            {
                flags |= IndexerFlags.G_Freeleech;
            }

            return flags;
		}

        protected string TryGetTorznabAttribute(XElement item, string key, string defaultValue = "")
        {
            var attr = item.Elements(ns + "attr").FirstOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (attr != null)
            {
                return attr.Attribute("value").Value;
            }

            return defaultValue;
        }

		protected float TryGetFloatTorznabAttribute(XElement item, string key, float defaultValue = 0)
        {
            var attr = TryGetTorznabAttribute(item, key, defaultValue.ToString());

            float result = 0;

            if (float.TryParse(attr, out result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}
