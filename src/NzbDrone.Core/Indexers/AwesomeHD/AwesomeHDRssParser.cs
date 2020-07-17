using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.AwesomeHD
{
    public class AwesomeHDRssParser : IParseIndexerResponse
    {
        private readonly AwesomeHDSettings _settings;

        public AwesomeHDRssParser(AwesomeHDSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            try
            {
                var xdoc = XDocument.Parse(indexerResponse.Content);
                var searchResults = xdoc.Descendants("searchresults").Select(x => new
                {
                    AuthKey = x.Element("authkey").Value,
                }).FirstOrDefault();

                var torrents = xdoc.Descendants("torrent")
                .Select(x => new
                {
                    Id = x.Element("id").Value,
                    Name = x.Element("name").Value,
                    Year = x.Element("year").Value,
                    GroupId = x.Element("groupid").Value,
                    Time = DateTime.Parse(x.Element("time").Value),
                    UserId = x.Element("userid").Value,
                    Size = long.Parse(x.Element("size").Value),
                    Snatched = x.Element("snatched").Value,
                    Seeders = x.Element("seeders").Value,
                    Leechers = x.Element("leechers").Value,
                    ReleaseGroup = x.Element("releasegroup").Value,
                    Resolution = x.Element("resolution").Value,
                    Media = x.Element("media").Value,
                    Format = x.Element("format").Value,
                    Encoding = x.Element("encoding").Value,
                    AudioFormat = x.Element("audioformat").Value,
                    AudioBitrate = x.Element("audiobitrate").Value,
                    AudioChannels = x.Element("audiochannels").Value,
                    Subtitles = x.Element("subtitles").Value,
                    EncodeStatus = x.Element("encodestatus").Value,
                    Freeleech = x.Element("freeleech").Value,
                    Internal = x.Element("internal").Value == "1",
                    UserRelease = x.Element("userrelease").Value == "1",
                    ImdbId = x.Element("imdb").Value
                }).ToList();

                foreach (var torrent in torrents)
                {
                    var id = torrent.Id;

                    var title = $"{torrent.Name}.{torrent.Year}.{torrent.Resolution}.{torrent.Media}.{torrent.Encoding}.{torrent.AudioFormat}-{torrent.ReleaseGroup}";

                    if (torrent.Encoding.ToLower() == "x265")
                    {
                        //Per AHD staff they only allow HDR x265 encodes (https://github.com/Radarr/Radarr/issues/4386)
                        title = $"{torrent.Name}.{torrent.Year}.{torrent.Resolution}.{torrent.Media}.HDR.{torrent.Encoding}.{torrent.AudioFormat}-{torrent.ReleaseGroup}";
                    }

                    IndexerFlags flags = 0;

                    if (torrent.Freeleech == "0.00")
                    {
                        flags |= IndexerFlags.G_Freeleech;
                    }

                    if (torrent.Freeleech == "0.25")
                    {
                        flags |= IndexerFlags.G_Freeleech75;
                    }

                    if (torrent.Freeleech == "0.75")
                    {
                        flags |= IndexerFlags.G_Freeleech25;
                    }

                    if (torrent.Freeleech == "0.50")
                    {
                        flags |= IndexerFlags.G_Halfleech;
                    }

                    if (torrent.Internal)
                    {
                        flags |= IndexerFlags.AHD_Internal;
                    }

                    if (torrent.UserRelease)
                    {
                        flags |= IndexerFlags.AHD_UserRelease;
                    }

                    var imdbId = 0;
                    if (torrent.ImdbId.Length > 2)
                    {
                        imdbId = int.Parse(torrent.ImdbId.Substring(2));
                    }

                    torrentInfos.Add(new TorrentInfo()
                    {
                        Guid = string.Format("AwesomeHD-{0}", id),
                        Title = title,
                        Size = torrent.Size,
                        DownloadUrl = GetDownloadUrl(id, searchResults.AuthKey, _settings.Passkey),
                        InfoUrl = GetInfoUrl(torrent.GroupId, id),
                        Seeders = int.Parse(torrent.Seeders),
                        Peers = int.Parse(torrent.Leechers) + int.Parse(torrent.Seeders),
                        PublishDate = torrent.Time.ToUniversalTime(),
                        ImdbId = imdbId,
                        IndexerFlags = flags,
                    });
                }
            }
            catch (XmlException)
            {
                throw new IndexerException(indexerResponse,
                   "An error occurred while processing feed, feed invalid");
            }

            return torrentInfos.OrderByDescending(o => ((dynamic)o).Seeders).ToArray();
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private string GetDownloadUrl(string torrentId, string authKey, string passKey)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("action", "download")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("authkey", authKey)
                .AddQueryParam("torrent_pass", passKey);

            return url.FullUri;
        }

        private string GetInfoUrl(string groupId, string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/torrents.php")
                .AddQueryParam("id", groupId)
                .AddQueryParam("torrentid", torrentId);

            return url.FullUri;
        }
    }
}
