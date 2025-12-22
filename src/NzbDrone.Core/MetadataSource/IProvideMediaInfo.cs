using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMediaInfo<TMetadata>
        where TMetadata : class
    {
        TMetadata GetByExternalId(string externalId);
        TMetadata GetById(int providerId);
        List<TMetadata> GetBulkInfo(List<int> providerIds);
        List<TMetadata> GetTrending();
        List<TMetadata> GetPopular();
        HashSet<int> GetChangedItems(DateTime startTime);
    }

    public interface ISearchableMediaProvider<TMetadata>
        where TMetadata : class
    {
        List<TMetadata> SearchByTitle(string title);
        List<TMetadata> SearchByTitle(string title, int year);
    }
}
