using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpdateTrackFileService
    {
        void ChangeFileDateForFile(TrackFile trackFile, Artist artist, List<Track> tracks);
    }

    public class UpdateTrackFileService : IUpdateTrackFileService,
                                            IHandle<ArtistScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAlbumService _albumService;
        private readonly IConfigService _configService;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UpdateTrackFileService(IDiskProvider diskProvider,
                                        IConfigService configService,
                                        ITrackService trackService,
                                        IAlbumService albumService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _trackService = trackService;
            _albumService = albumService;
            _logger = logger;
        }

        public void ChangeFileDateForFile(TrackFile trackFile, Artist artist, List<Track> tracks)
        {
            ChangeFileDate(trackFile, artist, tracks);
        }

        private bool ChangeFileDate(TrackFile trackFile, Artist artist, List<Track> tracks)
        {
            var trackFilePath = trackFile.Path;

            switch (_configService.FileDate)
            {
                case FileDateType.AlbumReleaseDate:
                    {
                        var album = _albumService.GetAlbum(trackFile.AlbumId);

                        if (!album.ReleaseDate.HasValue)
                        {
                            _logger.Debug("Could not create valid date to change file [{0}]", trackFilePath);
                            return false;
                        }

                        var relDate = album.ReleaseDate.Value;

                        // avoiding false +ve checks and set date skewing by not using UTC (Windows)
                        DateTime oldDateTime = _diskProvider.FileGetLastWrite(trackFilePath);

                        if (OsInfo.IsNotWindows && relDate < EpochTime)
                        {
                            _logger.Debug("Setting date of file to 1970-01-01 as actual airdate is before that time and will not be set properly");
                            relDate = EpochTime;
                        }

                        if (!DateTime.Equals(relDate, oldDateTime))
                        {
                            try
                            {
                                _diskProvider.FileSetLastWriteTime(trackFilePath, relDate);
                                _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", trackFilePath, oldDateTime, relDate);

                                return true;
                            }

                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to set date of file [" + trackFilePath + "]");
                            }
                        }

                        return false;
                    }
            }

            return false;
        }

        public void Handle(ArtistScannedEvent message)
        {
            if (_configService.FileDate == FileDateType.None)
            {
                return;
            }

            var tracks = _trackService.TracksWithFiles(message.Artist.Id);

            var trackFiles = new List<TrackFile>();
            var updated = new List<TrackFile>();

            foreach (var group in tracks.GroupBy(e => e.TrackFileId))
            {
                var tracksInFile = group.Select(e => e).ToList();
                var trackFile = tracksInFile.First().TrackFile;

                trackFiles.Add(trackFile);

                if (ChangeFileDate(trackFile, message.Artist, tracksInFile))
                {
                    updated.Add(trackFile);
                }
            }

            if (updated.Any())
            {
                _logger.ProgressDebug("Changed file date for {0} files of {1} in {2}", updated.Count, trackFiles.Count, message.Artist.Name);
            }

            else
            {
                _logger.ProgressDebug("No file dates changed for {0}", message.Artist.Name);
            }
        }
    }
}
