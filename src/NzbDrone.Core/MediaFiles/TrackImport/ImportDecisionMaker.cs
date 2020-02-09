using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, FilterFilesType filter, bool includeExisting);
        List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, DownloadClientItem downloadClientItem, ParsedTrackInfo folderInfo);
        List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, Album album, AlbumRelease albumRelease, DownloadClientItem downloadClientItem, ParsedTrackInfo folderInfo, FilterFilesType filter, bool newDownload, bool singleRelease, bool includeExisting);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IImportDecisionEngineSpecification<LocalTrack>> _trackSpecifications;
        private readonly IEnumerable<IImportDecisionEngineSpecification<LocalAlbumRelease>> _albumSpecifications;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAugmentingService _augmentingService;
        private readonly IIdentificationService _identificationService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IImportDecisionEngineSpecification<LocalTrack>> trackSpecifications,
                                   IEnumerable<IImportDecisionEngineSpecification<LocalAlbumRelease>> albumSpecifications,
                                   IMediaFileService mediaFileService,
                                   IAudioTagService audioTagService,
                                   IAugmentingService augmentingService,
                                   IIdentificationService identificationService,
                                   IAlbumService albumService,
                                   IReleaseService releaseService,
                                   IEventAggregator eventAggregator,
                                   IDiskProvider diskProvider,
                                   Logger logger)
        {
            _trackSpecifications = trackSpecifications;
            _albumSpecifications = albumSpecifications;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _augmentingService = augmentingService;
            _identificationService = identificationService;
            _albumService = albumService;
            _releaseService = releaseService;
            _eventAggregator = eventAggregator;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, FilterFilesType filter, bool includeExisting)
        {
            return GetImportDecisions(musicFiles, artist, null, null, null, null, filter, false, false, true);
        }

        public List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, DownloadClientItem downloadClientItem, ParsedTrackInfo folderInfo)
        {
            return GetImportDecisions(musicFiles, artist, null, null, downloadClientItem, folderInfo, FilterFilesType.None, true, false, false);
        }

        public List<ImportDecision<LocalTrack>> GetImportDecisions(List<IFileInfo> musicFiles, Artist artist, Album album, AlbumRelease albumRelease, DownloadClientItem downloadClientItem, ParsedTrackInfo folderInfo, FilterFilesType filter, bool newDownload, bool singleRelease, bool includeExisting)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var files = filter != FilterFilesType.None && (artist != null) ? _mediaFileService.FilterUnchangedFiles(musicFiles, artist, filter) : musicFiles;

            var localTracks = new List<LocalTrack>();
            var decisions = new List<ImportDecision<LocalTrack>>();

            _logger.Debug("Analyzing {0}/{1} files.", files.Count, musicFiles.Count);

            if (!files.Any())
            {
                return decisions;
            }

            ParsedAlbumInfo downloadClientItemInfo = null;

            if (downloadClientItem != null)
            {
                downloadClientItemInfo = Parser.Parser.ParseAlbumTitle(downloadClientItem.Title);
            }

            foreach (var file in files)
            {
                var localTrack = new LocalTrack
                {
                    Artist = artist,
                    Album = album,
                    DownloadClientAlbumInfo = downloadClientItemInfo,
                    FolderTrackInfo = folderInfo,
                    Path = file.FullName,
                    Size = file.Length,
                    Modified = file.LastWriteTimeUtc,
                    FileTrackInfo = _audioTagService.ReadTags(file.FullName),
                    ExistingFile = !newDownload,
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

            var releases = _identificationService.Identify(localTracks, artist, album, albumRelease, newDownload, singleRelease, includeExisting);

            foreach (var release in releases)
            {
                release.NewDownload = newDownload;
                var releaseDecision = GetDecision(release, downloadClientItem);

                foreach (var localTrack in release.LocalTracks)
                {
                    if (releaseDecision.Approved)
                    {
                        decisions.AddIfNotNull(GetDecision(localTrack, downloadClientItem));
                    }
                    else
                    {
                        decisions.Add(new ImportDecision<LocalTrack>(localTrack, releaseDecision.Rejections.ToArray()));
                    }
                }
            }

            return decisions;
        }

        private ImportDecision<LocalAlbumRelease> GetDecision(LocalAlbumRelease localAlbumRelease, DownloadClientItem downloadClientItem)
        {
            ImportDecision<LocalAlbumRelease> decision = null;

            if (localAlbumRelease.AlbumRelease == null)
            {
                decision = new ImportDecision<LocalAlbumRelease>(localAlbumRelease, new Rejection($"Couldn't find similar album for {localAlbumRelease}"));
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
                _logger.Debug("Album rejected for the following reasons: {0}", string.Join(", ", decision.Rejections));
            }
            else
            {
                _logger.Debug("Album accepted");
            }

            return decision;
        }

        private ImportDecision<LocalTrack> GetDecision(LocalTrack localTrack, DownloadClientItem downloadClientItem)
        {
            ImportDecision<LocalTrack> decision = null;

            if (localTrack.Tracks.Empty())
            {
                decision = localTrack.Album != null ? new ImportDecision<LocalTrack>(localTrack, new Rejection($"Couldn't parse track from: {localTrack.FileTrackInfo}")) :
                    new ImportDecision<LocalTrack>(localTrack, new Rejection($"Couldn't parse album from: {localTrack.FileTrackInfo}"));
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
