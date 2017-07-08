using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AlbumSearchCriteria : SearchCriteriaBase
    {
        public override string ToString()
        {
            var baseRepresentation = $"[{Artist.Name} - {Album.Title}]";
            if (Album.ReleaseDate.HasValue)
            {
                var beforeLast = baseRepresentation.Length - 1;
                return baseRepresentation.Insert(beforeLast, $" ({Album.ReleaseDate.Value.Year})");
            }
            else
            {
                return baseRepresentation;
            }
        }
    }
}
