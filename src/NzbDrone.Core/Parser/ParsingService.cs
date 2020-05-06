using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Author GetArtist(string title);
        Author GetArtistFromTag(string file);
        RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, SearchCriteriaBase searchCriteria = null);
        RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, int authorId, IEnumerable<int> bookIds);
        List<Book> GetAlbums(ParsedAlbumInfo parsedAlbumInfo, Author artist, SearchCriteriaBase searchCriteria = null);

        ParsedAlbumInfo ParseAlbumTitleFuzzy(string title);

        // Music stuff here
        Book GetLocalAlbum(string filename, Author artist);
    }

    public class ParsingService : IParsingService
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public ParsingService(IArtistService artistService,
                              IAlbumService albumService,
                              IMediaFileService mediaFileService,
                              Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public Author GetArtist(string title)
        {
            var parsedAlbumInfo = Parser.ParseAlbumTitle(title);

            if (parsedAlbumInfo != null && !parsedAlbumInfo.ArtistName.IsNullOrWhiteSpace())
            {
                title = parsedAlbumInfo.ArtistName;
            }

            var artistInfo = _artistService.FindByName(title);

            if (artistInfo == null)
            {
                _logger.Debug("Trying inexact artist match for {0}", title);
                artistInfo = _artistService.FindByNameInexact(title);
            }

            return artistInfo;
        }

        public Author GetArtistFromTag(string file)
        {
            var parsedTrackInfo = Parser.ParseMusicPath(file);

            var artist = new Author();

            if (parsedTrackInfo.ArtistMBId.IsNotNullOrWhiteSpace())
            {
                artist = _artistService.FindById(parsedTrackInfo.ArtistMBId);

                if (artist != null)
                {
                    return artist;
                }
            }

            if (parsedTrackInfo == null || parsedTrackInfo.ArtistTitle.IsNullOrWhiteSpace())
            {
                return null;
            }

            artist = _artistService.FindByName(parsedTrackInfo.ArtistTitle);

            if (artist == null)
            {
                _logger.Debug("Trying inexact artist match for {0}", parsedTrackInfo.ArtistTitle);
                artist = _artistService.FindByNameInexact(parsedTrackInfo.ArtistTitle);
            }

            return artist;
        }

        public RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, SearchCriteriaBase searchCriteria = null)
        {
            var remoteAlbum = new RemoteAlbum
            {
                ParsedAlbumInfo = parsedAlbumInfo,
            };

            var artist = GetArtist(parsedAlbumInfo, searchCriteria);

            if (artist == null)
            {
                return remoteAlbum;
            }

            remoteAlbum.Artist = artist;
            remoteAlbum.Albums = GetAlbums(parsedAlbumInfo, artist, searchCriteria);

            return remoteAlbum;
        }

        public List<Book> GetAlbums(ParsedAlbumInfo parsedAlbumInfo, Author artist, SearchCriteriaBase searchCriteria = null)
        {
            var albumTitle = parsedAlbumInfo.AlbumTitle;
            var result = new List<Book>();

            if (parsedAlbumInfo.AlbumTitle == null)
            {
                return new List<Book>();
            }

            Book albumInfo = null;

            if (parsedAlbumInfo.Discography)
            {
                if (parsedAlbumInfo.DiscographyStart > 0)
                {
                    return _albumService.ArtistAlbumsBetweenDates(artist,
                        new DateTime(parsedAlbumInfo.DiscographyStart, 1, 1),
                        new DateTime(parsedAlbumInfo.DiscographyEnd, 12, 31),
                        false);
                }

                if (parsedAlbumInfo.DiscographyEnd > 0)
                {
                    return _albumService.ArtistAlbumsBetweenDates(artist,
                        new DateTime(1800, 1, 1),
                        new DateTime(parsedAlbumInfo.DiscographyEnd, 12, 31),
                        false);
                }

                return _albumService.GetAlbumsByArtist(artist.Id);
            }

            if (searchCriteria != null)
            {
                albumInfo = searchCriteria.Albums.ExclusiveOrDefault(e => e.Title == albumTitle);
            }

            if (albumInfo == null)
            {
                // TODO: Search by Title and Year instead of just Title when matching
                albumInfo = _albumService.FindByTitle(artist.AuthorMetadataId, parsedAlbumInfo.AlbumTitle);
            }

            if (albumInfo == null)
            {
                _logger.Debug("Trying inexact album match for {0}", parsedAlbumInfo.AlbumTitle);
                albumInfo = _albumService.FindByTitleInexact(artist.AuthorMetadataId, parsedAlbumInfo.AlbumTitle);
            }

            if (albumInfo != null)
            {
                result.Add(albumInfo);
            }
            else
            {
                _logger.Debug("Unable to find {0}", parsedAlbumInfo);
            }

            return result;
        }

        public RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, int authorId, IEnumerable<int> bookIds)
        {
            return new RemoteAlbum
            {
                ParsedAlbumInfo = parsedAlbumInfo,
                Artist = _artistService.GetArtist(authorId),
                Albums = _albumService.GetAlbums(bookIds)
            };
        }

        private Author GetArtist(ParsedAlbumInfo parsedAlbumInfo, SearchCriteriaBase searchCriteria)
        {
            Author artist = null;

            if (searchCriteria != null)
            {
                if (searchCriteria.Artist.CleanName == parsedAlbumInfo.ArtistName.CleanArtistName())
                {
                    return searchCriteria.Artist;
                }
            }

            artist = _artistService.FindByName(parsedAlbumInfo.ArtistName);

            if (artist == null)
            {
                _logger.Debug("Trying inexact artist match for {0}", parsedAlbumInfo.ArtistName);
                artist = _artistService.FindByNameInexact(parsedAlbumInfo.ArtistName);
            }

            if (artist == null)
            {
                _logger.Debug("No matching artist {0}", parsedAlbumInfo.ArtistName);
                return null;
            }

            return artist;
        }

        public ParsedAlbumInfo ParseAlbumTitleFuzzy(string title)
        {
            var bestScore = 0.0;

            Author bestAuthor = null;
            Book bestBook = null;

            var possibleAuthors = _artistService.GetReportCandidates(title);

            foreach (var author in possibleAuthors)
            {
                _logger.Trace($"Trying possible author {author}");

                var authorMatch = title.FuzzyMatch(author.Metadata.Value.Name, 0.5);
                var possibleBooks = _albumService.GetCandidates(author.AuthorMetadataId, title);

                foreach (var book in possibleBooks)
                {
                    var bookMatch = title.FuzzyMatch(book.Title, 0.5);
                    var score = (authorMatch.Item2 + bookMatch.Item2) / 2;

                    _logger.Trace($"Book {book} has score {score}");

                    if (score > bestScore)
                    {
                        bestAuthor = author;
                        bestBook = book;
                    }
                }
            }

            _logger.Trace($"Best match: {bestAuthor} {bestBook}");

            if (bestAuthor != null)
            {
                return Parser.ParseAlbumTitleWithSearchCriteria(title, bestAuthor, new List<Book> { bestBook });
            }

            return null;
        }

        public Book GetLocalAlbum(string filename, Author artist)
        {
            if (Path.HasExtension(filename))
            {
                filename = Path.GetDirectoryName(filename);
            }

            var tracksInAlbum = _mediaFileService.GetFilesByArtist(artist.Id)
                .FindAll(s => Path.GetDirectoryName(s.Path) == filename)
                .DistinctBy(s => s.BookId)
                .ToList();

            return tracksInAlbum.Count == 1 ? _albumService.GetAlbum(tracksInAlbum.First().BookId) : null;
        }
    }
}
