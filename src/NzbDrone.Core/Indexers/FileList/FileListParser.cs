using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListParser : IParseIndexerResponse
    {
        private readonly FileListSettings _settings;

        public FileListParser(FileListSettings settings)
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

            var queryResults = JsonConvert.DeserializeObject<List<FileListTorrent>>(indexerResponse.Content);

            foreach (var result in queryResults)
            {
                var id = result.Id;

                IndexerFlags flags = 0;

                if (result.FreeLeech)
                {
                    flags |= IndexerFlags.G_Freeleech;
                }

                if (result.Internal)
                {
                    flags |= IndexerFlags.G_Internal;
                }

                var imdbId = 0;
                if (result.ImdbId != null && result.ImdbId.Length > 2)
                {
                    imdbId = int.Parse(result.ImdbId.Substring(2));
                }

                torrentInfos.Add(new TorrentInfo
                {
                    Guid = $"FileList-{id}",
                    Title = result.Name,
                    Size = result.Size,
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = result.UploadDate,
                    ImdbId = imdbId,
                    IndexerFlags = flags
                });
            }

            return torrentInfos.ToArray();
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private string GetDownloadUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/download.php")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("passkey", _settings.Passkey);

            return url.FullUri;
        }

        private string GetInfoUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/details.php")
                .AddQueryParam("id", torrentId);

            return url.FullUri;
        }
    }
}
