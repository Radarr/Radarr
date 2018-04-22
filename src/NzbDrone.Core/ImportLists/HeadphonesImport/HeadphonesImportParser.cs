using Newtonsoft.Json;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.ImportLists.HeadphonesImport
{
    public class HeadphonesImportParser : IParseImportListResponse
    {
        private ImportListResponse _importListResponse;

        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            _importListResponse = importListResponse;

            var items = new List<ImportListItemInfo>();

            if (!PreProcess(_importListResponse))
            {
                return items;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<HeadphonesImportArtist>>(_importListResponse.Content);

            // no albums were return
            if (jsonResponse == null)
            {
                return items;
            }

            foreach (var item in jsonResponse)
            {
                items.AddIfNotNull(new ImportListItemInfo
                {
                    Artist = item.ArtistName,
                    ArtistMusicBrainzId = item.ArtistId
                });
            }

            return items;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Import List API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(importListResponse, "Import List responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

    }
}
