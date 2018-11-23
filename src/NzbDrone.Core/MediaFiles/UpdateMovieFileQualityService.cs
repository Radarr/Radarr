using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using RestSharp.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpdateMovieFileQualityService
    {

    }

    public class UpdateMovieFileQualityService : IUpdateMovieFileQualityService, IExecute<UpdateMovieFileQualityCommand>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IParsingService _parsingService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public UpdateMovieFileQualityService(IMediaFileService mediaFileService,
            IHistoryService historyService,
            IParsingService parsingService,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _parsingService = parsingService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        //TODO add some good tests for this!
        public void Execute(UpdateMovieFileQualityCommand command)
        {
            var movieFiles = _mediaFileService.GetMovies(command.MovieFileIds);

            var count = 1;

            foreach (var movieFile in movieFiles)
            {
                _logger.ProgressInfo("Updating quality for {0}/{1} files.", count, movieFiles.Count);

                var history = _historyService.GetByMovieId(movieFile.MovieId, null).OrderByDescending(h => h.Date);
                var latestImported = history.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadFolderImported);
                var latestImportedName = latestImported?.SourceTitle;
                var latestGrabbed = history.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);
                var sizeMovie = new LocalMovie();
                sizeMovie.Size = movieFile.Size;

                var helpers = new List<object>{sizeMovie};

                if (movieFile.MediaInfo != null)
                {
                    helpers.Add(movieFile.MediaInfo);
                }

                if (latestGrabbed != null)
                {
                    helpers.Add(latestGrabbed);
                }

                ParsedMovieInfo parsedMovieInfo = null;

                if (latestImportedName?.IsNotNullOrWhiteSpace() == true)
                {
                    parsedMovieInfo = _parsingService.ParseMovieInfo(latestImportedName, helpers);
                }

                if (parsedMovieInfo == null)
                {
                    _logger.Debug("Could not parse movie info from history source title, using current path instead: {0}.", movieFile.RelativePath);
                    parsedMovieInfo = _parsingService.ParseMovieInfo(movieFile.RelativePath, helpers);
                }

                //Only update Custom formats for now.
                if (parsedMovieInfo != null)
                {
                    movieFile.Quality.CustomFormats = parsedMovieInfo.Quality.CustomFormats;
                    _mediaFileService.Update(movieFile);
                    _eventAggregator.PublishEvent(new MovieFileUpdatedEvent(movieFile));
                }
                else
                {
                    _logger.Warn("Could not update custom formats for {0}, since it's title could not be parsed!", movieFile);
                }

                count++;
            }

        }
    }
}
