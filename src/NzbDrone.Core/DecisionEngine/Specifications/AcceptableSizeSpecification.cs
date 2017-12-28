using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;

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
        
        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedAlbumInfo.Quality.Quality;

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check.");
                return Decision.Accept();
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);
            var albumsDuration = subject.Albums.Sum(album => album.Duration) / 1000;

            if (qualityDefinition.MinSize.HasValue)
            {
                var minSize = qualityDefinition.MinSize.Value.Kilobits();

                //Multiply minSize by Album.Duration
                minSize = minSize * albumsDuration;

                //If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = $"{albumsDuration}sec";

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return Decision.Reject("{0} is smaller than minimum allowed {1}", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix());
                }
            }
            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited - skipping check.");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Kilobits();

                //Multiply maxSize by Album.Duration
                maxSize = maxSize * albumsDuration;

                //If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    var runtimeMessage = $"{albumsDuration}sec";

                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, maxSize, runtimeMessage);
                    return Decision.Reject("{0} is larger than maximum allowed {1}", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix());
                }
            }

            _logger.Debug("Item: {0}, meets size constraints.", subject);
            return Decision.Accept();
        }
    }
}
