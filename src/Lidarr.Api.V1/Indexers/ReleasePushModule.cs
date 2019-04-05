using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using Lidarr.Http.Extensions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using Lidarr.Http.REST;
using System;
using NzbDrone.Core.Exceptions;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Lidarr.Api.V1.Indexers
{
    class ReleasePushModule : ReleaseModuleBase
    {
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;
        private ResourceValidator<ReleaseResource> _releaseValidator;

        public ReleasePushModule(IMakeDownloadDecision downloadDecisionMaker,
                                 IProcessDownloadDecisions downloadDecisionProcessor,
                                 IIndexerFactory indexerFactory,
                                 Logger logger)
        {
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadDecisionProcessor = downloadDecisionProcessor;
            _indexerFactory = indexerFactory;
            _logger = logger;

            _releaseValidator = new ResourceValidator<ReleaseResource>();
            _releaseValidator.RuleFor(s => s.Title).NotEmpty();
            _releaseValidator.RuleFor(s => s.DownloadUrl).NotEmpty();
            _releaseValidator.RuleFor(s => s.DownloadProtocol).NotEmpty();
            _releaseValidator.RuleFor(s => s.PublishDate).NotEmpty();

            Post["/push"] = x => ProcessRelease();
        }

        private Response ProcessRelease()
        {

            var resource = new ReleaseResource();

            try
            {
                resource = Request.Body.FromJson<ReleaseResource>();
            }
            catch (Exception ex)
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, ex.Message);
            }

            var validationFailures = _releaseValidator.Validate(resource).Errors;

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures);
            }

            _logger.Info("Release pushed: {0} - {1}", resource.Title, resource.DownloadUrl);

            var info = resource.ToModel();

            info.Guid = "PUSH-" + info.DownloadUrl;

            ResolveIndexer(info);

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            _downloadDecisionProcessor.ProcessDecisions(decisions);

            var firstDecision = decisions.FirstOrDefault();

            if (firstDecision?.RemoteAlbum.ParsedAlbumInfo == null)
            {
                throw new ValidationException(new List<ValidationFailure> { new ValidationFailure("Title", "Unable to parse", resource.Title) });
            }

            return MapDecisions(new[] { firstDecision }).AsResponse();
        }

        private void ResolveIndexer(ReleaseInfo release)
        {
            if (release.IndexerId == 0 && release.Indexer.IsNotNullOrWhiteSpace())
            {
                var indexer = _indexerFactory.All().FirstOrDefault(v => v.Name == release.Indexer);
                if (indexer != null)
                {
                    release.IndexerId = indexer.Id;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title, release.IndexerId, release.Indexer);
                }
                else
                {
                    _logger.Debug("Push Release {0} not associated with unknown indexer {1}.", release.Title, release.Indexer);
                }
            }
            else if (release.IndexerId != 0 && release.Indexer.IsNullOrWhiteSpace())
            {
                try
                {
                    var indexer = _indexerFactory.Get(release.IndexerId);
                    release.Indexer = indexer.Name;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title, release.IndexerId, release.Indexer);
                }
                catch (ModelNotFoundException)
                {
                    _logger.Debug("Push Release {0} not associated with unknown indexer {0}.", release.Title, release.IndexerId);
                    release.IndexerId = 0;
                }
            }
            else
            {
                _logger.Debug("Push Release {0} not associated with an indexer.", release.Title);
            }
        }
    }
}
