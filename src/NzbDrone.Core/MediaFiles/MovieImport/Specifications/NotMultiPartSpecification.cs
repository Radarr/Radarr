using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class NotMultiPartSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;

        public NotMultiPartSpecification(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex[] MovieMultiPartRegex = new Regex[]
        {
            new Regex(@"(?<!^)(?<identifier>[ _.-]*(?:cd|dvd|p(?:ar)?t|dis[ck])[ _.-]*[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(?<!^)(?<identifier>[ _.-]*(?:cd|dvd|p(?:ar)?t|dis[ck])[ _.-]*[a-d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var regexReplace = MovieMultiPartRegex.First().Replace(localMovie.Path, "");

            if (MovieMultiPartRegex.Any(v => v.IsMatch(localMovie.Path)))
            {
                var parentPath = localMovie.Path.GetParentPath();
                var filesInDirectory = _diskProvider.GetFiles(localMovie.Path.GetParentPath(), SearchOption.TopDirectoryOnly);

                foreach (var regex in MovieMultiPartRegex)
                {
                    if (filesInDirectory.Where(file => regex.Replace(file, "") == regex.Replace(localMovie.Path, "")).Count() > 1)
                    {
                        _logger.Debug("Rejected Multi-Part File: {0}", localMovie.Path);

                        return Decision.Reject("File is suspected multi-part file, Radarr doesn't support this");
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
