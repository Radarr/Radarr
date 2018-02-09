using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.MediaInfo;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool shouldCheckQuality);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldCheckQuality);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource);
        List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, ParsedEpisodeInfo folderInfo, bool sceneSource);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDetectSample _detectSample;
        private readonly IQualityDefinitionService _qualitiesService;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IDetectSample detectSample,
                                   IQualityDefinitionService qualitiesService,
                                   Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _detectSample = detectSample;
            _qualitiesService = qualitiesService;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series)
        {
            return GetImportDecisions(videoFiles, series, null, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie)
        {
            return GetImportDecisions(videoFiles, movie, null, null, true, false);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, bool shouldCheckQuality = false)
        {
            return GetImportDecisions(videoFiles, movie, null, null, true, shouldCheckQuality);
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Series series, ParsedEpisodeInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), series);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, series, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, series, folderInfo, sceneSource, shouldUseFolderName));
            }

            return decisions;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, movie, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, movie, downloadClientItem, folderInfo, sceneSource, shouldUseFolderName));
            }

            return decisions;
        }

        public List<ImportDecision> GetImportDecisions(List<string> videoFiles, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldCheckQuality)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(videoFiles.ToList(), movie);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, videoFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(videoFiles, movie, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, movie, downloadClientItem, folderInfo, sceneSource, shouldUseFolderName, shouldCheckQuality));
            }

            return decisions;
        }

        private ImportDecision GetDecision(string file, Movie movie, DownloadClientItem downloadClientItem, ParsedMovieInfo folderInfo, bool sceneSource, bool shouldUseFolderName, bool shouldCheckQuality = false)
        {
            ImportDecision decision = null;

            try
            {
                var localMovie = _parsingService.GetLocalMovie(file, movie, shouldUseFolderName ? folderInfo : null, sceneSource);

                if (localMovie != null)
                {
                    localMovie.Quality = GetQuality(folderInfo, localMovie.Quality, movie);
                    localMovie.Size = _diskProvider.GetFileSize(file);

                    _logger.Debug("Size: {0}", localMovie.Size);
					var current = localMovie.Quality;
                    localMovie.MediaInfo = _videoFileInfoReader.GetMediaInfo(file);
                    //TODO: make it so media info doesn't ruin the import process of a new series
					if (sceneSource && ShouldCheckQualityForParsedQuality(current.Quality))
                    {
                        
                        if (shouldCheckQuality)
                        {
							_logger.Debug("Checking quality for this video file to make sure nothing mismatched.");
                            var width = localMovie.MediaInfo.Width;
                            
                            var qualityName = current.Quality.Name.ToLower();
                            QualityModel updated = null;
                            if (width > 2000)
                            {
                                if (qualityName.Contains("bluray"))
                                {
                                    updated = new QualityModel(Quality.Bluray2160p);
                                }

                                else if (qualityName.Contains("webdl"))
                                {
                                    updated = new QualityModel(Quality.WEBDL2160p);
                                }

                                else if (qualityName.Contains("hdtv"))
                                {
                                    updated = new QualityModel(Quality.HDTV2160p);
                                }

                                else
                                {
                                    var def = _qualitiesService.Get(Quality.HDTV2160p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.HDTV2160p);
                                    }
                                    def = _qualitiesService.Get(Quality.WEBDL2160p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.WEBDL2160p);
                                    }
                                    def = _qualitiesService.Get(Quality.Bluray2160p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.Bluray2160p);
                                    }
                                    if (updated == null)
                                    {
                                        updated = new QualityModel(Quality.Bluray2160p);
                                    }
                                }

                            }
                            else if (width > 1400)
                            {
                                if (qualityName.Contains("bluray"))
                                {
                                    updated = new QualityModel(Quality.Bluray1080p);
                                }

                                else if (qualityName.Contains("webdl"))
                                {
                                    updated = new QualityModel(Quality.WEBDL1080p);
                                }

                                else if (qualityName.Contains("hdtv"))
                                {
                                    updated = new QualityModel(Quality.HDTV1080p);
                                }

                                else
                                {
                                    var def = _qualitiesService.Get(Quality.HDTV1080p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.HDTV1080p);
                                    }
                                    def = _qualitiesService.Get(Quality.WEBDL1080p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.WEBDL1080p);
                                    }
                                    def = _qualitiesService.Get(Quality.Bluray1080p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.Bluray1080p);
                                    }
                                    if (updated == null)
                                    {
                                        updated = new QualityModel(Quality.Bluray1080p);
                                    }
                                }

                            }
                            else
                            if (width > 900)
                            {
                                if (qualityName.Contains("bluray"))
                                {
                                    updated = new QualityModel(Quality.Bluray720p);
                                }

                                else if (qualityName.Contains("webdl"))
                                {
                                    updated = new QualityModel(Quality.WEBDL720p);
                                }

                                else if (qualityName.Contains("hdtv"))
                                {
                                    updated = new QualityModel(Quality.HDTV720p);
                                }

                                else
                                {

                                    var def = _qualitiesService.Get(Quality.HDTV720p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.HDTV720p);
                                    }
                                    def = _qualitiesService.Get(Quality.WEBDL720p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.WEBDL720p);
                                    }
                                    def = _qualitiesService.Get(Quality.Bluray720p);
                                    if (localMovie.Size > def.MinSize && def.MaxSize > localMovie.Size)
                                    {
                                        updated = new QualityModel(Quality.Bluray720p);
                                    }
                                    if (updated == null)
                                    {
                                        updated = new QualityModel(Quality.Bluray720p);
                                    }

                                }
                            }
                            if (updated != null && updated != current)
                            {
								_logger.Debug("Quality ({0}) of the file is different than the one we have ({1})", updated, current);
                                updated.QualitySource = QualitySource.MediaInfo;
                                localMovie.Quality = updated;
                            }
                        }



                        decision = GetDecision(localMovie, downloadClientItem);
                    }
                    else
                    {
                        decision = GetDecision(localMovie, downloadClientItem);
                    }
                }

                else
                {
                    localMovie = new LocalMovie();
                    localMovie.Path = file;

                    decision = new ImportDecision(localMovie, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't import file. " + file);

                var localMovie = new LocalMovie { Path = file };
                decision = new ImportDecision(localMovie, new Rejection("Unexpected error processing file"));
            }

            //LocalMovie nullMovie = null;

            //decision = new ImportDecision(nullMovie, new Rejection("IMPLEMENTATION MISSING!!!"));

            return decision;
        }

        private ImportDecision GetDecision(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localMovie, downloadClientItem))
                                         .Where(c => c != null);

            return new ImportDecision(localMovie, reasons.ToArray());
        }

        private ImportDecision GetDecision(string file, Series series, ParsedEpisodeInfo folderInfo, bool sceneSource, bool shouldUseFolderName)
        {
            ImportDecision decision = null;

            
            return decision;
        }

        private ImportDecision GetDecision(LocalEpisode localEpisode)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localEpisode))
                                         .Where(c => c != null);

            return new ImportDecision(localEpisode, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localMovie, downloadClientItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (NotImplementedException e)
            {
                _logger.Warn(e, "Spec " + spec.ToString() + " currently does not implement evaluation for movies.");
                return null;
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on " + localMovie.Path);
                return new Rejection(string.Format("{0}: {1}", spec.GetType().Name, e.Message));
            }

            return null;
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalEpisode localEpisode)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localEpisode);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on " + localEpisode.Path);
                return new Rejection(string.Format("{0}: {1}", spec.GetType().Name, e.Message));
            }

            return null;
        }

        private bool ShouldUseFolderName(List<string> videoFiles, Series series, ParsedEpisodeInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.FullSeason)
            {
                return false;
            }

            return videoFiles.Count(file =>
            {
                var size = _diskProvider.GetFileSize(file);
                var fileQuality = QualityParser.ParseQuality(file);
                var sample = _detectSample.IsSample(series, GetQuality(folderInfo, fileQuality, series), file, size, folderInfo.IsPossibleSpecialEpisode);

                if (sample)
                {
                    return false;
                }

                if (SceneChecker.IsSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private bool ShouldUseFolderName(List<string> videoFiles, Movie movie, ParsedMovieInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            //if (folderInfo.FullSeason)
            //{
            //    return false;
            //}

            return videoFiles.Count(file =>
            {
                var size = _diskProvider.GetFileSize(file);
                var fileQuality = QualityParser.ParseQuality(file);
                //var sample = null;//_detectSample.IsSample(movie, GetQuality(folderInfo, fileQuality, movie), file, size, folderInfo.IsPossibleSpecialEpisode); //Todo to this

                return true;

                //if (sample)
                {
                    return false;
                }

                if (SceneChecker.IsSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private QualityModel GetQuality(ParsedMovieInfo folderInfo, QualityModel fileQuality, Movie movie)
        {
            if (UseFolderQuality(folderInfo, fileQuality, movie))
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }

        private QualityModel GetQuality(ParsedEpisodeInfo folderInfo, QualityModel fileQuality, Series series)
        {
            if (UseFolderQuality(folderInfo, fileQuality, series))
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }

        private bool UseFolderQuality(ParsedMovieInfo folderInfo, QualityModel fileQuality, Movie movie)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Quality.Quality == Quality.Unknown)
            {
                return false;
            }

            if (fileQuality.QualitySource == QualitySource.Extension)
            {
                return true;
            }

            if (new QualityModelComparer(movie.Profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                return true;
            }

            return false;
        }

        private bool UseFolderQuality(ParsedEpisodeInfo folderInfo, QualityModel fileQuality, Series series)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Quality.Quality == Quality.Unknown)
            {
                return false;
            }

            if (fileQuality.QualitySource == QualitySource.Extension)
            {
                return true;
            }

            if (new QualityModelComparer(series.Profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                return true;
            }

            return false;
        }

		private bool ShouldCheckQualityForParsedQuality(Quality quality)
		{
			List<Quality> shouldNotCheck = new List<Quality> { Quality.WORKPRINT, Quality.TELECINE, Quality.TELESYNC,
			Quality.DVDSCR, Quality.DVD, Quality.CAM, Quality.DVDR, Quality.Remux1080p, Quality.Remux2160p, Quality.REGIONAL
			};

			if (shouldNotCheck.Contains(quality))
			{
				return false;

			}

			return true;
		}
    }
}
