using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Identification
{
    public interface ICandidateService
    {
        List<CandidateAlbumRelease> GetDbCandidatesFromTags(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting);
        List<CandidateAlbumRelease> GetRemoteCandidates(LocalAlbumRelease localAlbumRelease);
    }

    public class CandidateService : ICandidateService
    {
        private readonly ISearchForNewBook _bookSearchService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public CandidateService(ISearchForNewBook bookSearchService,
                                IAuthorService authorService,
                                IBookService bookService,
                                IMediaFileService mediaFileService,
                                Logger logger)
        {
            _bookSearchService = bookSearchService;
            _authorService = authorService;
            _bookService = bookService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public List<CandidateAlbumRelease> GetDbCandidatesFromTags(LocalAlbumRelease localAlbumRelease, IdentificationOverrides idOverrides, bool includeExisting)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Generally author, book and release are null.  But if they're not then limit candidates appropriately.
            // We've tried to make sure that tracks are all for a single release.
            List<CandidateAlbumRelease> candidateReleases;

            // if we have a Book ID, use that
            Book tagMbidRelease = null;
            List<CandidateAlbumRelease> tagCandidate = null;

            // TODO: select by ISBN?
            // var releaseIds = localAlbumRelease.LocalTracks.Select(x => x.FileTrackInfo.ReleaseMBId).Distinct().ToList();
            // if (releaseIds.Count == 1 && releaseIds[0].IsNotNullOrWhiteSpace())
            // {
            //     _logger.Debug("Selecting release from consensus ForeignReleaseId [{0}]", releaseIds[0]);
            //     tagMbidRelease = _releaseService.GetReleaseByForeignReleaseId(releaseIds[0], true);

            //     if (tagMbidRelease != null)
            //     {
            //         tagCandidate = GetDbCandidatesByRelease(new List<AlbumRelease> { tagMbidRelease }, includeExisting);
            //     }
            // }
            if (idOverrides?.Book != null)
            {
                // use the release from file tags if it exists and agrees with the specified book
                if (tagMbidRelease?.Id == idOverrides.Book.Id)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetDbCandidatesByAlbum(idOverrides.Book, includeExisting);
                }
            }
            else if (idOverrides?.Author != null)
            {
                // use the release from file tags if it exists and agrees with the specified book
                if (tagMbidRelease?.AuthorMetadataId == idOverrides.Author.AuthorMetadataId)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetDbCandidatesByArtist(localAlbumRelease, idOverrides.Author, includeExisting);
                }
            }
            else
            {
                if (tagMbidRelease != null)
                {
                    candidateReleases = tagCandidate;
                }
                else
                {
                    candidateReleases = GetDbCandidates(localAlbumRelease, includeExisting);
                }
            }

            watch.Stop();
            _logger.Debug($"Getting {candidateReleases.Count} candidates from tags for {localAlbumRelease.LocalBooks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetDbCandidatesByAlbum(Book book, bool includeExisting)
        {
            return new List<CandidateAlbumRelease>
            {
                new CandidateAlbumRelease
                {
                    Book = book,
                    ExistingTracks = includeExisting ? _mediaFileService.GetFilesByBook(book.Id) : new List<BookFile>()
                }
            };
        }

        private List<CandidateAlbumRelease> GetDbCandidatesByArtist(LocalAlbumRelease localAlbumRelease, Author author, bool includeExisting)
        {
            _logger.Trace("Getting candidates for {0}", author);
            var candidateReleases = new List<CandidateAlbumRelease>();

            var albumTag = localAlbumRelease.LocalBooks.MostCommon(x => x.FileTrackInfo.AlbumTitle) ?? "";
            if (albumTag.IsNotNullOrWhiteSpace())
            {
                var possibleAlbums = _bookService.GetCandidates(author.AuthorMetadataId, albumTag);
                foreach (var book in possibleAlbums)
                {
                    candidateReleases.AddRange(GetDbCandidatesByAlbum(book, includeExisting));
                }
            }

            return candidateReleases;
        }

        private List<CandidateAlbumRelease> GetDbCandidates(LocalAlbumRelease localAlbumRelease, bool includeExisting)
        {
            // most general version, nothing has been specified.
            // get all plausible artists, then all plausible albums, then get releases for each of these.
            var candidateReleases = new List<CandidateAlbumRelease>();

            // check if it looks like VA.
            if (TrackGroupingService.IsVariousArtists(localAlbumRelease.LocalBooks))
            {
                var va = _authorService.FindById(DistanceCalculator.VariousAuthorIds[0]);
                if (va != null)
                {
                    candidateReleases.AddRange(GetDbCandidatesByArtist(localAlbumRelease, va, includeExisting));
                }
            }

            var artistTag = localAlbumRelease.LocalBooks.MostCommon(x => x.FileTrackInfo.ArtistTitle) ?? "";
            if (artistTag.IsNotNullOrWhiteSpace())
            {
                var possibleArtists = _authorService.GetCandidates(artistTag);
                foreach (var author in possibleArtists)
                {
                    candidateReleases.AddRange(GetDbCandidatesByArtist(localAlbumRelease, author, includeExisting));
                }
            }

            return candidateReleases;
        }

        public List<CandidateAlbumRelease> GetRemoteCandidates(LocalAlbumRelease localAlbumRelease)
        {
            // Gets candidate book releases from the metadata server.
            // Will eventually need adding locally if we find a match
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<Book> remoteBooks = null;
            var candidates = new List<CandidateAlbumRelease>();

            var goodreads = localAlbumRelease.LocalBooks.Select(x => x.FileTrackInfo.GoodreadsId).Distinct().ToList();
            var isbns = localAlbumRelease.LocalBooks.Select(x => x.FileTrackInfo.Isbn).Distinct().ToList();
            var asins = localAlbumRelease.LocalBooks.Select(x => x.FileTrackInfo.Asin).Distinct().ToList();

            try
            {
                if (goodreads.Count == 1 && goodreads[0].IsNotNullOrWhiteSpace())
                {
                    if (int.TryParse(goodreads[0], out var id))
                    {
                        _logger.Trace($"Searching by goodreads id {id}");

                        remoteBooks = _bookSearchService.SearchByGoodreadsId(id);
                    }
                }

                if ((remoteBooks == null || !remoteBooks.Any()) &&
                    isbns.Count == 1 &&
                    isbns[0].IsNotNullOrWhiteSpace())
                {
                    _logger.Trace($"Searching by isbn {isbns[0]}");

                    remoteBooks = _bookSearchService.SearchByIsbn(isbns[0]);
                }

                // Calibre puts junk asins into books it creates so check for sensible length
                if ((remoteBooks == null || !remoteBooks.Any()) &&
                    asins.Count == 1 &&
                    asins[0].IsNotNullOrWhiteSpace() &&
                    asins[0].Length == 10)
                {
                    _logger.Trace($"Searching by asin {asins[0]}");

                    remoteBooks = _bookSearchService.SearchByAsin(asins[0]);
                }

                // if no asin/isbn or no result, fall back to text search
                if (remoteBooks == null || !remoteBooks.Any())
                {
                    // fall back to author / book name search
                    string artistTag;

                    if (TrackGroupingService.IsVariousArtists(localAlbumRelease.LocalBooks))
                    {
                        artistTag = "Various Artists";
                    }
                    else
                    {
                        artistTag = localAlbumRelease.LocalBooks.MostCommon(x => x.FileTrackInfo.ArtistTitle) ?? "";
                    }

                    var albumTag = localAlbumRelease.LocalBooks.MostCommon(x => x.FileTrackInfo.AlbumTitle) ?? "";

                    if (artistTag.IsNullOrWhiteSpace() || albumTag.IsNullOrWhiteSpace())
                    {
                        return candidates;
                    }

                    remoteBooks = _bookSearchService.SearchForNewBook(albumTag, artistTag);

                    if (!remoteBooks.Any())
                    {
                        var albumSearch = _bookSearchService.SearchForNewBook(albumTag, null);
                        var artistSearch = _bookSearchService.SearchForNewBook(artistTag, null);

                        remoteBooks = albumSearch.Concat(artistSearch).DistinctBy(x => x.ForeignBookId).ToList();
                    }
                }
            }
            catch (SkyHookException e)
            {
                _logger.Info(e, "Skipping book due to SkyHook error");
                remoteBooks = new List<Book>();
            }

            foreach (var book in remoteBooks)
            {
                candidates.Add(new CandidateAlbumRelease
                {
                    Book = book,
                    ExistingTracks = new List<BookFile>()
                });
            }

            watch.Stop();
            _logger.Debug($"Getting {candidates.Count} remote candidates from tags for {localAlbumRelease.LocalBooks.Count} tracks took {watch.ElapsedMilliseconds}ms");

            return candidates;
        }
    }
}
