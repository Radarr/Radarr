using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public interface ITrackService
    {
        Track GetTrack(int id);
        List<Track> GetTracks(IEnumerable<int> ids);
        Track FindTrack(int artistId, int albumId, int mediumNumber, int trackNumber);
        Track FindTrackByTitle(int artistId, int albumId, int mediumNumber, int trackNumber, string releaseTitle);
        List<Track> GetTracksByArtist(int artistId);
        List<Track> GetTracksByAlbum(int albumId);
        //List<Track> GetTracksByAlbumTitle(string artistId, string albumTitle);
        List<Track> TracksWithFiles(int artistId);
        //PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec);
        List<Track> GetTracksByFileId(int trackFileId);
        void UpdateTrack(Track track);
        void SetTrackMonitored(int trackId, bool monitored);
        void UpdateTracks(List<Track> tracks);
        void InsertMany(List<Track> tracks);
        void UpdateMany(List<Track> tracks);
        void DeleteMany(List<Track> tracks);
        void SetTrackMonitoredByAlbum(int artistId, int albumId, bool monitored);
    }

    public class TrackService : ITrackService,
                                IHandleAsync<ArtistDeletedEvent>,
                                IHandleAsync<AlbumDeletedEvent>,
                                IHandle<TrackFileDeletedEvent>,
                                IHandle<TrackFileAddedEvent>
    {
        private readonly ITrackRepository _trackRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TrackService(ITrackRepository trackRepository, IConfigService configService, Logger logger)
        {
            _trackRepository = trackRepository;
            _configService = configService;
            _logger = logger;
        }

        public Track GetTrack(int id)
        {
            return _trackRepository.Get(id);
        }

        public List<Track> GetTracks(IEnumerable<int> ids)
        {
            return _trackRepository.Get(ids).ToList();
        }

        public Track FindTrack(int artistId, int albumId, int mediumNumber, int trackNumber)
        {
            return _trackRepository.Find(artistId, albumId, mediumNumber, trackNumber);
        }

        public List<Track> GetTracksByArtist(int artistId)
        {
            _logger.Debug("Getting Tracks for ArtistId {0}", artistId);
            return _trackRepository.GetTracks(artistId).ToList();
        }

        public List<Track> GetTracksByAlbum(int albumId)
        {
            return _trackRepository.GetTracksByAlbum(albumId);
        }

        public Track FindTrackByTitle(int artistId, int albumId, int mediumNumber, int trackNumber, string releaseTitle)
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var normalizedReleaseTitle = Parser.Parser.NormalizeEpisodeTitle(releaseTitle).Replace(".", " ");
            var tracks = _trackRepository.GetTracksByMedium(albumId, mediumNumber);

            var matches = tracks.Select(
                track => new
                {
                    Position = normalizedReleaseTitle.IndexOf(Parser.Parser.NormalizeEpisodeTitle(track.Title), StringComparison.CurrentCultureIgnoreCase),
                    Length = Parser.Parser.NormalizeEpisodeTitle(track.Title).Length,
                    Track = track
                });

            if (trackNumber == 0)
            {
                matches =  matches.Where(e => e.Track.Title.Length > 0 && e.Position >= 0);
            } else
            {
                matches = matches.Where(e => e.Track.Title.Length > 0 && e.Position >= 0 && e.Track.AbsoluteTrackNumber == trackNumber);
            }
 
            matches.OrderBy(e => e.Position)
                    .ThenByDescending(e => e.Length)
                    .ToList();

            if (matches.Any())
            {
                return matches.First().Track;
            }

            return null;
        }

        public List<Track> TracksWithFiles(int artistId)
        {
            return _trackRepository.TracksWithFiles(artistId);
        }


        public PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec)
        {
            var episodeResult = _trackRepository.TracksWithoutFiles(pagingSpec);

            return episodeResult;
        }

        public List<Track> GetTracksByFileId(int trackFileId)
        {
            return _trackRepository.GetTracksByFileId(trackFileId);
        }

        public void UpdateTrack(Track track)
        {
            _trackRepository.Update(track);
        }

        public void SetTrackMonitored(int trackId, bool monitored)
        {
            var track = _trackRepository.Get(trackId);
            _trackRepository.SetMonitoredFlat(track, monitored);

            _logger.Debug("Monitored flag for Track:{0} was set to {1}", trackId, monitored);
        }

        public void SetTrackMonitoredByAlbum(int artistId, int albumId, bool monitored)
        {
            _trackRepository.SetMonitoredByAlbum(artistId, albumId, monitored);
        }

        public void UpdateTracks(List<Track> tracks)
        {
            _trackRepository.UpdateMany(tracks);
        }

        public void InsertMany(List<Track> tracks)
        {
            _trackRepository.InsertMany(tracks);
        }

        public void UpdateMany(List<Track> tracks)
        {
            _trackRepository.UpdateMany(tracks);
        }

        public void DeleteMany(List<Track> tracks)
        {
            _trackRepository.DeleteMany(tracks);
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            var tracks = GetTracksByArtist(message.Artist.Id);
            _trackRepository.DeleteMany(tracks);
        }

        public void HandleAsync(AlbumDeletedEvent message)
        {
            var tracks = GetTracksByAlbum(message.Album.Id);
            _trackRepository.DeleteMany(tracks);
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            foreach (var track in GetTracksByFileId(message.TrackFile.Id))
            {
                _logger.Debug("Detaching track {0} from file.", track.Id);
                track.TrackFileId = 0;

                if (message.Reason != DeleteMediaFileReason.Upgrade && _configService.AutoUnmonitorPreviouslyDownloadedTracks)
                {
                    track.Monitored = false;
                }

                UpdateTrack(track);
            }
        }

        public void Handle(TrackFileAddedEvent message)
        {
            foreach (var track in message.TrackFile.Tracks.Value)
            {
                _trackRepository.SetFileId(track.Id, message.TrackFile.Id);
                _logger.Debug("Linking [{0}] > [{1}]", message.TrackFile.RelativePath, track);
            }
        }
    }
}
