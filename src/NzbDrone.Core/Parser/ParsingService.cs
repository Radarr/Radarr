using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using System;
using System.Drawing.Text;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Artist GetArtist(string title);
        Artist GetArtistFromTag(string file);
        RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, SearchCriteriaBase searchCriteria = null);
        RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, int artistId, IEnumerable<int> albumIds);
        List<Album> GetAlbums(ParsedAlbumInfo parsedAlbumInfo, Artist artist, SearchCriteriaBase searchCriteria = null);

        // Music stuff here
        Album GetLocalAlbum(string filename, Artist artist);
        LocalTrack GetLocalTrack(string filename, Artist artist);
        LocalTrack GetLocalTrack(string filename, Artist artist, ParsedTrackInfo folderInfo);

    }

    public class ParsingService : IParsingService
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ITrackService _trackService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public ParsingService(ITrackService trackService,
                              IArtistService artistService,
                              IAlbumService albumService,
                              IMediaFileService mediaFileService,
                              Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _trackService = trackService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public Artist GetArtist(string title)
        {
            var parsedAlbumInfo = Parser.ParseAlbumTitle(title);
            
            if (parsedAlbumInfo == null || parsedAlbumInfo.ArtistName.IsNullOrWhiteSpace())
            {
                return _artistService.FindByName(title);
            }

            return _artistService.FindByName(parsedAlbumInfo.ArtistName);
            
        }

        public Artist GetArtistFromTag(string file)
        {
            var parsedTrackInfo = Parser.ParseMusicPath(file);

            var artist = new Artist();

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

            return _artistService.FindByName(parsedTrackInfo.ArtistTitle);

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

        public List<Album> GetAlbums(ParsedAlbumInfo parsedAlbumInfo, Artist artist, SearchCriteriaBase searchCriteria = null)
        {
            var albumTitle = parsedAlbumInfo.AlbumTitle;
            var result = new List<Album>();

            if (parsedAlbumInfo.AlbumTitle == null)
            {
                return new List<Album>();
            }

            Album albumInfo = null;

            if (parsedAlbumInfo.Discography)
            {
                if (parsedAlbumInfo.DiscographyStart > 0)
                {
                    return _albumService.ArtistAlbumsBetweenDates(artist,
                        new DateTime(parsedAlbumInfo.DiscographyStart, 1, 1),
                        new DateTime(parsedAlbumInfo.DiscographyEnd, 12, 31), false);
                }

                if (parsedAlbumInfo.DiscographyEnd > 0)
                {
                    return _albumService.ArtistAlbumsBetweenDates(artist,
                        new DateTime(1800, 1, 1),
                        new DateTime(parsedAlbumInfo.DiscographyEnd, 12, 31), false);
                }

                return _albumService.GetAlbumsByArtist(artist.Id);
            }

            if (searchCriteria != null)
            {
                albumInfo = searchCriteria.Albums.SingleOrDefault(e => e.Title == albumTitle);
            }

            if (albumInfo == null)
            {
                // TODO: Search by Title and Year instead of just Title when matching
                albumInfo = _albumService.FindByTitle(artist.Id, parsedAlbumInfo.AlbumTitle);
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

        public RemoteAlbum Map(ParsedAlbumInfo parsedAlbumInfo, int artistId, IEnumerable<int> albumIds)
        {
            return new RemoteAlbum
            {
                ParsedAlbumInfo = parsedAlbumInfo,
                Artist = _artistService.GetArtist(artistId),
                Albums = _albumService.GetAlbums(albumIds)
            };
        }

        private Artist GetArtist(ParsedAlbumInfo parsedAlbumInfo, SearchCriteriaBase searchCriteria)
        {
            Artist artist = null;

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
                _logger.Debug("No matching artist {0}", parsedAlbumInfo.ArtistName);
                return null;
            }

            return artist;
        }

        public Album GetLocalAlbum(string filename, Artist artist)
        {
            
            if (Path.HasExtension(filename))
            {
                filename = Path.GetDirectoryName(filename);
            }

            filename = artist.Path.GetRelativePath(filename);

            var tracksInAlbum = _mediaFileService.GetFilesByArtist(artist.Id)
                .FindAll(s => Path.GetDirectoryName(s.RelativePath) == filename)
                .DistinctBy(s => s.AlbumId)
                .ToList();

            return tracksInAlbum.Count == 1 ? _albumService.GetAlbum(tracksInAlbum.First().AlbumId) : null;
        }

        public LocalTrack GetLocalTrack(string filename, Artist artist)
        {
            return GetLocalTrack(filename, artist, null);
        }

        public LocalTrack GetLocalTrack(string filename, Artist artist, ParsedTrackInfo folderInfo)
        {
            ParsedTrackInfo parsedTrackInfo;


            if (folderInfo != null)
            {
                parsedTrackInfo = folderInfo.JsonClone();
                parsedTrackInfo.Quality = QualityParser.ParseQuality(Path.GetFileName(filename), null, 0);
            } else
            {
                parsedTrackInfo = Parser.ParseMusicPath(filename);
            }

            if (parsedTrackInfo == null || (parsedTrackInfo.AlbumTitle.IsNullOrWhiteSpace()) && parsedTrackInfo.ReleaseMBId.IsNullOrWhiteSpace())
            {
                if (MediaFileExtensions.Extensions.Contains(Path.GetExtension(filename)))
                {
                    _logger.Warn("Unable to parse track info from path {0}", filename);
                }

                return null;
            }

            var album = GetAlbum(artist, parsedTrackInfo);
            var tracks = new List<Track>();
            if (album != null)
            {
                tracks = GetTracks(artist, album, parsedTrackInfo);
            }
            
            return new LocalTrack
            {
                Artist = artist,
                Album = album,
                Quality = parsedTrackInfo.Quality,
                Language = parsedTrackInfo.Language,
                Tracks = tracks,
                Path = filename,
                ParsedTrackInfo = parsedTrackInfo,
                ExistingFile = artist.Path.IsParentPath(filename)
            };
        }

        private Album GetAlbum(Artist artist, ParsedTrackInfo parsedTrackInfo)
        {
            Album album = null;

            if (parsedTrackInfo == null)
            {
                return null;
            }

            if (parsedTrackInfo.ReleaseMBId.IsNotNullOrWhiteSpace())
            {
                album = _albumService.FindAlbumByRelease(parsedTrackInfo.ReleaseMBId);
            }

            if (album == null && parsedTrackInfo.AlbumTitle.IsNullOrWhiteSpace())
            {
                _logger.Debug("Album title could not be parsed for {0}", parsedTrackInfo);
                return null;
            }

            var cleanAlbumTitle = Parser.CleanAlbumTitle(parsedTrackInfo.AlbumTitle);
            _logger.Debug("Cleaning Album title of common matching issues. Cleaned album title is '{0}'", cleanAlbumTitle);

            if (album == null)
            {
                album = _albumService.FindByTitle(artist.Id, cleanAlbumTitle);
            }

            if (album == null)
            {
                _logger.Debug("Parsed album title not found in Db for {0}", parsedTrackInfo);
                return null;
            }

            _logger.Debug("Album {0} selected for {1}", album, parsedTrackInfo);

            return album;
        }

        private List<Track> GetTracks(Artist artist, Album album, ParsedTrackInfo parsedTrackInfo)
        {
            var result = new List<Track>();

            if (parsedTrackInfo.Title.IsNotNullOrWhiteSpace())
            {
                Track trackInfo;
                var cleanTrackTitle = Parser.CleanTrackTitle(parsedTrackInfo.Title);
                _logger.Debug("Cleaning Track title of common matching issues. Cleaned track title is '{0}'", cleanTrackTitle);

                trackInfo = _trackService.FindTrackByTitle(artist.Id, album.Id, parsedTrackInfo.DiscNumber, parsedTrackInfo.TrackNumbers.FirstOrDefault(), cleanTrackTitle);

                if (trackInfo == null)
                {
                    trackInfo = _trackService.FindTrackByTitle(artist.Id, album.Id, parsedTrackInfo.DiscNumber, parsedTrackInfo.TrackNumbers.FirstOrDefault(), parsedTrackInfo.Title);
                }

                if (trackInfo != null)
                {
                    _logger.Debug("Track {0} selected for {1}", trackInfo, parsedTrackInfo);
                    result.Add(trackInfo);
                }
                else
                {
                    _logger.Debug("Unable to find track for {0}", parsedTrackInfo);
                }
            }

            return result;
        }
    }
}
