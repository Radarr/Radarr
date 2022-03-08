using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(CollectionEditedEvent), CheckOnCondition.Always)]
    public class MovieCollectionRootFolderCheck : HealthCheckBase
    {
        private readonly IMovieCollectionService _collectionService;
        private readonly IDiskProvider _diskProvider;

        public MovieCollectionRootFolderCheck(IMovieCollectionService collectionService, IDiskProvider diskProvider, ILocalizationService localizationService)
            : base(localizationService)
        {
            _collectionService = collectionService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var collections = _collectionService.GetAllCollections();
            var missingRootFolders = new Dictionary<string, List<MovieCollection>>();

            foreach (var collection in collections)
            {
                var rootFolderPath = collection.RootFolderPath;

                if (missingRootFolders.ContainsKey(rootFolderPath))
                {
                    missingRootFolders[rootFolderPath].Add(collection);

                    continue;
                }

                if (rootFolderPath.IsNullOrWhiteSpace() || !_diskProvider.FolderExists(rootFolderPath))
                {
                    missingRootFolders.Add(rootFolderPath, new List<MovieCollection> { collection });
                }
            }

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    var missingRootFolder = missingRootFolders.First();
                    return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format(_localizationService.GetLocalizedString("MovieCollectionMissingRoot"), FormatRootFolder(missingRootFolder.Key, missingRootFolder.Value)), "#movie-collection-missing-root-folder");
                }

                var message = string.Format(_localizationService.GetLocalizedString("MovieCollectionMultipleMissingRoots"), string.Join(" | ", missingRootFolders.Select(m => FormatRootFolder(m.Key, m.Value))));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#movie-collection-missing-root-folder");
            }

            return new HealthCheck(GetType());
        }

        private string FormatRootFolder(string rootFolderPath, List<MovieCollection> collections)
        {
            return $"{rootFolderPath} ({string.Join(", ", collections.Select(l => l.Title))})";
        }
    }
}
