using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDecisionEngineSpecification
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualityDefinitionService qualityDefinitionService, Logger logger)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedMovieInfo.Quality.Quality;

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check");
                return Decision.Accept();
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);

            if (subject.Movie.MovieMetadata.Value.Runtime == 0)
            {
                _logger.Warn("{0} has no runtime information using median movie runtime of 110 minutes.", subject.Movie);
                subject.Movie.MovieMetadata.Value.Runtime = 110;
            }

            if (qualityDefinition.MinSize.HasValue)
            {
                var minSize = qualityDefinition.MinSize.Value.Megabytes();

                //Multiply maxSize by Series.Runtime
                minSize = minSize * subject.Movie.MovieMetadata.Value.Runtime;

                //If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Movie.Title;

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return Decision.Reject("{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }

            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping check");
            }
            else if (subject.Movie.MovieMetadata.Value.Runtime == 0)
            {
                _logger.Debug("Movie runtime is 0, unable to validate size until it is available, rejecting");
                return Decision.Reject("Movie runtime is 0, unable to validate size until it is available");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Megabytes();

                //Multiply maxSize by Series.Runtime
                maxSize = maxSize * subject.Movie.MovieMetadata.Value.Runtime;

                //If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, subject.Movie.Title);
                    return Decision.Reject("{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), subject.Movie.Title);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return Decision.Accept();
        }
    }
}
