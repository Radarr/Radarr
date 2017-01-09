using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportApprovedMovie
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedMovie : IImportApprovedMovie
    {
        private readonly IUpgradeMediaFiles _episodeFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedMovie(IUpgradeMediaFiles episodeFileUpgrader,
                                      IMediaFileService mediaFileService,
                                      IExtraService extraService,
                                      IDiskProvider diskProvider,
                                      IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _episodeFileUpgrader = episodeFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalMovie.Movie.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalMovie.Quality, new QualityModelComparer(s.First().LocalMovie.Movie.Profile))
                   .ThenByDescending(c => c.LocalMovie.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderBy(e => e.LocalMovie.Size)
                                                           .ThenByDescending(e => e.LocalMovie.Size))
            {
                var localMovie = importDecision.LocalMovie;
                var oldFiles = new List<MovieFile>();

                try
                {
                    //check if already imported
                    if (importResults.Select(r => r.ImportDecision.LocalMovie.Movie)
                                         .Select(e => e.Id).Contains(localMovie.Movie.Id))
                    {
                        importResults.Add(new ImportResult(importDecision, "Movie has already been imported"));
                        continue;
                    }

                    var episodeFile = new MovieFile();
                    episodeFile.DateAdded = DateTime.UtcNow;
                    episodeFile.MovieId = localMovie.Movie.Id;
                    episodeFile.Path = localMovie.Path.CleanFilePath();
                    episodeFile.Size = _diskProvider.GetFileSize(localMovie.Path);
                    episodeFile.Quality = localMovie.Quality;
                    episodeFile.MediaInfo = localMovie.MediaInfo;
                    episodeFile.Movie = localMovie.Movie;
                    episodeFile.ReleaseGroup = localMovie.ParsedMovieInfo.ReleaseGroup;

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem != null && downloadClientItem.IsReadOnly;
                            break;
                        case ImportMode.Move:
                            copyOnly = false;
                            break;
                        case ImportMode.Copy:
                            copyOnly = true;
                            break;
                    }

                    if (newDownload)
                    {
                        episodeFile.SceneName = GetSceneName(downloadClientItem, localMovie);

                        var moveResult = _episodeFileUpgrader.UpgradeMovieFile(episodeFile, localMovie, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        episodeFile.RelativePath = localMovie.Movie.Path.GetRelativePath(episodeFile.Path);
                    }

                    _mediaFileService.Add(episodeFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (newDownload)
                    {
                        //_extraService.ImportExtraFiles(localMovie, episodeFile, copyOnly); TODO update for movie
                    }

                    if (downloadClientItem != null)
                    {
                        _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, episodeFile, newDownload, downloadClientItem.DownloadClient, downloadClientItem.DownloadId, downloadClientItem.IsReadOnly));
                    }
                    else
                    {
                        _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, episodeFile, newDownload));
                    }

                    if (newDownload)
                    {
                        _eventAggregator.PublishEvent(new MovieDownloadedEvent(localMovie, episodeFile, oldFiles));
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localMovie);
                    importResults.Add(new ImportResult(importDecision, "Failed to import episode"));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            return importResults;
        }

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalMovie localMovie)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseTitle(title);

                if (parsedTitle != null && !parsedTitle.FullSeason)
                {
                    return title;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(localMovie.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            return null;
        }
    }
}
