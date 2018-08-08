using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

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
        private readonly Logger _logger;

        public UpdateMovieFileQualityService(IMediaFileService mediaFileService, IHistoryService historyService, IParsingService parsingService, Logger logger)
        {
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _parsingService = parsingService;
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
                
                var history = _historyService.FindByMovieId(movieFile.MovieId).OrderByDescending(h => h.Date);
                var latestImported = history.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadFolderImported);
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
                
                var parsedMovieInfo = _parsingService.ParseMovieInfo(latestImported?.SourceTitle ?? movieFile.RelativePath, helpers);
                
                //Only update Custom formats for now.
                movieFile.Quality.CustomFormats = parsedMovieInfo.Quality.CustomFormats;
                //movieFile.Edition = parsedMovieInfo.Edition;
                //movieFile.ReleaseGroup = parsedMovieInfo.ReleaseGroup;
                _mediaFileService.Update(movieFile);
                count++;
            }
        }
    }
}
