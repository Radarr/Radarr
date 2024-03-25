using System.Collections.Generic;
using System.Linq;
using System.Net;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.PassThePopcorn.Collection;

public class PassThePopcornCollectionParser : IParseImportListResponse
{
    public IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse)
    {
        if (!STJson.TryDeserialize<PassThePopcornCollectionResponse>(importListResponse.Content, out var jsonResponse))
        {
            throw new ImportListException(importListResponse, "List responded with invalid JSON content. Site is likely blocked or unavailable.");
        }

        return jsonResponse.CoverView
            .Movies
            .Select(item => new ImportListMovie
            {
                ImdbId = item.ImdbId,
                Title = WebUtility.HtmlDecode(item.Title),
                Year = int.Parse(item.Year)
            })
            .ToList();
    }
}
