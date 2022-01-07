using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IImportApprovedMovie
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedMovie : IImportApprovedMovie
    {
        private readonly IUpgradeMediaFiles _movieFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedMovie(IUpgradeMediaFiles movieFileUpgrader,
                                   IMediaFileService mediaFileService,
                                   IExtraService extraService,
                                   IDiskProvider diskProvider,
                                   IHistoryService historyService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _movieFileUpgrader = movieFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            _logger.Debug("Decisions: {0}", decisions.Count);

            //I added a null op for the rare case that the quality is null. TODO: find out why that would even happen in the first place.
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalMovie.Movie.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalMovie.Quality ?? new QualityModel { Quality = Quality.Unknown }, new QualityModelComparer(s.First().LocalMovie.Movie.Profile))
                   .ThenByDescending(c => c.LocalMovie.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderByDescending(e => e.LocalMovie.Size))
            {
                var localMovie = importDecision.LocalMovie;
                var oldFiles = new List<MovieFile>();

                try
                {
                    //check if already imported
                    if (importResults.Select(r => r.ImportDecision.LocalMovie.Movie)
                                         .Select(m => m.Id).Contains(localMovie.Movie.Id))
                    {
                        importResults.Add(new ImportResult(importDecision, "Movie has already been imported"));
                        continue;
                    }

                    var movieFile = new MovieFile();
                    movieFile.DateAdded = DateTime.UtcNow;
                    movieFile.MovieId = localMovie.Movie.Id;
                    movieFile.Path = localMovie.Path.CleanFilePath();
                    movieFile.Size = _diskProvider.GetFileSize(localMovie.Path);
                    movieFile.Quality = localMovie.Quality;
                    movieFile.Languages = localMovie.Languages;
                    movieFile.MediaInfo = localMovie.MediaInfo;
                    movieFile.Movie = localMovie.Movie;
                    movieFile.ReleaseGroup = localMovie.ReleaseGroup;
                    movieFile.Edition = localMovie.Edition;

                    if (downloadClientItem?.DownloadId.IsNotNullOrWhiteSpace() == true)
                    {
                        var grabHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                            .OrderByDescending(h => h.Date)
                            .FirstOrDefault(h => h.EventType == MovieHistoryEventType.Grabbed);

                        if (Enum.TryParse(grabHistory?.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
                        {
                            movieFile.IndexerFlags = flags;
                        }
                    }

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem != null && !downloadClientItem.CanMoveFiles;
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
                        movieFile.OriginalFilePath = GetOriginalFilePath(downloadClientItem, localMovie);
                        movieFile.SceneName = GetSceneName(downloadClientItem, localMovie);
                        var moveResult = _movieFileUpgrader.UpgradeMovieFile(movieFile, localMovie, copyOnly); //TODO: Check if this works
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        movieFile.RelativePath = localMovie.Movie.Path.GetRelativePath(movieFile.Path);

                        // Delete existing files from the DB mapped to this path
                        var previousFiles = _mediaFileService.GetFilesWithRelativePath(localMovie.Movie.Id, movieFile.RelativePath);

                        foreach (var previousFile in previousFiles)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }
                    }

                    movieFile = _mediaFileService.Add(movieFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (newDownload)
                    {
                        _extraService.ImportMovie(localMovie, movieFile, copyOnly);
                    }

                    _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, movieFile, oldFiles, newDownload, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import movie " + localMovie);
                    _eventAggregator.PublishEvent(new MovieImportFailedEvent(e, localMovie, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import movie, Root folder missing."));
                }
                catch (DestinationAlreadyExistsException e)
                {
                    _logger.Warn(e, "Couldn't import movie " + localMovie);
                    importResults.Add(new ImportResult(importDecision, "Failed to import movie, Destination already exists."));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import movie " + localMovie);
                    importResults.Add(new ImportResult(importDecision, "Failed to import movie"));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            return importResults;
        }

        private string GetOriginalFilePath(DownloadClientItem downloadClientItem, LocalMovie localMovie)
        {
            var path = localMovie.Path;

            if (downloadClientItem != null && !downloadClientItem.OutputPath.IsEmpty)
            {
                var outputDirectory = downloadClientItem.OutputPath.Directory.ToString();

                if (outputDirectory.IsParentPath(path))
                {
                    return outputDirectory.GetRelativePath(path);
                }
            }

            var folderMovieInfo = localMovie.FolderMovieInfo;

            if (folderMovieInfo != null)
            {
                var folderPath = path.GetAncestorPath(folderMovieInfo.OriginalTitle);

                if (folderPath != null)
                {
                    return folderPath.GetParentPath().GetRelativePath(path);
                }
            }

            var parentPath = path.GetParentPath();
            var grandparentPath = parentPath.GetParentPath();

            if (grandparentPath != null)
            {
                return grandparentPath.GetRelativePath(path);
            }

            return Path.Combine(Path.GetFileName(parentPath), Path.GetFileName(path));
        }

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalMovie localMovie)
        {
            if (downloadClientItem != null)
            {
                var sceneNameTitle = SceneChecker.GetSceneTitle(downloadClientItem.Title);
                if (sceneNameTitle != null)
                {
                    return sceneNameTitle;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(localMovie.Path.CleanFilePath());
            var sceneNameFile = SceneChecker.GetSceneTitle(fileName);
            if (sceneNameFile != null)
            {
                return sceneNameFile;
            }

            var folderTitle = localMovie.FolderMovieInfo?.ReleaseTitle;
            if (folderTitle.IsNotNullOrWhiteSpace() && SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
