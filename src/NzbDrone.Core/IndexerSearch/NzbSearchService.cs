using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForNzb
    {
        List<DownloadDecision> AlbumSearch(int bookId, bool missingOnly, bool userInvokedSearch, bool interactiveSearch);
        List<DownloadDecision> ArtistSearch(int authorId, bool missingOnly, bool userInvokedSearch, bool interactiveSearch);
    }

    public class NzbSearchService : ISearchForNzb
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly Logger _logger;

        public NzbSearchService(IIndexerFactory indexerFactory,
                                IAlbumService albumService,
                                IArtistService artistService,
                                IMakeDownloadDecision makeDownloadDecision,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _albumService = albumService;
            _artistService = artistService;
            _makeDownloadDecision = makeDownloadDecision;
            _logger = logger;
        }

        public List<DownloadDecision> AlbumSearch(int bookId, bool missingOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var album = _albumService.GetAlbum(bookId);
            return AlbumSearch(album, missingOnly, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> ArtistSearch(int authorId, bool missingOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var artist = _artistService.GetArtist(authorId);
            return ArtistSearch(artist, missingOnly, userInvokedSearch, interactiveSearch);
        }

        public List<DownloadDecision> ArtistSearch(Author artist, bool missingOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var searchSpec = Get<ArtistSearchCriteria>(artist, userInvokedSearch, interactiveSearch);
            var albums = _albumService.GetAlbumsByArtist(artist.Id);

            albums = albums.Where(a => a.Monitored).ToList();

            searchSpec.Albums = albums;

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        public List<DownloadDecision> AlbumSearch(Book album, bool missingOnly, bool userInvokedSearch, bool interactiveSearch)
        {
            var artist = _artistService.GetArtist(album.AuthorId);

            var searchSpec = Get<AlbumSearchCriteria>(artist, new List<Book> { album }, userInvokedSearch, interactiveSearch);

            searchSpec.AlbumTitle = album.Title;
            if (album.ReleaseDate.HasValue)
            {
                searchSpec.AlbumYear = album.ReleaseDate.Value.Year;
            }

            if (album.Disambiguation.IsNotNullOrWhiteSpace())
            {
                searchSpec.Disambiguation = album.Disambiguation;
            }

            return Dispatch(indexer => indexer.Fetch(searchSpec), searchSpec);
        }

        private TSpec Get<TSpec>(Author artist, List<Book> albums, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();

            spec.Albums = albums;
            spec.Artist = artist;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private static TSpec Get<TSpec>(Author artist, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec();
            spec.Artist = artist;
            spec.UserInvokedSearch = userInvokedSearch;
            spec.InteractiveSearch = interactiveSearch;

            return spec;
        }

        private List<DownloadDecision> Dispatch(Func<IIndexer, IEnumerable<ReleaseInfo>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            var reports = new List<ReleaseInfo>();

            _logger.ProgressInfo("Searching {0} indexers for {1}", indexers.Count, criteriaBase);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                taskList.Add(taskFactory.StartNew(() =>
                {
                    try
                    {
                        var indexerReports = searchAction(indexerLocal);

                        lock (reports)
                        {
                            reports.AddRange(indexerReports);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error while searching for {0}", criteriaBase);
                    }
                }).LogExceptions());
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }
    }
}
