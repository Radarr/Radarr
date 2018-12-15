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
        List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId);
        Album AddAlbum(Album newAlbum, string albumArtistId);
        Album FindById(string spotifyId);
        Album FindByTitle(int artistId, string title);
        Album FindByTitleInexact(int artistId, string title);
        void DeleteAlbum(int albumId, bool deleteFiles);
        List<Album> GetAllAlbums();
        Album UpdateAlbum(Album album);
        List<Album> UpdateAlbums(List<Album> album);
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
                                IHandleAsync<ArtistDeletedEvent>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;

        public AlbumService(IAlbumRepository albumRepository,
                            IReleaseRepository releaseRepository,
                            IArtistMetadataRepository artistMetadataRepository,
                            IEventAggregator eventAggregator,
                            ITrackService trackService,
                            Logger logger)
        {
            _albumRepository = albumRepository;
            _releaseRepository = releaseRepository;
            _artistMetadataRepository = artistMetadataRepository;
            _eventAggregator = eventAggregator;
            _trackService = trackService;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum, string albumArtistId)
        {
            _albumRepository.Insert(newAlbum);
            
            foreach (var release in newAlbum.AlbumReleases.Value)
            {
                release.AlbumId = newAlbum.Id;
            }
            _releaseRepository.InsertMany(newAlbum.AlbumReleases.Value);
            
            newAlbum.ArtistMetadata = _artistMetadataRepository.FindById(albumArtistId);
            newAlbum.ArtistMetadataId = newAlbum.ArtistMetadata.Value.Id;
            
            _albumRepository.Update(newAlbum);
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

        public Album FindByTitle(int artistId, string title)
        {
            return _albumRepository.FindByTitle(artistId, title);
        }

        public Album FindByTitleInexact(int artistId, string title)
        {
            var cleanTitle = title.CleanArtistName();

            var albums = GetAlbumsByArtistMetadataId(artistId);

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

            foreach (var func in scoringFunctions)
            {
                var album = FindByStringInexact(albums, func.Item1, func.Item2);
                if (album != null)
                {
                    return album;
                }
            }

            return null;
        }

        private Album FindByStringInexact(List<Album> albums, Func<Album, string, double> scoreFunction, string title)
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

            if (!sortedAlbums.Any())
            {
                return null;
            }

            _logger.Trace("\nFuzzy album match on '{0}':\n{1}",
                          title,
                          string.Join("\n", sortedAlbums.Select(x => $"[{x.Album.Title}] {x.Album.CleanTitle}: {x.MatchProb}")));

            if (sortedAlbums[0].MatchProb > fuzzThreshold
                && (sortedAlbums.Count == 1 || sortedAlbums[0].MatchProb - sortedAlbums[1].MatchProb > fuzzGap))
            {
                return sortedAlbums[0].Album;
            }

            return null;
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

        public List<Album> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return _albumRepository.GetAlbumsByArtistMetadataId(artistMetadataId).ToList();
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

            _logger.Debug("Monitored flag for Album:{0} was set to {1}", albumId, monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            _albumRepository.SetMonitored(ids, monitored);
        }

        public List<Album> UpdateAlbums(List<Album> albums)
        {
            _logger.Debug("Updating {0} albums", albums.Count);

            _albumRepository.UpdateMany(albums);
            _logger.Debug("{0} albums updated", albums.Count);

            return albums;
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            var albums = GetAlbumsByArtistMetadataId(message.Artist.ArtistMetadataId);
            DeleteMany(albums);
        }
    }
}
