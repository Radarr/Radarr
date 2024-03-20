using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.PassThePopcorn;

internal class CollectionPage
{
    [JsonProperty("pages")]
    public string Pages { get; set; }

    public CoverView CoverView { get; set; }
}

internal class CoverView
{
    public List<CollectionMovie> Movies { get; set; }
}

internal class CollectionMovie
{
    public string Title { get; set; }
    public string ImdbId { get; set; }
}
