using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService
    {
        Album GetAlbum(int albumId);
        List<Album> GetAlbums(IEnumerable<int> albumIds);
        List<Album> GetAlbumsByArtist(int artistId);
        List<Album> GetNextAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds);
        List<Album> GetLastAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds);
        List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId);
        List<Album> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds);
        Album AddAlbum(Album newAlbum);
        Album FindById(string foreignId);
        Album FindByTitle(int artistMetadataId, string title);
        Album FindByTitleInexact(int artistMetadataId, string title);
        List<Album> GetCandidates(int artistId, string title);
        void DeleteAlbum(int albumId, bool deleteFiles);
        List<Album> GetAllAlbums();
        Album UpdateAlbum(Album album);
        void SetAlbumMonitored(int albumId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec);
        List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime start, DateTime end, bool includeUnmonitored);
        void InsertMany(List<Album> albums);
        void UpdateMany(List<Album> albums);
        void DeleteMany(List<Album> albums);
        void RemoveAddOptions(Album album);
        Album FindAlbumByRelease(string albumReleaseId);
        Album FindAlbumByTrackId(int trackId);
        List<Album> GetArtistAlbumsWithFiles(Artist artist);
    }

    public class AlbumService : IAlbumService,
                                IHandle<ArtistDeletedEvent>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public AlbumService(IAlbumRepository albumRepository,
                            IEventAggregator eventAggregator,
                            Logger logger)
        {
            _albumRepository = albumRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            _albumRepository.Insert(newAlbum);

            //_eventAggregator.PublishEvent(new AlbumAddedEvent(GetAlbum(newAlbum.Id)));

            return newAlbum;
        }

        public void DeleteAlbum(int albumId, bool deleteFiles)
        {
            var album = _albumRepository.Get(albumId);
            _albumRepository.Delete(albumId);
            _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, deleteFiles));
        }

        public Album FindById(string lidarrId)
        {
            return _albumRepository.FindById(lidarrId);
        }

        public Album FindByTitle(int artistMetadataId, string title)
        {
            return _albumRepository.FindByTitle(artistMetadataId, title);
        }

        private List<Tuple<Func<Album, string, double>, string>> AlbumScoringFunctions(string title, string cleanTitle)
        {
            Func< Func<Album, string, double>, string, Tuple<Func<Album, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Album, string, double>, string>> {
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), cleanTitle),
                tc((a, t) => a.Title.FuzzyMatch(t), title),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveBracketsAndContents().CleanArtistName()),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveAfterDash().CleanArtistName()),
                tc((a, t) => a.CleanTitle.FuzzyMatch(t), title.RemoveBracketsAndContents().RemoveAfterDash().CleanArtistName()),
                tc((a, t) => t.FuzzyContains(a.CleanTitle), cleanTitle),
                tc((a, t) => t.FuzzyContains(a.Title), title)
            };

            return scoringFunctions;
        }

        public Album FindByTitleInexact(int artistMetadataId, string title)
        {
            var albums = GetAlbumsByArtistMetadataId(artistMetadataId);

            foreach (var func in AlbumScoringFunctions(title, title.CleanArtistName()))
            {
                var results = FindByStringInexact(albums, func.Item1, func.Item2);
                if (results.Count == 1)
                {
                    return results[0];
                }
            }

            return null;
        }

        public List<Album> GetCandidates(int artistMetadataId, string title)
        {
            var albums = GetAlbumsByArtistMetadataId(artistMetadataId);
            var output = new List<Album>();
            
            foreach (var func in AlbumScoringFunctions(title, title.CleanArtistName()))
            {
                output.AddRange(FindByStringInexact(albums, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        private List<Album> FindByStringInexact(List<Album> albums, Func<Album, string, double> scoreFunction, string title)
        {
            const double fuzzThreshold = 0.7;
            const double fuzzGap = 0.4;

            var sortedAlbums = albums.Select(s => new
                {
                    MatchProb = scoreFunction(s, title),
                    Album = s
                })
                .ToList()
                .OrderByDescending(s => s.MatchProb)
                .ToList();

            _logger.Trace("\nFuzzy album match on '{0}':\n{1}",
                          title,
                          string.Join("\n", sortedAlbums.Select(x => $"[{x.Album.Title}] {x.Album.CleanTitle}: {x.MatchProb}")));

            return sortedAlbums.TakeWhile((x, i) => i == 0 ? true : sortedAlbums[i - 1].MatchProb - x.MatchProb < fuzzGap)
                .TakeWhile((x, i) => x.MatchProb > fuzzThreshold || (i > 0 && sortedAlbums[i - 1].MatchProb > fuzzThreshold))
                .Select(x => x.Album)
                .ToList();
        }

        public List<Album> GetAllAlbums()
        {
            return _albumRepository.All().ToList();
        }

        public Album GetAlbum(int albumId)
        {
            return _albumRepository.Get(albumId);
        }

        public List<Album> GetAlbums(IEnumerable<int> albumIds)
        {
            return _albumRepository.Get(albumIds).ToList();
        }

        public List<Album> GetAlbumsByArtist(int artistId)
        {
            return _albumRepository.GetAlbums(artistId).ToList();
        }

        public List<Album> GetNextAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return _albumRepository.GetNextAlbums(artistMetadataIds).ToList();
        }

        public List<Album> GetLastAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return _albumRepository.GetLastAlbums(artistMetadataIds).ToList();
        }

        public List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return _albumRepository.GetAlbumsByArtistMetadataId(artistMetadataId).ToList();
        }

        public List<Album> GetAlbumsForRefresh(int artistId, IEnumerable<string> foreignIds)
        {
            return _albumRepository.GetAlbumsForRefresh(artistId, foreignIds);
        }

        public Album FindAlbumByRelease(string albumReleaseId)
        {
            return _albumRepository.FindAlbumByRelease(albumReleaseId);
        }

        public Album FindAlbumByTrackId(int trackId)
        {
            return _albumRepository.FindAlbumByTrack(trackId);
        }

        public void RemoveAddOptions(Album album)
        {
            var rg = _albumRepository.Get(album.Id);
            _albumRepository.SetFields(rg, s => s.AddOptions);
        }

        public PagingSpec<Album> AlbumsWithoutFiles(PagingSpec<Album> pagingSpec)
        {
            var albumResult = _albumRepository.AlbumsWithoutFiles(pagingSpec);

            return albumResult;
        }

        public List<Album> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var albums = _albumRepository.AlbumsBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return albums;
        }

        public List<Album> ArtistAlbumsBetweenDates(Artist artist, DateTime start, DateTime end, bool includeUnmonitored)
        {
            var albums = _albumRepository.ArtistAlbumsBetweenDates(artist, start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return albums;
        }

        public List<Album> GetArtistAlbumsWithFiles(Artist artist)
        {
            return _albumRepository.GetArtistAlbumsWithFiles(artist);
        }

        public void InsertMany(List<Album> albums)
        {
            _albumRepository.InsertMany(albums);
        }

        public void UpdateMany(List<Album> albums)
        {
            _albumRepository.UpdateMany(albums);
        }

        public void DeleteMany(List<Album> albums)
        {
            _albumRepository.DeleteMany(albums);

            foreach (var album in albums)
            {
                _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, false));
            }
        }

        public Album UpdateAlbum(Album album)
        {
            var storedAlbum = GetAlbum(album.Id);
            var updatedAlbum = _albumRepository.Update(album);

            // If updatedAlbum has populated the Releases, populate in the storedAlbum too
            if (updatedAlbum.AlbumReleases.IsLoaded)
            {
                storedAlbum.AlbumReleases.LazyLoad();
            }
            _eventAggregator.PublishEvent(new AlbumEditedEvent(updatedAlbum, storedAlbum));

            return updatedAlbum;
        }

        public void SetAlbumMonitored(int albumId, bool monitored)
        {
            var album = _albumRepository.Get(albumId);
            _albumRepository.SetMonitoredFlat(album, monitored);

            // publish album edited event so artist stats update
            _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));

            _logger.Debug("Monitored flag for Album:{0} was set to {1}", albumId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _albumRepository.SetMonitored(ids, monitored);

            // publish album edited event so artist stats update
            foreach (var album in _albumRepository.Get(ids))
            {
                _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));
            }
        }

        public void Handle(ArtistDeletedEvent message)
        {
            var albums = GetAlbumsByArtistMetadataId(message.Artist.ArtistMetadataId);
            DeleteMany(albums);
        }
    }
}
