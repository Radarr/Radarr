using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        TrackFileMoveResult UpgradeTrackFile(BookFile trackFile, LocalTrack localTrack, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly IMoveTrackFiles _trackFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly IRootFolderWatchingService _rootFolderWatchingService;
        private readonly ICalibreProxy _calibre;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IAudioTagService audioTagService,
                                       IMoveTrackFiles trackFileMover,
                                       IDiskProvider diskProvider,
                                       IRootFolderService rootFolderService,
                                       IRootFolderWatchingService rootFolderWatchingService,
                                       ICalibreProxy calibre,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _trackFileMover = trackFileMover;
            _diskProvider = diskProvider;
            _rootFolderService = rootFolderService;
            _rootFolderWatchingService = rootFolderWatchingService;
            _calibre = calibre;
            _logger = logger;
        }

        public TrackFileMoveResult UpgradeTrackFile(BookFile trackFile, LocalTrack localTrack, bool copyOnly = false)
        {
            var moveFileResult = new TrackFileMoveResult();
            var existingFiles = localTrack.Album.BookFiles.Value;

            var rootFolderPath = _diskProvider.GetParentFolder(localTrack.Artist.Path);
            var rootFolder = _rootFolderService.GetBestRootFolder(rootFolderPath);
            var isCalibre = rootFolder.IsCalibreLibrary && rootFolder.CalibreSettings != null;

            var settings = rootFolder.CalibreSettings;

            // If there are existing track files and the root folder is missing, throw, so the old file isn't left behind during the import process.
            if (existingFiles.Any() && !_diskProvider.FolderExists(rootFolderPath))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolderPath}' was not found.");
            }

            foreach (var file in existingFiles)
            {
                var trackFilePath = file.Path;
                var subfolder = rootFolderPath.GetRelativePath(_diskProvider.GetParentFolder(trackFilePath));

                trackFile.CalibreId = file.CalibreId;

                if (_diskProvider.FileExists(trackFilePath))
                {
                    _logger.Debug("Removing existing track file: {0}", file);

                    if (!isCalibre)
                    {
                        _recycleBinProvider.DeleteFile(trackFilePath, subfolder);
                    }
                    else
                    {
                        _calibre.RemoveFormats(file.CalibreId,
                                              new[]
                                              {
                                                  Path.GetExtension(trackFile.Path)
                                              },
                                              settings);
                    }
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            if (!isCalibre)
            {
                if (copyOnly)
                {
                    moveFileResult.TrackFile = _trackFileMover.CopyTrackFile(trackFile, localTrack);
                }
                else
                {
                    moveFileResult.TrackFile = _trackFileMover.MoveTrackFile(trackFile, localTrack);
                }

                _audioTagService.WriteTags(trackFile, true);
            }
            else
            {
                var source = trackFile.Path;

                moveFileResult.TrackFile = CalibreAddAndConvert(trackFile, settings);

                if (!copyOnly)
                {
                    _diskProvider.DeleteFile(source);
                }
            }

            return moveFileResult;
        }

        public BookFile CalibreAddAndConvert(BookFile file, CalibreSettings settings)
        {
            _logger.Trace($"Importing to calibre: {file.Path}");

            if (file.CalibreId == 0)
            {
                var import = _calibre.AddBook(file, settings);
                file.CalibreId = import.Id;
            }
            else
            {
                _calibre.AddFormat(file, settings);
            }

            _calibre.SetFields(file, settings);

            var updated = _calibre.GetBook(file.CalibreId, settings);
            var path = updated.Formats.Values.OrderByDescending(x => x.LastModified).First().Path;

            file.Path = path;

            _rootFolderWatchingService.ReportFileSystemChangeBeginning(file.Path);

            if (settings.OutputFormat.IsNotNullOrWhiteSpace())
            {
                _logger.Trace($"Getting book data for {file.CalibreId}");
                var options = _calibre.GetBookData(file.CalibreId, settings);
                var inputFormat = file.Quality.Quality.Name.ToUpper();

                options.Conversion_options.Input_fmt = inputFormat;

                var formats = settings.OutputFormat.Split(',').Select(x => x.Trim());
                foreach (var format in formats)
                {
                    if (format.ToLower() == inputFormat ||
                        options.Input_formats.Contains(format, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    options.Conversion_options.Output_fmt = format;

                    if (settings.OutputProfile != (int)CalibreProfile.Default)
                    {
                        options.Conversion_options.Options.Output_profile = ((CalibreProfile)settings.OutputProfile).ToString();
                    }

                    _logger.Trace($"Starting conversion to {format}");
                    _calibre.ConvertBook(file.CalibreId, options.Conversion_options, settings);
                }
            }

            return file;
        }
    }
}
