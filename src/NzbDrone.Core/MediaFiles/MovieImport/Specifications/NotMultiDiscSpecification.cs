using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class NotMultiDiscSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public NotMultiDiscSpecification(Logger logger)
        {
            _logger = logger;
        }

        private static readonly Regex[] MovieMultiPartRegex = new Regex[]
        {
            new Regex(@"(.*?)([ _.-]*(?:cd|dvd|p(?:ar)?t|dis[ck])[ _.-]*[0-9]+)(.*?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(.*?)([ _.-]*(?:cd|dvd|p(?:ar)?t|dis[ck])[ _.-]*[a-d])(.*?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (MovieMultiPartRegex.Any(v => v.IsMatch(localMovie.Path)))
            {
                _logger.Debug("Rejected Multi-Movie File: " + localMovie.Path);

                return Decision.Reject("File is Multi-Disc, Radarr doesn't support");
            }

            return Decision.Accept();
        }
    }
}
