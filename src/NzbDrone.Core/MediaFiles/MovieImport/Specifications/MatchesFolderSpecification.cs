using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
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

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return Decision.Accept();
            }

            var dirInfo = new FileInfo(localMovie.Path).Directory;

            if (dirInfo == null)
            {
                return Decision.Accept();
            }

            var folderInfo = Parser.Parser.ParseMovieTitle(dirInfo.Name, false);

            if (folderInfo == null)
            {
                return Decision.Accept();
            }

            return Decision.Accept();
        }
    }
}
