﻿using System;
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
            _logger.Debug("Decisions: {0}", decisions.Count);

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
                    movieFile.MediaInfo = localMovie.MediaInfo;
                    movieFile.Movie = localMovie.Movie;
                    movieFile.ReleaseGroup = localMovie.ParsedMovieInfo.ReleaseGroup;
                    movieFile.Edition = localMovie.ParsedMovieInfo.Edition;

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
                        movieFile.SceneName = GetSceneName(downloadClientItem, localMovie);

                        var moveResult = _episodeFileUpgrader.UpgradeMovieFile(movieFile, localMovie, copyOnly); //TODO: Check if this works
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        movieFile.RelativePath = localMovie.Movie.Path.GetRelativePath(movieFile.Path);
                    }

                    _mediaFileService.Add(movieFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (newDownload)
                    {
                        //_extraService.ImportExtraFiles(localMovie, episodeFile, copyOnly); TODO update for movie
                    }

                    if (downloadClientItem != null)
                    {
                        _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, movieFile, newDownload, downloadClientItem.DownloadClient, downloadClientItem.DownloadId, downloadClientItem.IsReadOnly));
                    }
                    else
                    {
                        _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, movieFile, newDownload));
                    }

                    if (newDownload)
                    {
                        _eventAggregator.PublishEvent(new MovieDownloadedEvent(localMovie, movieFile, oldFiles));
                    }
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

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalMovie localMovie)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseMovieTitle(title);

                if (parsedTitle != null)
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
