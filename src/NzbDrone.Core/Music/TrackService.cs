using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Extensions;
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
        Track FindTrackByTitleInexact(int artistId, int albumId, int mediumNumber, int trackNumber, string releaseTitle);
        List<Track> GetTracksByArtist(int artistId);
        List<Track> GetTracksByAlbum(int albumId);
        List<Track> GetTracksByRelease(int albumReleaseId);
        List<Track> GetTracksByForeignReleaseId(string foreignReleaseId);
        List<Track> GetTracksByForeignTrackIds(List<string> ids);
        List<Track> TracksWithFiles(int artistId);
        List<Track> GetTracksByFileId(int trackFileId);
        void UpdateTrack(Track track);
        void UpdateTracks(List<Track> tracks);
        void InsertMany(List<Track> tracks);
        void UpdateMany(List<Track> tracks);
        void DeleteMany(List<Track> tracks);
    }

    public class TrackService : ITrackService,
                                IHandleAsync<ReleaseDeletedEvent>,
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

        public List<Track> GetTracksByRelease(int albumReleaseId)
        {
            return _trackRepository.GetTracksByRelease(albumReleaseId);
        }

        public List<Track> GetTracksByForeignReleaseId(string foreignReleaseId)
        {
            return _trackRepository.GetTracksByForeignReleaseId(foreignReleaseId);
        }

        public List<Track> GetTracksByForeignTrackIds(List<string> ids)
        {
            return _trackRepository.GetTracksByForeignTrackIds(ids);
        }

        public Track FindTrackByTitle(int artistId, int albumId, int mediumNumber, int trackNumber, string releaseTitle)
        {
            // TODO: can replace this search mechanism with something smarter/faster/better
            var normalizedReleaseTitle = releaseTitle.NormalizeTrackTitle().Replace(".", " ");
            var tracks = _trackRepository.GetTracksByMedium(albumId, mediumNumber);

            var matches = tracks.Where(t => (trackNumber == 0 || t.AbsoluteTrackNumber == trackNumber)
                                       && t.Title.Length > 0
                                       && (normalizedReleaseTitle.Contains(t.Title.NormalizeTrackTitle())
                                           || t.Title.NormalizeTrackTitle().Contains(normalizedReleaseTitle)));

            return matches.Count() > 1 ? null : matches.SingleOrDefault();
        }

        public Track FindTrackByTitleInexact(int artistId, int albumId, int mediumNumber, int trackNumber, string title)
        {
            var normalizedTitle = title.NormalizeTrackTitle().Replace(".", " ");
            var tracks = _trackRepository.GetTracksByMedium(albumId, mediumNumber);

            Func< Func<Track, string, double>, string, Tuple<Func<Track, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Track, string, double>, string>> {
                tc((a, t) => a.Title.NormalizeTrackTitle().FuzzyMatch(t), normalizedTitle),
                tc((a, t) => a.Title.NormalizeTrackTitle().FuzzyContains(t), normalizedTitle),
                tc((a, t) => t.FuzzyContains(a.Title.NormalizeTrackTitle()), normalizedTitle)
            };

            foreach (var func in scoringFunctions)
            {
                var track = FindByStringInexact(tracks, func.Item1, func.Item2, trackNumber);
                if (track != null)
                {
                    return track;
                }
            }

            return null;
        }

        private Track FindByStringInexact(List<Track> tracks, Func<Track, string, double> scoreFunction, string title, int trackNumber)
        {
            const double fuzzThreshold = 0.7;
            const double fuzzGap = 0.2;

            var sortedTracks = tracks.Select(s => new
                {
                    MatchProb = scoreFunction(s, title),
                    Track = s
                })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            if (!sortedTracks.Any())
            {
                return null;
            }

            _logger.Trace("\nFuzzy track match on '{0:D2} - {1}':\n{2}",
                          trackNumber,
                          title,
                          string.Join("\n", sortedTracks.Select(x => $"{x.Track.AbsoluteTrackNumber:D2} - {x.Track.Title}: {x.MatchProb}")));

            if (sortedTracks[0].MatchProb > fuzzThreshold
                && (sortedTracks.Count == 1 || sortedTracks[0].MatchProb - sortedTracks[1].MatchProb > fuzzGap)
                && (trackNumber == 0
                    || sortedTracks[0].Track.AbsoluteTrackNumber == trackNumber
                    || sortedTracks[0].Track.AbsoluteTrackNumber + tracks.Count(t => t.MediumNumber < sortedTracks[0].Track.MediumNumber) == trackNumber))
            {
                return sortedTracks[0].Track;
            }

            return null;
        }

        public List<Track> TracksWithFiles(int artistId)
        {
            return _trackRepository.TracksWithFiles(artistId);
        }

        public List<Track> GetTracksByFileId(int trackFileId)
        {
            return _trackRepository.GetTracksByFileId(trackFileId);
        }

        public void UpdateTrack(Track track)
        {
            _trackRepository.Update(track);
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

        public void HandleAsync(ReleaseDeletedEvent message)
        {
            var tracks = GetTracksByRelease(message.Release.Id);
            _trackRepository.DeleteMany(tracks);
        }

        public void Handle(TrackFileDeletedEvent message)
        {
            foreach (var track in GetTracksByFileId(message.TrackFile.Id))
            {
                _logger.Debug("Detaching track {0} from file.", track.Id);
                track.TrackFileId = 0;
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
