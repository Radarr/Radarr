using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRenameTrackFileService
    {
        List<RenameTrackFilePreview> GetRenamePreviews(int authorId);
        List<RenameTrackFilePreview> GetRenamePreviews(int authorId, int bookId);
    }

    public class RenameTrackFileService : IRenameTrackFileService, IExecute<RenameFilesCommand>, IExecute<RenameArtistCommand>
    {
        private readonly IArtistService _artistService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveTrackFiles _trackFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RenameTrackFileService(IArtistService artistService,
                                        IMediaFileService mediaFileService,
                                        IMoveTrackFiles trackFileMover,
                                        IEventAggregator eventAggregator,
                                        IBuildFileNames filenameBuilder,
                                        IDiskProvider diskProvider,
                                        Logger logger)
        {
            _artistService = artistService;
            _mediaFileService = mediaFileService;
            _trackFileMover = trackFileMover;
            _eventAggregator = eventAggregator;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameTrackFilePreview> GetRenamePreviews(int authorId)
        {
            var artist = _artistService.GetArtist(authorId);
            var files = _mediaFileService.GetFilesByArtist(authorId);

            _logger.Trace($"got {files.Count} files");

            return GetPreviews(artist, files)
                .OrderByDescending(e => e.BookId)
                .ToList();
        }

        public List<RenameTrackFilePreview> GetRenamePreviews(int authorId, int bookId)
        {
            var artist = _artistService.GetArtist(authorId);
            var files = _mediaFileService.GetFilesByAlbum(bookId);

            return GetPreviews(artist, files)
                .OrderByDescending(e => e.TrackNumbers.First()).ToList();
        }

        private IEnumerable<RenameTrackFilePreview> GetPreviews(Author artist, List<BookFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var book = file.Album.Value;
                var trackFilePath = file.Path;

                if (book == null)
                {
                    _logger.Warn("File ({0}) is not linked to a book", trackFilePath);
                    continue;
                }

                var newName = _filenameBuilder.BuildTrackFileName(artist, book, file);

                _logger.Trace($"got name {newName}");

                var newPath = _filenameBuilder.BuildTrackFilePath(artist, book, newName, Path.GetExtension(trackFilePath));

                _logger.Trace($"got path {newPath}");

                if (!trackFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameTrackFilePreview
                    {
                        AuthorId = artist.Id,
                        BookId = book.Id,
                        TrackFileId = file.Id,
                        ExistingPath = file.Path,
                        NewPath = newPath
                    };
                }
            }
        }

        private void RenameFiles(List<BookFile> trackFiles, Author artist)
        {
            var renamed = new List<BookFile>();

            foreach (var trackFile in trackFiles)
            {
                var trackFilePath = trackFile.Path;

                try
                {
                    _logger.Debug("Renaming track file: {0}", trackFile);
                    _trackFileMover.MoveTrackFile(trackFile, artist);

                    _mediaFileService.Update(trackFile);
                    renamed.Add(trackFile);

                    _logger.Debug("Renamed track file: {0}", trackFile);

                    _eventAggregator.PublishEvent(new TrackFileRenamedEvent(artist, trackFile, trackFilePath));
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to rename file {0}", trackFilePath);
                }
            }

            if (renamed.Any())
            {
                _eventAggregator.PublishEvent(new ArtistRenamedEvent(artist));

                _logger.Debug("Removing Empty Subfolders from: {0}", artist.Path);
                _diskProvider.RemoveEmptySubfolders(artist.Path);
            }
        }

        public void Execute(RenameFilesCommand message)
        {
            var artist = _artistService.GetArtist(message.AuthorId);
            var trackFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", trackFiles.Count, artist.Name);
            RenameFiles(trackFiles, artist);
            _logger.ProgressInfo("Selected track files renamed for {0}", artist.Name);
        }

        public void Execute(RenameArtistCommand message)
        {
            _logger.Debug("Renaming all files for selected artist");
            var artistToRename = _artistService.GetArtists(message.AuthorIds);

            foreach (var artist in artistToRename)
            {
                var trackFiles = _mediaFileService.GetFilesByArtist(artist.Id);
                _logger.ProgressInfo("Renaming all files in artist: {0}", artist.Name);
                RenameFiles(trackFiles, artist);
                _logger.ProgressInfo("All track files renamed for {0}", artist.Name);
            }
        }
    }
}
