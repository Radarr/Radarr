using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class NotUnpackingSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NotUnpackingSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                _logger.Debug("{0} is in movie folder, skipping unpacking check", localMovie.Path);
                return ImportSpecDecision.Accept();
            }

            foreach (var workingFolder in _configService.DownloadClientWorkingFolders.Split('|'))
            {
                var parent = Directory.GetParent(localMovie.Path);
                while (parent != null)
                {
                    if (parent.Name.StartsWith(workingFolder))
                    {
                        if (OsInfo.IsNotWindows)
                        {
                            _logger.Debug("{0} is still being unpacked", localMovie.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }

                        if (_diskProvider.FileGetLastWrite(localMovie.Path) > DateTime.UtcNow.AddMinutes(-5))
                        {
                            _logger.Debug("{0} appears to be unpacking still", localMovie.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }
                    }

                    parent = parent.Parent;
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
