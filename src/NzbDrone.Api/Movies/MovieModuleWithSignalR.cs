using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.SignalR;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public abstract class MovieModuleWithSignalR : RadarrRestModuleWithSignalR<MovieResource, Core.Movies.Movie>,
        IHandle<MovieGrabbedEvent>,
        IHandle<MovieDownloadedEvent>
    {
        protected readonly IMovieService _movieService;
        protected readonly IUpgradableSpecification _qualityUpgradableSpecification;

        protected MovieModuleWithSignalR(IMovieService movieService,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieModuleWithSignalR(IMovieService movieService,
                                           IUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _movieService = movieService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetMovie;
        }

        protected MovieResource GetMovie(int id)
        {
            var movie = _movieService.GetMovie(id);
            var resource = MapToResource(movie, true);
            return resource;
        }

        protected MovieResource MapToResource(Core.Movies.Movie episode, bool includeSeries)
        {
            var resource = episode.ToResource();

            if (includeSeries)
            {
                var series = episode ?? _movieService.GetMovie(episode.Id);
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
