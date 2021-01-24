using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.LazyLibrarianImport
{
    public class LazyLibrarianImportParser : IParseImportListResponse
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

            var jsonResponse = JsonConvert.DeserializeObject<List<LazyLibrarianBook>>(_importListResponse.Content);

            // no books were return
            if (jsonResponse == null)
            {
                return items;
            }

            foreach (var item in jsonResponse)
            {
                items.AddIfNotNull(new ImportListItemInfo
                {
                    Author = item.AuthorName,
                    Book = item.BookName,
                    EditionGoodreadsId = item.BookId
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
