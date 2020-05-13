using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.MediaFiles.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(AuthorDeletedEvent))]
    [CheckOn(typeof(AuthorMovedEvent))]
    [CheckOn(typeof(TrackImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(TrackImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly IAuthorService _authorService;
        private readonly IImportListFactory _importListFactory;
        private readonly IDiskProvider _diskProvider;

        public RootFolderCheck(IAuthorService authorService, IImportListFactory importListFactory, IDiskProvider diskProvider)
        {
            _authorService = authorService;
            _importListFactory = importListFactory;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var missingRootFolders = _authorService.GetAllAuthors()
                                                   .Select(s => _diskProvider.GetParentFolder(s.Path))
                                                   .Distinct()
                                                   .Where(s => !_diskProvider.FolderExists(s))
                                                   .ToList();

            missingRootFolders.AddRange(_importListFactory.All()
                .Select(s => s.RootFolderPath)
                .Distinct()
                .Where(s => !_diskProvider.FolderExists(s))
                .ToList());

            missingRootFolders = missingRootFolders.Distinct().ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, "Missing root folder: " + missingRootFolders.First(), "#missing-root-folder");
                }

                var message = string.Format("Multiple root folders are missing: {0}", string.Join(" | ", missingRootFolders));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#missing-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
