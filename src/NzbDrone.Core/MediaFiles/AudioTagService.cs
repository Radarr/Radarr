using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using TagLib;

namespace NzbDrone.Core.MediaFiles
{
    public interface IAudioTagService
    {
        ParsedTrackInfo ReadTags(string file);
        void WriteTags(BookFile trackfile, bool newDownload, bool force = false);
        void SyncTags(List<Book> tracks);
        List<RetagTrackFilePreview> GetRetagPreviewsByArtist(int authorId);
        List<RetagTrackFilePreview> GetRetagPreviewsByAlbum(int authorId);
    }

    public class AudioTagService : IAudioTagService,
        IExecute<RetagArtistCommand>,
        IExecute<RetagFilesCommand>
    {
        private readonly IConfigService _configService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderWatchingService _rootFolderWatchingService;
        private readonly IArtistService _artistService;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public AudioTagService(IConfigService configService,
                               IMediaFileService mediaFileService,
                               IDiskProvider diskProvider,
                               IRootFolderWatchingService rootFolderWatchingService,
                               IArtistService artistService,
                               IMapCoversToLocal mediaCoverService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _configService = configService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _rootFolderWatchingService = rootFolderWatchingService;
            _artistService = artistService;
            _mediaCoverService = mediaCoverService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public AudioTag ReadAudioTag(string path)
        {
            return new AudioTag(path);
        }

        public ParsedTrackInfo ReadTags(string path)
        {
            return new AudioTag(path);
        }

        public AudioTag GetTrackMetadata(BookFile trackfile)
        {
            return new AudioTag();
        }

        private void UpdateTrackfileSizeAndModified(BookFile trackfile, string path)
        {
            // update the saved file size so that the importer doesn't get confused on the next scan
            var fileInfo = _diskProvider.GetFileInfo(path);
            trackfile.Size = fileInfo.Length;
            trackfile.Modified = fileInfo.LastWriteTimeUtc;

            if (trackfile.Id > 0)
            {
                _mediaFileService.Update(trackfile);
            }
        }

        public void RemoveAllTags(string path)
        {
            TagLib.File file = null;
            try
            {
                file = TagLib.File.Create(path);
                file.RemoveTags(TagLib.TagTypes.AllTags);
                file.Save();
            }
            catch (CorruptFileException ex)
            {
                _logger.Warn(ex, $"Tag removal failed for {path}.  File is corrupt");
            }
            catch (Exception ex)
            {
                _logger.Warn()
                    .Exception(ex)
                    .Message($"Tag removal failed for {path}")
                    .WriteSentryWarn("Tag removal failed")
                    .Write();
            }
            finally
            {
                file?.Dispose();
            }
        }

        public void WriteTags(BookFile trackfile, bool newDownload, bool force = false)
        {
            if (!force)
            {
                if (_configService.WriteAudioTags == WriteAudioTagsType.No ||
                    (_configService.WriteAudioTags == WriteAudioTagsType.NewFiles && !newDownload))
                {
                    return;
                }
            }

            var newTags = GetTrackMetadata(trackfile);
            var path = trackfile.Path;

            var diff = ReadAudioTag(path).Diff(newTags);

            _rootFolderWatchingService.ReportFileSystemChangeBeginning(path);

            if (_configService.ScrubAudioTags)
            {
                _logger.Debug($"Scrubbing tags for {trackfile}");
                RemoveAllTags(path);
            }

            _logger.Debug($"Writing tags for {trackfile}");

            newTags.Write(path);

            UpdateTrackfileSizeAndModified(trackfile, path);

            _eventAggregator.PublishEvent(new TrackFileRetaggedEvent(trackfile.Artist.Value, trackfile, diff, _configService.ScrubAudioTags));
        }

        public void SyncTags(List<Book> books)
        {
            if (_configService.WriteAudioTags != WriteAudioTagsType.Sync)
            {
                return;
            }

            // get the tracks to update
            foreach (var book in books)
            {
                var trackFiles = book.BookFiles.Value;

                _logger.Debug($"Syncing audio tags for {trackFiles.Count} files");

                foreach (var file in trackFiles)
                {
                    // populate tracks (which should also have release/album/artist set) because
                    // not all of the updates will have been committed to the database yet
                    file.Album = book;
                    WriteTags(file, false);
                }
            }
        }

        public List<RetagTrackFilePreview> GetRetagPreviewsByArtist(int authorId)
        {
            var files = _mediaFileService.GetFilesByArtist(authorId);

            return GetPreviews(files).ToList();
        }

        public List<RetagTrackFilePreview> GetRetagPreviewsByAlbum(int bookId)
        {
            var files = _mediaFileService.GetFilesByAlbum(bookId);

            return GetPreviews(files).ToList();
        }

        private IEnumerable<RetagTrackFilePreview> GetPreviews(List<BookFile> files)
        {
            foreach (var f in files.OrderBy(x => x.Album.Value.Title))
            {
                var file = f;

                if (f.Album.Value == null)
                {
                    _logger.Warn($"File {f} is not linked to any books");
                    continue;
                }

                var oldTags = ReadAudioTag(f.Path);
                var newTags = GetTrackMetadata(f);
                var diff = oldTags.Diff(newTags);

                if (diff.Any())
                {
                    yield return new RetagTrackFilePreview
                    {
                        AuthorId = file.Artist.Value.Id,
                        BookId = file.Album.Value.Id,
                        TrackFileId = file.Id,
                        Path = file.Path,
                        Changes = diff
                    };
                }
            }
        }

        public void Execute(RetagFilesCommand message)
        {
            var artist = _artistService.GetArtist(message.AuthorId);
            var trackFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Re-tagging {0} files for {1}", trackFiles.Count, artist.Name);
            foreach (var file in trackFiles)
            {
                WriteTags(file, false, force: true);
            }

            _logger.ProgressInfo("Selected track files re-tagged for {0}", artist.Name);
        }

        public void Execute(RetagArtistCommand message)
        {
            _logger.Debug("Re-tagging all files for selected artists");
            var artistToRename = _artistService.GetArtists(message.AuthorIds);

            foreach (var artist in artistToRename)
            {
                var trackFiles = _mediaFileService.GetFilesByArtist(artist.Id);
                _logger.ProgressInfo("Re-tagging all files in artist: {0}", artist.Name);
                foreach (var file in trackFiles)
                {
                    WriteTags(file, false, force: true);
                }

                _logger.ProgressInfo("All track files re-tagged for {0}", artist.Name);
            }
        }
    }
}
