using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Specifications
{
    public class SameFileSpecification : IImportDecisionEngineSpecification<LocalBook>
    {
        private readonly Logger _logger;

        public SameFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalBook item, DownloadClientItem downloadClientItem)
        {
            var bookFiles = item.Book?.BookFiles?.Value;

            if (bookFiles == null || !bookFiles.Any())
            {
                _logger.Debug("No existing book file, skipping");
                return Decision.Accept();
            }

            foreach (var bookFile in bookFiles)
            {
                if (bookFile.Size == item.Size)
                {
                    _logger.Debug("'{0}' Has the same filesize as existing file", item.Path);
                    return Decision.Reject("Has the same filesize as existing file");
                }
            }

            return Decision.Accept();
        }
    }
}
