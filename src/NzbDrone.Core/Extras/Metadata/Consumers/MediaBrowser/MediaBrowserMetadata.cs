using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser
{
    public class MediaBrowserMetadata : MetadataBase<MediaBrowserMetadataSettings>
    {
        private readonly Logger _logger;

        public MediaBrowserMetadata(
                            Logger logger)
        {
            _logger = logger;
        }

        public override string Name => "Emby (Legacy)";

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

            if (filename.Equals("artist.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.ArtistMetadata;
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

            _logger.Debug("Generating artist.xml for: {0}", artist.Name);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var artistElement = new XElement("Artist");

                artistElement.Add(new XElement("id", artist.ForeignArtistId));
                artistElement.Add(new XElement("Status", artist.Status));

                artistElement.Add(new XElement("Added", artist.Added.ToString("MM/dd/yyyy HH:mm:ss tt"))); 
                artistElement.Add(new XElement("LockData", "false"));
                artistElement.Add(new XElement("Overview", artist.Overview));
                artistElement.Add(new XElement("LocalTitle", artist.Name));

                artistElement.Add(new XElement("Rating", artist.Ratings.Value));

                var persons   = new XElement("Persons");

                foreach (var person in artist.Members)
                {
                    persons.Add(new XElement("Person",
                        new XElement("Name", person.Name),
                        new XElement("Type", "Actor"),
                        new XElement("Role", person.Instrument)
                        ));
                }

                artistElement.Add(persons);


                var doc = new XDocument(artistElement);
                doc.Save(xw);

                _logger.Debug("Saving artist.xml for {0}", artist.Name);

                return new MetadataFileResult("artist.xml", doc.ToString());
            }
        }

        public override MetadataFileResult AlbumMetadata(Artist artist, Album album, string albumPath)
        {
            return null;
        }

        public override MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile)
        {
            return null;
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

        private IEnumerable<ImageFileResult> ProcessArtistImages(Artist artist)
        {
            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessAlbumImages(Artist artist, Album album)
        {
            return new List<ImageFileResult>();
        }

        private string GetEpisodeNfoFilename(string episodeFilePath)
        {
            return null;
        }

        private string GetEpisodeImageFilename(string episodeFilePath)
        {
            return null;
        }
    }
}
