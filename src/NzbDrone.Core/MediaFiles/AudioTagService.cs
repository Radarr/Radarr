using NLog;
using NzbDrone.Core.Parser.Model;
using System.IO;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Music;
using System.Collections.Generic;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Disk;
using System;
using NLog.Fluent;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using TagLib;

namespace NzbDrone.Core.MediaFiles
{
    public interface IAudioTagService
    {
        ParsedTrackInfo ReadTags(string file);
        void WriteTags(TrackFile trackfile, bool newDownload, bool force = false);
        void SyncTags(List<Track> tracks);
        void RemoveMusicBrainzTags(IEnumerable<Album> album);
        void RemoveMusicBrainzTags(IEnumerable<AlbumRelease> albumRelease);
        void RemoveMusicBrainzTags(IEnumerable<Track> tracks);
        void RemoveMusicBrainzTags(TrackFile trackfile);
        List<RetagTrackFilePreview> GetRetagPreviewsByArtist(int artistId);
        List<RetagTrackFilePreview> GetRetagPreviewsByAlbum(int artistId);
    }
    
    public class AudioTagService : IAudioTagService,
        IExecute<RetagArtistCommand>,
        IExecute<RetagFilesCommand>
    {
        private readonly IConfigService _configService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IArtistService _artistService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;
        
        public AudioTagService(IConfigService configService,
                               IMediaFileService mediaFileService,
                               IDiskProvider diskProvider,
                               IArtistService artistService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _configService = configService;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _artistService = artistService;
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

        private AudioTag GetTrackMetadata(TrackFile trackfile)
        {
            var track = trackfile.Tracks.Value[0];
            var release = track.AlbumRelease.Value;
            var album = release.Album.Value;
            var albumartist = album.Artist.Value;
            var artist = track.ArtistMetadata.Value;

            return new AudioTag {
                Title = track.Title,
                Performers = new [] { artist.Name },
                AlbumArtists = new [] { albumartist.Name },
                Track = (uint)track.AbsoluteTrackNumber,
                TrackCount = (uint)release.Tracks.Value.Count(x => x.MediumNumber == track.MediumNumber),
                Album = album.Title,
                Disc = (uint)track.MediumNumber,
                DiscCount = (uint)release.Media.Count,
                Media = release.Media[track.MediumNumber - 1].Format,
                Date = release.ReleaseDate,
                Year = (uint)album.ReleaseDate?.Year,
                OriginalReleaseDate = album.ReleaseDate,
                OriginalYear = (uint)album.ReleaseDate?.Year,
                Publisher = release.Label.FirstOrDefault(),
                MusicBrainzReleaseCountry = IsoCountries.Find(release.Country.FirstOrDefault()).TwoLetterCode,
                MusicBrainzReleaseStatus = release.Status.ToLower(),
                MusicBrainzReleaseType = album.AlbumType.ToLower(),
                MusicBrainzReleaseId = release.ForeignReleaseId,
                MusicBrainzArtistId = artist.ForeignArtistId,
                MusicBrainzReleaseArtistId = albumartist.ForeignArtistId,
                MusicBrainzReleaseGroupId = album.ForeignAlbumId,
                MusicBrainzTrackId = track.ForeignRecordingId,
                MusicBrainzReleaseTrackId = track.ForeignTrackId,
                MusicBrainzAlbumComment = album.Disambiguation,
            };
        }

        private void UpdateTrackfileSize(TrackFile trackfile, string path)
        {
            // update the saved file size so that the importer doesn't get confused on the next scan
            trackfile.Size = _diskProvider.GetFileSize(path);
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

        public void RemoveMusicBrainzTags(string path)
        {
            var tags = new AudioTag(path);
            
            tags.MusicBrainzReleaseCountry = null;
            tags.MusicBrainzReleaseStatus = null;
            tags.MusicBrainzReleaseType = null;
            tags.MusicBrainzReleaseId = null;
            tags.MusicBrainzArtistId = null;
            tags.MusicBrainzReleaseArtistId = null;
            tags.MusicBrainzReleaseGroupId = null;
            tags.MusicBrainzTrackId = null;
            tags.MusicBrainzAlbumComment = null;
            tags.MusicBrainzReleaseTrackId = null;

            tags.Write(path);
        }

        public void WriteTags(TrackFile trackfile, bool newDownload, bool force = false)
        {
            if (!force)
            {
                if (_configService.WriteAudioTags == WriteAudioTagsType.No ||
                    (_configService.WriteAudioTags == WriteAudioTagsType.NewFiles && !newDownload))
                {
                    return;
                }
            }

            if (trackfile.Tracks.Value.Count > 1)
            {
                _logger.Debug($"File {trackfile} is linked to multiple tracks. Not writing tags.");
                return;
            }

            var newTags = GetTrackMetadata(trackfile);
            var path = Path.Combine(trackfile.Artist.Value.Path, trackfile.RelativePath);

            var diff = ReadAudioTag(path).Diff(newTags);

            if (_configService.ScrubAudioTags)
            {
                _logger.Debug($"Scrubbing tags for {trackfile}");
                RemoveAllTags(path);
            }

            _logger.Debug($"Writing tags for {trackfile}");
            newTags.Write(path);

            UpdateTrackfileSize(trackfile, path);
            
            _eventAggregator.PublishEvent(new TrackFileRetaggedEvent(trackfile.Artist.Value, trackfile, diff, _configService.ScrubAudioTags));
        }

        public void SyncTags(List<Track> tracks)
        {
            if (_configService.WriteAudioTags != WriteAudioTagsType.Sync)
            {
                return;
            }

            // get the tracks to update
            var trackFiles = _mediaFileService.Get(tracks.Where(x => x.TrackFileId > 0).Select(x => x.TrackFileId));

            _logger.Debug($"Syncing audio tags for {trackFiles.Count} files");

            foreach (var file in trackFiles)
            {
                // populate tracks (which should also have release/album/artist set) because
                // not all of the updates will have been committed to the database yet
                file.Tracks = tracks.Where(x => x.TrackFileId == file.Id).ToList();
                WriteTags(file, false);
            }
        }

        public void RemoveMusicBrainzTags(IEnumerable<Album> albums)
        {
            if (_configService.WriteAudioTags < WriteAudioTagsType.AllFiles)
            {
                return;
            }

            foreach (var album in albums)
            {
                var files = _mediaFileService.GetFilesByAlbum(album.Id);
                foreach (var file in files)
                {
                    RemoveMusicBrainzTags(file);
                }
            }
        }

        public void RemoveMusicBrainzTags(IEnumerable<AlbumRelease> releases)
        {
            if (_configService.WriteAudioTags < WriteAudioTagsType.AllFiles)
            {
                return;
            }

            foreach (var release in releases)
            {
                var files = _mediaFileService.GetFilesByRelease(release.Id);
                foreach (var file in files)
                {
                    RemoveMusicBrainzTags(file);
                }
            }
        }

        public void RemoveMusicBrainzTags(IEnumerable<Track> tracks)
        {
            if (_configService.WriteAudioTags < WriteAudioTagsType.AllFiles)
            {
                return;
            }

            var files = _mediaFileService.Get(tracks.Where(x => x.TrackFileId > 0).Select(x => x.TrackFileId));
            foreach (var file in files)
            {
                RemoveMusicBrainzTags(file);
            }
        }

        public void RemoveMusicBrainzTags(TrackFile trackfile)
        {
            if (_configService.WriteAudioTags < WriteAudioTagsType.AllFiles)
            {
                return;
            }

            var path = Path.Combine(trackfile.Artist.Value.Path, trackfile.RelativePath);
            _logger.Debug($"Removing MusicBrainz tags for {path}");

            RemoveMusicBrainzTags(path);
            
            UpdateTrackfileSize(trackfile, path);
        }

        public List<RetagTrackFilePreview> GetRetagPreviewsByArtist(int artistId)
        {
            var files = _mediaFileService.GetFilesByArtist(artistId);

            return GetPreviews(files).ToList();
        }

        public List<RetagTrackFilePreview> GetRetagPreviewsByAlbum(int albumId)
        {
            var files = _mediaFileService.GetFilesByAlbum(albumId);

            return GetPreviews(files).ToList();
        }

        private IEnumerable<RetagTrackFilePreview> GetPreviews(List<TrackFile> files)
        {
            foreach (var f in files.OrderBy(x => x.Album.Value.Title)
                     .ThenBy(x => x.Tracks.Value.First().MediumNumber)
                     .ThenBy(x => x.Tracks.Value.First().AbsoluteTrackNumber))
            {
                var file = f;

                if (!f.Tracks.Value.Any())
                {
                    _logger.Warn($"File {f} is not linked to any tracks");
                    continue;
                }

                if (f.Tracks.Value.Count > 1)
                {
                    _logger.Debug($"File {f} is linked to multiple tracks. Not writing tags.");
                    continue;
                }

                var oldTags = ReadAudioTag(Path.Combine(f.Artist.Value.Path, f.RelativePath));
                var newTags = GetTrackMetadata(f);
                var diff = oldTags.Diff(newTags);

                if (diff.Any())
                {
                    yield return new RetagTrackFilePreview {
                        ArtistId = file.Artist.Value.Id,
                        AlbumId = file.Album.Value.Id,
                        TrackNumbers = file.Tracks.Value.Select(e => e.AbsoluteTrackNumber).ToList(),
                        TrackFileId = file.Id,
                        RelativePath = file.RelativePath,
                        Changes = diff
                    };
                }
            }
        }

        public void Execute(RetagFilesCommand message)
        {
            var artist = _artistService.GetArtist(message.ArtistId);
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
            var artistToRename = _artistService.GetArtists(message.ArtistIds);

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
