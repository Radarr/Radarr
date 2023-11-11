using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.PassThePopcorn.Collection;

internal class PassThePopcornCollectionResponse
{
    public PassThePopcornCoverView CoverView { get; set; }
}

internal class PassThePopcornCoverView
{
    public IList<PassThePopcornCollectionMovie> Movies { get; set; }
}

internal class PassThePopcornCollectionMovie
{
    public string Title { get; set; }
    public string Year { get; set; }
    public string ImdbId { get; set; }
}
