using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist);
        List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IDetectSample detectSample,
                                   Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _detectSample = detectSample;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist)
        {
            return GetImportDecisions(musicFiles, artist, null);
        }

        public List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo)
        {
            var newFiles = _mediaFileService.FilterExistingFiles(musicFiles.ToList(), artist);

            _logger.Debug("Analyzing {0}/{1} files.", newFiles.Count, musicFiles.Count());

            var shouldUseFolderName = ShouldUseFolderName(musicFiles, artist, folderInfo);
            var decisions = new List<ImportDecision>();

            foreach (var file in newFiles)
            {
                decisions.AddIfNotNull(GetDecision(file, artist, folderInfo, shouldUseFolderName));
            }

            return decisions;
        }

        private ImportDecision GetDecision(string file, Artist artist, ParsedTrackInfo folderInfo, bool shouldUseFolderName)
        {
            ImportDecision decision = null;

            try
            {
                var localTrack = _parsingService.GetLocalTrack(file, artist, shouldUseFolderName ? folderInfo : null);

                if (localTrack != null)
                {
                    localTrack.Quality = GetQuality(folderInfo, localTrack.Quality, artist);
                    localTrack.Size = _diskProvider.GetFileSize(file);

                    _logger.Debug("Size: {0}", localTrack.Size);

                    //TODO: make it so media info doesn't ruin the import process of a new artist

                    if (localTrack.Tracks.Empty())
                    {
                        decision = new ImportDecision(localTrack, new Rejection("Invalid album or track"));
                    }
                    else
                    {
                        decision = GetDecision(localTrack);
                    }
                }

                else
                {
                    localTrack = new LocalTrack();
                    localTrack.Path = file;

                    decision = new ImportDecision(localTrack, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't import file. {0}", file);

                var localTrack = new LocalTrack { Path = file };
                decision = new ImportDecision(localTrack, new Rejection("Unexpected error processing file"));
            }

            return decision;
        }

        private ImportDecision GetDecision(LocalTrack localTrack)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localTrack))
                                         .Where(c => c != null);

            return new ImportDecision(localTrack, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IImportDecisionEngineSpecification spec, LocalTrack localTrack)
        {
            try
            {
                var result = spec.IsSatisfiedBy(localTrack);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", localTrack.Path);
                return new Rejection($"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private bool ShouldUseFolderName(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo)
        {
            if (folderInfo == null)
            {
                return false;
            }

            //if (folderInfo.FullSeason)
            //{
            //    return false;
            //}

            return musicFiles.Count(file =>
            {
                var size = _diskProvider.GetFileSize(file);
                var fileQuality = QualityParser.ParseQuality(file);
                //var sample = _detectSample.IsSample(artist, GetQuality(folderInfo, fileQuality, artist), file, size, folderInfo.IsPossibleSpecialEpisode);

                //if (sample)
                //{
                //    return false;
                //}

                if (SceneChecker.IsSceneTitle(Path.GetFileName(file)))
                {
                    return false;
                }

                return true;
            }) == 1;
        }

        private QualityModel GetQuality(ParsedTrackInfo folderInfo, QualityModel fileQuality, Artist artist)
        {
            if (UseFolderQuality(folderInfo, fileQuality, artist))
            {
                _logger.Debug("Using quality from folder: {0}", folderInfo.Quality);
                return folderInfo.Quality;
            }

            return fileQuality;
        }

        private bool UseFolderQuality(ParsedTrackInfo folderInfo, QualityModel fileQuality, Artist artist)
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

            if (new QualityModelComparer(artist.Profile).Compare(folderInfo.Quality, fileQuality) > 0)
            {
                return true;
            }

            return false;
        }
    }
}
