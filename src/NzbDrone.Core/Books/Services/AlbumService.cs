using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService
    {
        Book GetAlbum(int bookId);
        List<Book> GetAlbums(IEnumerable<int> bookIds);
        List<Book> GetAlbumsByArtist(int authorId);
        List<Book> GetNextAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds);
        List<Book> GetLastAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds);
        List<Book> GetAlbumsByArtistMetadataId(int artistMetadataId);
        List<Book> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds);
        List<Book> GetAlbumsByFileIds(IEnumerable<int> fileIds);
        Book AddAlbum(Book newAlbum, bool doRefresh = true);
        Book FindById(string foreignId);
        Book FindBySlug(string titleSlug);
        Book FindByTitle(int artistMetadataId, string title);
        Book FindByTitleInexact(int artistMetadataId, string title);
        List<Book> GetCandidates(int artistMetadataId, string title);
        void DeleteAlbum(int bookId, bool deleteFiles, bool addImportListExclusion = false);
        List<Book> GetAllAlbums();
        Book UpdateAlbum(Book album);
        void SetAlbumMonitored(int bookId, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        PagingSpec<Book> AlbumsWithoutFiles(PagingSpec<Book> pagingSpec);
        List<Book> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Book> ArtistAlbumsBetweenDates(Author artist, DateTime start, DateTime end, bool includeUnmonitored);
        void InsertMany(List<Book> albums);
        void UpdateMany(List<Book> albums);
        void DeleteMany(List<Book> albums);
        void SetAddOptions(IEnumerable<Book> albums);
        List<Book> GetArtistAlbumsWithFiles(Author artist);
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

        public Book AddAlbum(Book newAlbum, bool doRefresh = true)
        {
            _albumRepository.Insert(newAlbum);

            _eventAggregator.PublishEvent(new AlbumAddedEvent(GetAlbum(newAlbum.Id), doRefresh));

            return newAlbum;
        }

        public void DeleteAlbum(int bookId, bool deleteFiles, bool addImportListExclusion = false)
        {
            var album = _albumRepository.Get(bookId);
            album.Author.LazyLoad();
            _albumRepository.Delete(bookId);
            _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, deleteFiles, addImportListExclusion));
        }

        public Book FindById(string foreignId)
        {
            return _albumRepository.FindById(foreignId);
        }

        public Book FindBySlug(string titleSlug)
        {
            return _albumRepository.FindBySlug(titleSlug);
        }

        public Book FindByTitle(int artistMetadataId, string title)
        {
            return _albumRepository.FindByTitle(artistMetadataId, title);
        }

        private List<Tuple<Func<Book, string, double>, string>> AlbumScoringFunctions(string title, string cleanTitle)
        {
            Func<Func<Book, string, double>, string, Tuple<Func<Book, string, double>, string>> tc = Tuple.Create;
            var scoringFunctions = new List<Tuple<Func<Book, string, double>, string>>
            {
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

        public Book FindByTitleInexact(int artistMetadataId, string title)
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

        public List<Book> GetCandidates(int artistMetadataId, string title)
        {
            var albums = GetAlbumsByArtistMetadataId(artistMetadataId);
            var output = new List<Book>();

            foreach (var func in AlbumScoringFunctions(title, title.CleanArtistName()))
            {
                output.AddRange(FindByStringInexact(albums, func.Item1, func.Item2));
            }

            return output.DistinctBy(x => x.Id).ToList();
        }

        private List<Book> FindByStringInexact(List<Book> albums, Func<Book, string, double> scoreFunction, string title)
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

            return sortedAlbums.TakeWhile((x, i) => i == 0 || sortedAlbums[i - 1].MatchProb - x.MatchProb < fuzzGap)
                .TakeWhile((x, i) => x.MatchProb > fuzzThreshold || (i > 0 && sortedAlbums[i - 1].MatchProb > fuzzThreshold))
                .Select(x => x.Album)
                .ToList();
        }

        public List<Book> GetAllAlbums()
        {
            return _albumRepository.All().ToList();
        }

        public Book GetAlbum(int bookId)
        {
            return _albumRepository.Get(bookId);
        }

        public List<Book> GetAlbums(IEnumerable<int> bookIds)
        {
            return _albumRepository.Get(bookIds).ToList();
        }

        public List<Book> GetAlbumsByArtist(int authorId)
        {
            return _albumRepository.GetAlbums(authorId).ToList();
        }

        public List<Book> GetNextAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return _albumRepository.GetNextAlbums(artistMetadataIds).ToList();
        }

        public List<Book> GetLastAlbumsByArtistMetadataId(IEnumerable<int> artistMetadataIds)
        {
            return _albumRepository.GetLastAlbums(artistMetadataIds).ToList();
        }

        public List<Book> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return _albumRepository.GetAlbumsByArtistMetadataId(artistMetadataId).ToList();
        }

        public List<Book> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds)
        {
            return _albumRepository.GetAlbumsForRefresh(artistMetadataId, foreignIds);
        }

        public List<Book> GetAlbumsByFileIds(IEnumerable<int> fileIds)
        {
            return _albumRepository.GetAlbumsByFileIds(fileIds);
        }

        public void SetAddOptions(IEnumerable<Book> albums)
        {
            _albumRepository.SetFields(albums.ToList(), s => s.AddOptions);
        }

        public PagingSpec<Book> AlbumsWithoutFiles(PagingSpec<Book> pagingSpec)
        {
            var albumResult = _albumRepository.AlbumsWithoutFiles(pagingSpec);

            return albumResult;
        }

        public List<Book> AlbumsBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var albums = _albumRepository.AlbumsBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return albums;
        }

        public List<Book> ArtistAlbumsBetweenDates(Author artist, DateTime start, DateTime end, bool includeUnmonitored)
        {
            var albums = _albumRepository.ArtistAlbumsBetweenDates(artist, start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return albums;
        }

        public List<Book> GetArtistAlbumsWithFiles(Author artist)
        {
            return _albumRepository.GetArtistAlbumsWithFiles(artist);
        }

        public void InsertMany(List<Book> albums)
        {
            _albumRepository.InsertMany(albums);
        }

        public void UpdateMany(List<Book> albums)
        {
            _albumRepository.UpdateMany(albums);
        }

        public void DeleteMany(List<Book> albums)
        {
            _albumRepository.DeleteMany(albums);

            foreach (var album in albums)
            {
                _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, false, false));
            }
        }

        public Book UpdateAlbum(Book album)
        {
            var storedAlbum = GetAlbum(album.Id);
            var updatedAlbum = _albumRepository.Update(album);

            _eventAggregator.PublishEvent(new AlbumEditedEvent(updatedAlbum, storedAlbum));

            return updatedAlbum;
        }

        public void SetAlbumMonitored(int bookId, bool monitored)
        {
            var album = _albumRepository.Get(bookId);
            _albumRepository.SetMonitoredFlat(album, monitored);

            // publish album edited event so artist stats update
            _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));

            _logger.Debug("Monitored flag for Album:{0} was set to {1}", bookId, monitored);
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
            var albums = GetAlbumsByArtistMetadataId(message.Artist.AuthorMetadataId);
            DeleteMany(albums);
        }
    }
}
