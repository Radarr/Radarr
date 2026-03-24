using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class NotMultiPartSpecification : IImportDecisionEngineSpecification
    {
        private static readonly Regex[] MovieMultiPartRegex = new[]
        {
            new Regex(@"(?<!^)(?<identifier>[ _.-]*(?:cd|dvd|p(?:(?:ar)?t)?|dis[ck])[ _.-]*[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(?<!^)(?<identifier>[ _.-]*(?:cd|dvd|p(?:(?:ar)?t)?|dis[ck])[ _.-]*[a-d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public NotMultiPartSpecification(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (MovieMultiPartRegex.Any(v => v.IsMatch(localMovie.Path)))
            {
                var filesInDirectory = _diskProvider.GetFiles(localMovie.Path.GetParentPath(), false).ToList();

                foreach (var regex in MovieMultiPartRegex)
                {
                    if (filesInDirectory.Count(file => regex.Replace(file, "") == regex.Replace(localMovie.Path, "")) > 1)
                    {
                        _logger.Debug("Rejected Multi-Part File: {0}", localMovie.Path);

                        return ImportSpecDecision.Reject(ImportRejectionReason.MultiPartMovie, "File is suspected multi-part file, Radarr doesn't support this");
                    }
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
