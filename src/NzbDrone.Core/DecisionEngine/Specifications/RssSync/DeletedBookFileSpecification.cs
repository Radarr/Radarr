using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DeletedBookFileSpecification : IDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IMediaFileService _bookService;
        private readonly Logger _logger;

        public DeletedBookFileSpecification(IDiskProvider diskProvider,
                                             IConfigService configService,
                                             IMediaFileService bookService,
                                             Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _bookService = bookService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Disk;
        public RejectionType Type => RejectionType.Temporary;

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            if (!_configService.AutoUnmonitorPreviouslyDownloadedBooks)
            {
                return Decision.Accept();
            }

            if (searchCriteria != null)
            {
                _logger.Debug("Skipping deleted bookfile check during search");
                return Decision.Accept();
            }

            var missingTrackFiles = subject.Books
                                             .SelectMany(v => _bookService.GetFilesByBook(v.Id))
                                             .DistinctBy(v => v.Id)
                                             .Where(v => IsTrackFileMissing(subject.Author, v))
                                             .ToArray();

            if (missingTrackFiles.Any())
            {
                foreach (var missingTrackFile in missingTrackFiles)
                {
                    _logger.Trace("Book file {0} is missing from disk.", missingTrackFile.Path);
                }

                _logger.Debug("Files for this book exist in the database but not on disk, will be unmonitored on next diskscan. skipping.");
                return Decision.Reject("Author is not monitored");
            }

            return Decision.Accept();
        }

        private bool IsTrackFileMissing(Author author, BookFile bookFile)
        {
            return !_diskProvider.FileExists(bookFile.Path);
        }
    }
}
