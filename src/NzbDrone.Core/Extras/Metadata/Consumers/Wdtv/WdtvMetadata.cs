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

namespace NzbDrone.Core.Extras.Metadata.Consumers.Wdtv
{
    public class WdtvMetadata : MetadataBase<WdtvMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public WdtvMetadata(IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public override string Name => "WDTV";

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

            var parseResult = Parser.Parser.ParseMusicTitle(filename);

            if (parseResult != null)
            {
                switch (Path.GetExtension(filename).ToLowerInvariant())
                {
                    case ".xml":
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

                    var details = new XElement("details");
                    details.Add(new XElement("id", artist.Id));
                    details.Add(new XElement("title", string.Format("{0} - {1} - {2}", artist.Name, track.TrackNumber, track.Title)));
                    details.Add(new XElement("artist_name", artist.Metadata.Value.Name));
                    details.Add(new XElement("track_name", track.Title));
                    details.Add(new XElement("track_number", track.AbsoluteTrackNumber.ToString("00")));
                    details.Add(new XElement("member", string.Join(" / ", artist.Metadata.Value.Members.ConvertAll(c => c.Name + " - " + c.Instrument))));

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            var filename = GetTrackMetadataFilename(artist.Path.GetRelativePath(trackFile.Path));

            return new MetadataFileResult(filename, xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> ArtistImages(Artist artist)
        {
            return new List<ImageFileResult>();
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
