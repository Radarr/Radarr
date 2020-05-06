using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, IdentificationOverrides idOverrides, ImportDecisionMakerInfo itemInfo, ImportDecisionMakerConfig config);
    }

    public class IdentificationOverrides
    {
        public Author Artist { get; set; }
        public Book Album { get; set; }
    }

    public class ImportDecisionMakerInfo
    {
        public DownloadClientItem DownloadClientItem { get; set; }
        public ParsedTrackInfo ParsedTrackInfo { get; set; }
    }

    public class ImportDecisionMakerConfig
    {
        public FilterFilesType Filter { get; set; }
        public bool NewDownload { get; set; }
        public bool SingleRelease { get; set; }
        public bool IncludeExisting { get; set; }
        public bool AddNewArtists { get; set; }
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification<LocalTrack>> _trackSpecifications;
        private readonly IEnumerable<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumSpecifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IEBookTagService _eBookTagService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAugmentingService _augmentingService;
        private readonly IIdentificationService _identificationService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IProfileService _qualityProfileService;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification<LocalTrack>> trackSpecifications,
                                   IEnumerable<IImportDecisionEngineSpecification<LocalAlbumRelease>> albumSpecifications,
                                   IMediaFileService mediaFileService,
                                   IEBookTagService eBookTagService,
                                   IAudioTagService audioTagService,
                                   IAugmentingService augmentingService,
                                   IIdentificationService identificationService,
                                   IRootFolderService rootFolderService,
                                   IProfileService qualityProfileService,
                                   Logger logger)
        {
            _trackSpecifications = trackSpecifications;
            _albumSpecifications = albumSpecifications;
            _mediaFileService = mediaFileService;
            _eBookTagService = eBookTagService;
            _audioTagService = audioTagService;
            _augmentingService = augmentingService;
            _identificationService = identificationService;
            _rootFolderService = rootFolderService;
            _qualityProfileService = qualityProfileService;
            _logger = logger;
        }

        public Tuple<List<LocalTrack>, List<ImportDecision<LocalTrack>>> GetLocalTracks(List<IFileInfo> musicFiles, DownloadClientItem downloadClientItem, ParsedTrackInfo folderInfo, FilterFilesType filter)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var files = _mediaFileService.FilterUnchangedFiles(musicFiles, filter);

            var localTracks = new List<LocalTrack>();
            var decisions = new List<ImportDecision<LocalTrack>>();

            _logger.Debug("Analyzing {0}/{1} files.", files.Count, musicFiles.Count);

            if (!files.Any())
            {
                return Tuple.Create(localTracks, decisions);
            }

            ParsedAlbumInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseAlbumTitle(downloadClientItem.Title);
            }

            int i = 1;
            foreach (var file in files)
            {
                _logger.ProgressInfo($"Reading file {i++}/{files.Count}");

                var localTrack = new LocalTrack
                {
                    DownloadClientAlbumInfo = downloadClientItemInfo,
                    FolderTrackInfo = folderInfo,
                    Path = file.FullName,
                    Size = file.Length,
                    Modified = file.LastWriteTimeUtc,
                    FileTrackInfo = _eBookTagService.ReadTags(file),
                    AdditionalFile = false
                };

                try
                {
                    // TODO fix otherfiles?
                    _augmentingService.Augment(localTrack, true);
                    localTracks.Add(localTrack);
                }
                catch (AugmentingFailedException)
                {
                    decisions.Add(new ImportDecision<LocalTrack>(localTrack, new Rejection("Unable to parse file")));
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't import file. {0}", localTrack.Path);

                    decisions.Add(new ImportDecision<LocalTrack>(localTrack, new Rejection("Unexpected error processing file")));
                }
            }

            _logger.Debug($"Tags parsed for {files.Count} files in {watch.ElapsedMilliseconds}ms");

            return Tuple.Create(localTracks, decisions);
        }

        public List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, IdentificationOverrides idOverrides, ImportDecisionMakerInfo itemInfo, ImportDecisionMakerConfig config)
        {
            idOverrides = idOverrides ?? new IdentificationOverrides();
            itemInfo = itemInfo ?? new ImportDecisionMakerInfo();

            var trackData = GetLocalTracks(musicFiles, itemInfo.DownloadClientItem, itemInfo.ParsedTrackInfo, config.Filter);
            var localTracks = trackData.Item1;
            var decisions = trackData.Item2;

            localTracks.ForEach(x => x.ExistingFile = !config.NewDownload);

            var releases = _identificationService.Identify(localTracks, idOverrides, config);

            foreach (var release in releases)
            {
                // make sure the appropriate quality profile is set for the release artist
                // in case it's a new artist
                EnsureData(release);
                release.NewDownload = config.NewDownload;

                var releaseDecision = GetDecision(release, itemInfo.DownloadClientItem);

                foreach (var localTrack in release.LocalTracks)
                {
                    if (releaseDecision.Approved)
                    {
                        decisions.AddIfNotNull(GetDecision(localTrack, itemInfo.DownloadClientItem));
                    }
                    else
                    {
                        decisions.Add(new ImportDecision<LocalTrack>(localTrack, releaseDecision.Rejections.ToArray()));
                    }
                }
            }

            return decisions;
        }

        private void EnsureData(LocalAlbumRelease release)
        {
            if (release.Book != null && release.Book.Author.Value.QualityProfileId == 0)
            {
                var rootFolder = _rootFolderService.GetBestRootFolder(release.LocalTracks.First().Path);
                var qualityProfile = _qualityProfileService.Get(rootFolder.DefaultQualityProfileId);

                var artist = release.Book.Author.Value;
                artist.QualityProfileId = qualityProfile.Id;
                artist.QualityProfile = qualityProfile;
            }
        }

        private ImportDecision<LocalAlbumRelease> GetDecision(LocalAlbumRelease localAlbumRelease, DownloadClientItem downloadClientItem)
        {
            ImportDecision<LocalAlbumRelease> decision = null;

            if (localAlbumRelease.Book == null)
            {
                decision = new ImportDecision<LocalAlbumRelease>(localAlbumRelease, new Rejection($"Couldn't find similar book for {localAlbumRelease}"));
            }
            else
            {
                var reasons = _albumSpecifications.Select(c => EvaluateSpec(c, localAlbumRelease, downloadClientItem))
                    .Where(c => c != null);

                decision = new ImportDecision<LocalAlbumRelease>(localAlbumRelease, reasons.ToArray());
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localAlbumRelease);
            }
            else if (decision.Rejections.Any())
            {
                _logger.Debug("Book rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("Book accepted");
            }

            return decision;
        }

        private ImportDecision<LocalTrack> GetDecision(LocalTrack localTrack, DownloadClientItem downloadClientItem)
        {
            ImportDecision<LocalTrack> decision = null;

            if (localTrack.Album == null)
            {
                decision = new ImportDecision<LocalTrack>(localTrack, new Rejection($"Couldn't parse album from: {localTrack.FileTrackInfo}"));
            }
            else
            {
                var reasons = _trackSpecifications.Select(c => EvaluateSpec(c, localTrack, downloadClientItem))
                    .Where(c => c != null);

                decision = new ImportDecision<LocalTrack>(localTrack, reasons.ToArray());
            }

            if (decision == null)
            {
                _logger.Error("Unable to make a decision on {0}", localTrack.Path);
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

        private Rejection EvaluateSpec<T>(IImportDecisionEngineSpecification<T> spec, T item, DownloadClientItem downloadClientItem)
        {
            try
            {
                var result = spec.IsSatisfiedBy(item, downloadClientItem);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't evaluate decision on {0}", item);
                return new Rejection($"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }
    }
}
