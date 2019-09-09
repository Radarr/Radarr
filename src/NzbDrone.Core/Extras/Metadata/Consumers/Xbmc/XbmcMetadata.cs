using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly Logger _logger;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDetectXbmcNfo _detectNfo;

        public XbmcMetadata(IDetectXbmcNfo detectNfo,
                            IMapCoversToLocal mediaCoverService,
                            Logger logger)
        {
            _logger = logger;
            _mediaCoverService = mediaCoverService;
            _detectNfo = detectNfo;
        }

        private static readonly Regex ArtistImagesRegex = new Regex(@"^(?<type>folder|banner|fanart|logo)\.(?:png|jpg|jpeg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex AlbumImagesRegex = new Regex(@"^(?<type>cover|disc)\.(?:png|jpg|jpeg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Kodi (XBMC) / Emby";

        public override string GetFilenameAfterMove(Artist artist, TrackFile trackFile, MetadataFile metadataFile)
        {
            var trackFilePath = trackFile.Path;

            if (metadataFile.Type == MetadataType.TrackMetadata)
            {
                return GetTrackMetadataFilename(trackFilePath);
            }

            _logger.Debug("Unknown track file metadata: {0}", metadataFile.RelativePath);
            return Path.Combine(artist.Path, metadataFile.RelativePath);
        }

        public override MetadataFile FindMetadataFile(Artist artist, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null) return null;

            var metadata = new MetadataFile
            {
                ArtistId = artist.Id,
                Consumer = GetType().Name,
                RelativePath = artist.Path.GetRelativePath(path)
            };

            if (ArtistImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.ArtistImage;
                return metadata;
            }

            var albumMatch = AlbumImagesRegex.Match(filename);

            if (albumMatch.Success)
            {
                metadata.Type = MetadataType.AlbumImage;
                return metadata;
            }

            var isXbmcNfoFile = _detectNfo.IsXbmcNfoFile(path);

            if (filename.Equals("artist.nfo", StringComparison.OrdinalIgnoreCase) &&
                isXbmcNfoFile)
            {
                metadata.Type = MetadataType.ArtistMetadata;
                return metadata;
            }

            if (filename.Equals("album.nfo", StringComparison.OrdinalIgnoreCase) &&
                isXbmcNfoFile)
            {
                metadata.Type = MetadataType.AlbumMetadata;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult ArtistMetadata(Artist artist)
        {
            if (!Settings.ArtistMetadata)
            {
                return null;
            }

            _logger.Debug("Generating artist.nfo for: {0}", artist.Name);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var artistElement = new XElement("artist");

                artistElement.Add(new XElement("title", artist.Name));

                if (artist.Metadata.Value.Ratings != null && artist.Metadata.Value.Ratings.Votes > 0)
                {
                    artistElement.Add(new XElement("rating", artist.Metadata.Value.Ratings.Value));
                }

                artistElement.Add(new XElement("musicbrainzartistid", artist.Metadata.Value.ForeignArtistId));
                artistElement.Add(new XElement("biography", artist.Metadata.Value.Overview));
                artistElement.Add(new XElement("outline", artist.Metadata.Value.Overview));

                var doc = new XDocument(artistElement);
                doc.Save(xw);

                _logger.Debug("Saving artist.nfo for {0}", artist.Metadata.Value.Name);

                return new MetadataFileResult("artist.nfo", doc.ToString());
            }
        }

        public override MetadataFileResult AlbumMetadata(Artist artist, Album album, string albumPath)
        {
            if (!Settings.AlbumMetadata)
            {
                return null;
            }

            _logger.Debug("Generating album.nfo for: {0}", album.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var albumElement = new XElement("album");

                albumElement.Add(new XElement("title", album.Title));

                if (album.Ratings != null && album.Ratings.Votes > 0)
                {
                    albumElement.Add(new XElement("rating", album.Ratings.Value));
                }

                albumElement.Add(new XElement("musicbrainzalbumid", album.ForeignAlbumId));
                albumElement.Add(new XElement("artistdesc", artist.Metadata.Value.Overview));
                albumElement.Add(new XElement("releasedate", album.ReleaseDate.Value.ToShortDateString()));

                var doc = new XDocument(albumElement);
                doc.Save(xw);

                _logger.Debug("Saving album.nfo for {0}", album.Title);
                
                var fileName = Path.Combine(albumPath, "album.nfo");

                return new MetadataFileResult(fileName, doc.ToString());
            }
        }

        public override MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile)
        {
            return null;
        }

        public override List<ImageFileResult> ArtistImages(Artist artist)
        {
            if (!Settings.ArtistImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessArtistImages(artist).ToList();
        }

        public override List<ImageFileResult> AlbumImages(Artist artist, Album album, string albumPath)
        {
            if (!Settings.AlbumImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessAlbumImages(artist, album, albumPath).ToList();
        }

        public override List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile)
        {

            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessArtistImages(Artist artist)
        {
            foreach (var image in artist.Metadata.Value.Images)
            {
                var source = _mediaCoverService.GetCoverPath(artist.Id, MediaCoverEntity.Artist, image.CoverType, image.Extension);
                var destination = image.CoverType.ToString().ToLowerInvariant() + image.Extension;
                if (image.CoverType == MediaCoverTypes.Poster)
                {
                    destination = "folder" + image.Extension;
                }

                yield return new ImageFileResult(destination, source);
            }
        }

        private IEnumerable<ImageFileResult> ProcessAlbumImages(Artist artist, Album album, string albumPath)
        {
            foreach (var image in album.Images)
            {
                // TODO: Make Source fallback to URL if local does not exist
                // var source = _mediaCoverService.GetCoverPath(album.ArtistId, image.CoverType, null, album.Id);
                string filename;

                switch(image.CoverType)
                {
                    case MediaCoverTypes.Cover:
                        filename = "folder";
                        break;
                    case MediaCoverTypes.Disc:
                        filename = "discart";
                        break;
                    default:
                        continue;
                }

                var destination = Path.Combine(albumPath, filename + image.Extension);

                yield return new ImageFileResult(destination, image.Url);
            }
        }

        private string GetTrackMetadataFilename(string trackFilePath)
        {
            return Path.ChangeExtension(trackFilePath, "nfo");
        }

    }
}
