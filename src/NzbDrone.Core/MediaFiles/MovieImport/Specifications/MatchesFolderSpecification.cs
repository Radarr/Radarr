using System.IO;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesFolderSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var dirInfo = new FileInfo(localMovie.Path).Directory;

            if (dirInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            // TODO: Actually implement this!!!!
            /*var folderInfo = Parser.Parser.ParseMovieTitle(dirInfo.Name, false);

            if (folderInfo == null)
            {
                return Decision.Accept();
            }*/

            return ImportSpecDecision.Accept();
        }
    }
}
