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
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System.Diagnostics;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist);
        List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo);
        List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo, bool filterExistingFiles, bool timidReleaseSwitching);
        ImportDecision GetImportDecision(string musicFile, Artist artist, Album album);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification> specifications,
                                   IParsingService parsingService,
                                   IMediaFileService mediaFileService,
                                   IAlbumService albumService,
                                   IReleaseService releaseService,
                                   IEventAggregator eventAggregator,
                                   IDiskProvider diskProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _mediaFileService = mediaFileService;
            _albumService = albumService;
            _releaseService = releaseService;
            _eventAggregator = eventAggregator;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist)
        {
            return GetImportDecisions(musicFiles, artist, null);
        }

        public List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo)
        {
            return GetImportDecisions(musicFiles, artist, folderInfo, false, false);
        }

        private bool MatchesCurrentRelease(ImportDecision decision)
        {
            return decision.Approved || decision.Rejections.Select(x => x.Reason).Contains("Has the same filesize as existing file");
        }

        public List<ImportDecision> GetImportDecisions(List<string> musicFiles, Artist artist, ParsedTrackInfo folderInfo, bool filterExistingFiles, bool timidReleaseSwitching)
        {
            var files = filterExistingFiles ? _mediaFileService.FilterExistingFiles(musicFiles.ToList(), artist) : musicFiles.ToList();

            _logger.Debug("Analyzing {0}/{1} files.", files.Count, musicFiles.Count);

            var shouldUseFolderName = ShouldUseFolderName(musicFiles, artist, folderInfo);

            // We have to do this once to match against albums
            var decisions = GetImportDecisionsForCurrentRelease(files, artist, folderInfo, shouldUseFolderName);

            // Now we have matched the files against albums, we can group by album and check for the best release
            var albums = decisions.Where(x => x.LocalTrack.Album != null)
                .Select(x => x.LocalTrack.Album)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            var revisedDecisions = decisions.Where(x => x.LocalTrack.Album == null).ToList();

            foreach (var album in albums)
            {
                var albumDecisions = decisions.Where(x => x.LocalTrack.Album != null && x.LocalTrack.Album.Id == album.Id).ToList();
                revisedDecisions.AddRange(GetImportDecisions(albumDecisions, artist, album, folderInfo, shouldUseFolderName, timidReleaseSwitching));
            }

            Ensure.That(decisions.Count == revisedDecisions.Count).IsTrue();
            
            return revisedDecisions;
        }

        private List<ImportDecision> GetImportDecisionsForCurrentRelease(List<string> files, Artist artist, ParsedTrackInfo folderInfo, bool shouldUseFolderName)
        {
            var decisions = new List<ImportDecision>();

            foreach (var file in files)
            {
                decisions.AddIfNotNull(GetDecision(file, artist, null, folderInfo, shouldUseFolderName));
            }

            return decisions;
        }

        public ImportDecision GetImportDecision(string file, Artist artist, Album album)
        {
            return GetDecision(file, artist, album, null, false);
        }
        
        public List<ImportDecision> GetImportDecisions(List<ImportDecision> decisions, Artist artist, Album album, ParsedTrackInfo folderInfo, bool shouldUseFolderName, bool timidReleaseSwitching)
        {
            _logger.Debug("Importing {0}", album);
            var maxTrackCount = album.AlbumReleases.Value.Where(x => x.Monitored).Select(x => x.TrackCount).Max();
            var haveExistingFiles = _mediaFileService.GetFilesByAlbum(album.Id).Any();
            var releaseSwitchingAllowed = !(haveExistingFiles && timidReleaseSwitching);

            if (album.AnyReleaseOk && releaseSwitchingAllowed)
            {
                if (decisions.Any(x => !MatchesCurrentRelease(x)) || decisions.Count != maxTrackCount)
                {
                    _logger.Debug("Importing {0}: {1}/{2} files approved for {3} track release",
                                  album,
                                  decisions.Count(x => MatchesCurrentRelease(x)),
                                  decisions.Count,
                                  maxTrackCount);
                    return GetImportDecisionsForBestRelease(decisions, artist, album, folderInfo, shouldUseFolderName);
                }
                else
                {
                    _logger.Debug("Importing {0}: All files approved and all tracks have a file", album);
                    return decisions;
                }
            }
            else
            {
                _logger.Debug("Importing {0}: {1}/{2} files approved for {3} track release.  Release switching not allowed.",
                              album,
                              decisions.Count(x => MatchesCurrentRelease(x)),
                              decisions.Count,
                              maxTrackCount);
                return decisions;
            }
        }

        private List<ImportDecision> GetImportDecisionsForBestRelease(List<ImportDecision> decisions, Artist artist, Album album, ParsedTrackInfo folderInfo, bool shouldUseFolderName)
        {
            var files = decisions.Select(x => x.LocalTrack.Path).ToList();

            // At the moment we assume only one release can be monitored at a time
            var originalRelease = album.AlbumReleases.Value.Where(x => x.Monitored).Single();
            var candidateReleases = album.AlbumReleases.Value.Where(x => x.TrackCount >= files.Count && x.Id != originalRelease.Id).ToList();
            var bestRelease = originalRelease;
            var bestMatchCount = decisions.Count(x => MatchesCurrentRelease(x));
            var bestDecisions = decisions;
                
            foreach (var release in candidateReleases)
            {
                _logger.Debug("Trying Release {0} [{1} tracks]", release, release.TrackCount);
                album.AlbumReleases = _releaseService.SetMonitored(release);
                var newDecisions = GetImportDecisionsForCurrentRelease(files, artist, folderInfo, shouldUseFolderName);

                _logger.Debug("Importing {0}: {1}/{2} files approved for {3} track release {4}",
                              album,
                              newDecisions.Count(x => MatchesCurrentRelease(x)),
                              newDecisions.Count,
                              release.TrackCount,
                              release);

                // We want the release that matches the most tracks.  If there's a tie,
                // we want the release with the fewest entries (i.e. fewest missing)
                var currentMatchCount = newDecisions.Count(x => MatchesCurrentRelease(x));
                if (currentMatchCount > bestMatchCount
                    || (currentMatchCount == bestMatchCount && release.TrackCount < bestRelease.TrackCount))
                {
                    bestMatchCount = currentMatchCount;
                    bestRelease = release;
                    bestDecisions = newDecisions;

                    if (currentMatchCount == release.TrackCount && newDecisions.All(x => MatchesCurrentRelease(x)))
                    {
                        break;
                    }
                }
            }

            _logger.Debug("{0} Best release: {1}", album, bestRelease);

            // reinstate the original release in case the import isn't run (manual import)
            album.AlbumReleases = _releaseService.SetMonitored(originalRelease);

            return bestDecisions;
        }

        private ImportDecision GetDecision(string file, Artist artist, Album album, ParsedTrackInfo folderInfo, bool shouldUseFolderName)
        {
            ImportDecision decision = null;

            try
            {
                var localTrack = _parsingService.GetLocalTrack(file, artist, album, shouldUseFolderName ? folderInfo : null);

                if (localTrack != null)
                {
                    localTrack.Quality = GetQuality(folderInfo, localTrack.Quality, artist);
                    localTrack.Language = GetLanguage(folderInfo, localTrack.Language, artist);
                    localTrack.Size = _diskProvider.GetFileSize(file);

                    _logger.Debug("Size: {0}", localTrack.Size);

                    //TODO: make it so media info doesn't ruin the import process of a new artist

                    if (localTrack.Tracks.Empty())
                    {
                        decision = localTrack.Album != null ? new ImportDecision(localTrack, new Rejection($"Couldn't parse track from: {localTrack.ParsedTrackInfo}")) :
                            new ImportDecision(localTrack, new Rejection($"Couldn't parse album from: {localTrack.ParsedTrackInfo}"));
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
                    localTrack.Quality = new QualityModel(Quality.Unknown);
                    localTrack.Language = Language.Unknown;

                    decision = new ImportDecision(localTrack, new Rejection("Unable to parse file"));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't import file. {0}", file);

                var localTrack = new LocalTrack { Path = file };
                decision = new ImportDecision(localTrack, new Rejection("Unexpected error processing file"));
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", file);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("File rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("File accepted");
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

            return musicFiles.Count(file =>
            {

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

        private Language GetLanguage(ParsedTrackInfo folderInfo, Language fileLanguage, Artist artist)
        {
            if (UseFolderLanguage(folderInfo, fileLanguage, artist))
            {
                _logger.Debug("Using language from folder: {0}", folderInfo.Language);
                return folderInfo.Language;
            }

            return fileLanguage;
        }

        private bool UseFolderLanguage(ParsedTrackInfo folderInfo, Language fileLanguage, Artist artist)
        {
            if (folderInfo == null)
            {
                return false;
            }

            if (folderInfo.Language == Language.Unknown)
            {
                return false;
            }

            if (new LanguageComparer(artist.LanguageProfile).Compare(folderInfo.Language, fileLanguage) > 0)
            {
                return true;
            }

            return false;
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
