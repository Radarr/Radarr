using NzbDrone.Api.Movie;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Movies
{
    public abstract class MovieModuleWithSignalR : NzbDroneRestModuleWithSignalR<MovieResource, Core.Tv.Movie>,
        IHandle<MovieGrabbedEvent>,
        IHandle<MovieDownloadedEvent>
    {
        protected readonly IMovieService _episodeService;
        protected readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        protected MovieModuleWithSignalR(IMovieService episodeService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieModuleWithSignalR(IMovieService episodeService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _episodeService = episodeService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieResource GetMovie(int id)
        {
            var episode = _episodeService.GetMovie(id);
            var resource = MapToResource(episode, true);
            return resource;
        }

        protected MovieResource MapToResource(Core.Tv.Movie episode, bool includeSeries)
        {
            var resource = episode.ToResource();

            if (includeSeries)
            {
                var series = episode ?? _episodeService.GetMovie(episode.Id);
                resource = series.ToResource();
            }

            return resource;
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var resource = message.Movie.Movie.ToResource();

            //add a grabbed field in MovieResource?
            //resource.Grabbed = true;

            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        public void Handle(MovieDownloadedEvent message)
        {
            var resource = message.Movie.Movie.ToResource();
            BroadcastResourceChange(ModelAction.Updated, resource);
        }
    }
}
