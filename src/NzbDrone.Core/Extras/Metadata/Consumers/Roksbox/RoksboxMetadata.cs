using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxMetadata : MetadataBase<RoksboxMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RoksboxMetadata(IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex SeasonImagesRegex = new Regex(@"^(season (?<season>\d+))|(?<specials>specials)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Roksbox";

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
            var parentdir = Directory.GetParent(path);

            var metadata = new MetadataFile
                           {
                               ArtistId = artist.Id,
                               Consumer = GetType().Name,
                               RelativePath = artist.Path.GetRelativePath(path)
                           };

            //Series and season images are both named folder.jpg, only season ones sit in season folders
            if (Path.GetFileNameWithoutExtension(filename).Equals(parentdir.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var seasonMatch = SeasonImagesRegex.Match(parentdir.Name);

                if (seasonMatch.Success)
                {
                    metadata.Type = MetadataType.AlbumImage;

                    if (seasonMatch.Groups["specials"].Success)
                    {
                        metadata.AlbumId = 0;
                    }

                    else
                    {
                        metadata.AlbumId = Convert.ToInt32(seasonMatch.Groups["season"].Value);
                    }

                    return metadata;
                }

                metadata.Type = MetadataType.ArtistImage;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseMusicTitle(filename);

            if (parseResult != null)
            {
                var extension = Path.GetExtension(filename).ToLowerInvariant();

                if (extension == ".xml")
                {
                    metadata.Type = MetadataType.TrackMetadata;
                    return metadata;
                }             
            }

            return null;
        }

        public override MetadataFileResult ArtistMetadata(Artist artist)
        {
            //Artist metadata is not supported
            return null;
        }

        public override MetadataFileResult AlbumMetadata(Artist artist, Album album, string albumPath)
        {
            return null;
        }

        public override MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile)
        {
            if (!Settings.TrackMetadata)
            {
                return null;
            }
            
            _logger.Debug("Generating Track Metadata for: {0}", trackFile.Path);

            var xmlResult = string.Empty;
            foreach (var track in trackFile.Tracks.Value)
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();

                    var details = new XElement("song");
                    details.Add(new XElement("title", track.Title));
                    details.Add(new XElement("performingartist", artist.Name));

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            return new MetadataFileResult(GetTrackMetadataFilename(artist.Path.GetRelativePath(trackFile.Path)), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> ArtistImages(Artist artist)
        {
            if (!Settings.ArtistImages)
            {
                return new List<ImageFileResult>();
            }

            var image = artist.Metadata.Value.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? artist.Metadata.Value.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Artist image for artist {0}.", artist.Name);
                return new List<ImageFileResult>(); ;
            }

            var source = _mediaCoverService.GetCoverPath(artist.Id, MediaCoverEntity.Artist, image.CoverType, image.Extension);
            var destination = Path.GetFileName(artist.Path) + Path.GetExtension(source);

            return new List<ImageFileResult>{ new ImageFileResult(destination, source) };
        }

        public override List<ImageFileResult> AlbumImages(Artist artist, Album album, string albumFolder)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile)
        {
            return new List<ImageFileResult>();
        }

        private string GetTrackMetadataFilename(string trackFilePath)
        {
            return Path.ChangeExtension(trackFilePath, "xml");
        }
    }
}
