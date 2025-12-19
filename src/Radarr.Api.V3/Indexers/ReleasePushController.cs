using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using Radarr.Http;

namespace Radarr.Api.V3.Indexers
{
    [V3ApiController("release/push")]
    public class ReleasePushController : ReleaseControllerBase
    {
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly Logger _logger;

        private static readonly SemaphoreSlim PushLock = new SemaphoreSlim(1, 1);

        public ReleasePushController(IMakeDownloadDecision downloadDecisionMaker,
                                 IProcessDownloadDecisions downloadDecisionProcessor,
                                 IIndexerFactory indexerFactory,
                                 IDownloadClientFactory downloadClientFactory,
                                 IQualityProfileService qualityProfileService,
                                 Logger logger)
            : base(qualityProfileService)
        {
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadDecisionProcessor = downloadDecisionProcessor;
            _indexerFactory = indexerFactory;
            _downloadClientFactory = downloadClientFactory;
            _logger = logger;

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.DownloadUrl).NotEmpty().When(s => s.MagnetUrl.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.MagnetUrl).NotEmpty().When(s => s.DownloadUrl.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Protocol).NotEmpty();
            PostValidator.RuleFor(s => s.PublishDate).NotEmpty();
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<List<ReleaseResource>>> Create([FromBody] ReleaseResource release)
        {
            _logger.Info("Release pushed: {0} - {1}", release.Title.SanitizeForLog(), (release.DownloadUrl ?? release.MagnetUrl).SanitizeForLog());

            ValidateResource(release);

            var info = release.ToModel();

            info.Guid = "PUSH-" + info.DownloadUrl;

            ResolveIndexer(info);

            var downloadClientId = ResolveDownloadClientId(release);

            DownloadDecision decision;

            await PushLock.WaitAsync();
            try
            {
                var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info }, true);

                decision = decisions.FirstOrDefault();

                await _downloadDecisionProcessor.ProcessDecision(decision, downloadClientId);
            }
            finally
            {
                PushLock.Release();
            }

            if (decision?.RemoteMovie.ParsedMovieInfo == null)
            {
                throw new ValidationException(new List<ValidationFailure> { new ("Title", "Unable to parse", release.Title) });
            }

            return MapDecisions(new[] { decision });
        }

        private void ResolveIndexer(ReleaseInfo release)
        {
            if (release.IndexerId == 0 && release.Indexer.IsNotNullOrWhiteSpace())
            {
                var indexer = _indexerFactory.All().FirstOrDefault(v => v.Name.EqualsIgnoreCase(release.Indexer));

                if (indexer != null)
                {
                    release.IndexerId = indexer.Id;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title.SanitizeForLog(), release.IndexerId, release.Indexer.SanitizeForLog());
                }
                else
                {
                    _logger.Debug("Push Release {0} not associated with known indexer {1}.", release.Title.SanitizeForLog(), release.Indexer.SanitizeForLog());
                }
            }
            else if (release.IndexerId != 0 && release.Indexer.IsNullOrWhiteSpace())
            {
                try
                {
                    var indexer = _indexerFactory.Get(release.IndexerId);
                    release.Indexer = indexer.Name;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title.SanitizeForLog(), release.IndexerId, release.Indexer.SanitizeForLog());
                }
                catch (ModelNotFoundException)
                {
                    _logger.Debug("Push Release {0} not associated with known indexer {1}.", release.Title.SanitizeForLog(), release.IndexerId);
                    release.IndexerId = 0;
                }
            }
            else
            {
                _logger.Debug("Push Release {0} not associated with an indexer.", release.Title.SanitizeForLog());
            }
        }

        private int? ResolveDownloadClientId(ReleaseResource release)
        {
            var downloadClientId = release.DownloadClientId.GetValueOrDefault();

            if (downloadClientId == 0 && release.DownloadClient.IsNotNullOrWhiteSpace())
            {
                var downloadClient = _downloadClientFactory.All().FirstOrDefault(v => v.Name.EqualsIgnoreCase(release.DownloadClient));

                if (downloadClient != null)
                {
                    _logger.Debug("Push Release {0} associated with download client {1} - {2}.", release.Title.SanitizeForLog(), downloadClientId, release.DownloadClient.SanitizeForLog());

                    return downloadClient.Id;
                }

                _logger.Debug("Push Release {0} not associated with known download client {1}.", release.Title.SanitizeForLog(), release.DownloadClient.SanitizeForLog());
            }

            return release.DownloadClientId;
        }
    }
}
