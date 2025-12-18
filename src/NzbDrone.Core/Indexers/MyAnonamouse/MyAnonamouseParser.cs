using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.MyAnonamouse
{
    public class MyAnonamouseParser : IParseIndexerResponse
    {
        private readonly MyAnonamouseSettings _settings;

        public MyAnonamouseParser(MyAnonamouseSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new IndexerException(indexerResponse, "[403 Forbidden] - mam_id expired or invalid");
            }

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            var jsonResponse = JsonConvert.DeserializeObject<MyAnonamouseResponse>(indexerResponse.Content);

            if (jsonResponse.Error.IsNotNullOrWhiteSpace() && jsonResponse.Error.StartsWithIgnoreCase("Nothing returned, out of"))
            {
                return torrentInfos;
            }

            if (jsonResponse.Data == null)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response content: {0}",
                    jsonResponse.Message ?? "Check the logs for more information");
            }

            foreach (var item in jsonResponse.Data)
            {
                var id = item.Id;
                var title = item.Title;

                if (item.AuthorInfo != null)
                {
                    try
                    {
                        var authorInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.AuthorInfo);
                        var author = authorInfo?.Take(5).Select(v => v.Value).Join(", ");

                        if (author.IsNotNullOrWhiteSpace())
                        {
                            title += " by " + author;
                        }
                    }
                    catch
                    {
                    }
                }

                var flags = new List<string>();

                if (item.LanguageCode.IsNotNullOrWhiteSpace())
                {
                    flags.Add(item.LanguageCode);
                }

                if (item.Filetype.IsNotNullOrWhiteSpace())
                {
                    flags.Add(item.Filetype.ToUpper());
                }

                if (flags.Count > 0)
                {
                    title += " [" + flags.Join(" / ") + "]";
                }

                if (item.Vip)
                {
                    title += " [VIP]";
                }

                var isFreeLeech = item.Free || item.PersonalFreeLeech || item.FreeVip;

                torrentInfos.Add(new TorrentInfo
                {
                    Guid = $"MyAnonamouse-{id}",
                    Title = title,
                    Size = ParseSize(item.Size),
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = item.Seeders,
                    Peers = item.Leechers + item.Seeders,
                    PublishDate = ParseDate(item.Added),
                    IndexerFlags = GetIndexerFlags(isFreeLeech)
                });
            }

            return torrentInfos;
        }

        private static IndexerFlags GetIndexerFlags(bool isFreeLeech)
        {
            IndexerFlags flags = 0;

            if (isFreeLeech)
            {
                flags |= IndexerFlags.G_Freeleech;
            }

            return flags;
        }

        private static long ParseSize(string sizeString)
        {
            if (sizeString.IsNullOrWhiteSpace())
            {
                return 0;
            }

            if (long.TryParse(sizeString, out var size))
            {
                return size;
            }

            var parts = sizeString.Trim().Split(' ');
            if (parts.Length != 2 || !double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return 0;
            }

            var unit = parts[1].ToUpperInvariant();
            return unit switch
            {
                "B" => (long)value,
                "KB" => (long)(value * 1024),
                "MB" => (long)(value * 1024 * 1024),
                "GB" => (long)(value * 1024 * 1024 * 1024),
                "TB" => (long)(value * 1024 * 1024 * 1024 * 1024),
                _ => 0
            };
        }

        private static DateTime ParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return date.ToLocalTime();
            }

            return DateTime.UtcNow;
        }

        private string GetDownloadUrl(int torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/tor/download.php")
                .AddQueryParam("tid", torrentId);

            return url.FullUri;
        }

        private string GetInfoUrl(int torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/t/")
                .CombinePath(torrentId.ToString());

            return url.FullUri;
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
