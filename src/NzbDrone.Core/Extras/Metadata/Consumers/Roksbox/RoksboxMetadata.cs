using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

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

        //Re-enable when/if we store and use mpaa certification
        //private static List<string> ValidCertification = new List<string> { "G", "NC-17", "PG", "PG-13", "R", "UR", "UNRATED", "NR", "TV-Y", "TV-Y7", "TV-Y7-FV", "TV-G", "TV-PG", "TV-14", "TV-MA" };
        public override string Name => "Roksbox";

        public override string GetFilenameAfterMove(Movie movie, MovieFile movieFile, MetadataFile metadataFile)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

            if (metadataFile.Type == MetadataType.MovieImage)
            {
                return GetMovieFileImageFilename(movieFilePath);
            }

            if (metadataFile.Type == MetadataType.MovieMetadata)
            {
                return GetMovieFileMetadataFilename(movieFilePath);
            }

            _logger.Debug("Unknown movie file metadata: {0}", metadataFile.RelativePath);
            return Path.Combine(movie.Path, metadataFile.RelativePath);
        }

        public override MetadataFile FindMetadataFile(Movie movie, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var parentdir = Directory.GetParent(path);

            var metadata = new MetadataFile
            {
                MovieId = movie.Id,
                Consumer = GetType().Name,
                RelativePath = movie.Path.GetRelativePath(path)
            };

            var parseResult = Parser.Parser.ParseMovieTitle(filename);

            if (parseResult != null)
            {
                var extension = Path.GetExtension(filename).ToLowerInvariant();

                if (extension == ".xml")
                {
                    metadata.Type = MetadataType.MovieMetadata;
                    return metadata;
                }

                if (extension == ".jpg")
                {
                    if (Path.GetFileNameWithoutExtension(filename).Equals(parentdir.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        metadata.Type = MetadataType.MovieImage;
                        return metadata;
                    }
                }
            }

            return null;
        }

        public override MetadataFileResult MovieMetadata(Movie movie, MovieFile movieFile)
        {
            if (!Settings.MovieMetadata)
            {
                return null;
            }

            _logger.Debug("Generating Movie File Metadata for: {0}", movieFile.RelativePath);

            var xmlResult = string.Empty;

            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var doc = new XDocument();

                var details = new XElement("video");
                details.Add(new XElement("title", movie.Title));

                details.Add(new XElement("genre", string.Join(" / ", movie.MovieMetadata.Value.Genres)));
                details.Add(new XElement("description", movie.MovieMetadata.Value.Overview));
                details.Add(new XElement("length", movie.MovieMetadata.Value.Runtime));

                doc.Add(details);
                doc.Save(xw);

                xmlResult += doc.ToString();
                xmlResult += Environment.NewLine;
            }

            return new MetadataFileResult(GetMovieFileMetadataFilename(movieFile.RelativePath), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> MovieImages(Movie movie)
        {
            if (!Settings.MovieImages)
            {
                return new List<ImageFileResult>();
            }

            var image = movie.MovieMetadata.Value.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? movie.MovieMetadata.Value.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Movie image for movie {0}.", movie.Title);
                return new List<ImageFileResult>();
            }

            var source = _mediaCoverService.GetCoverPath(movie.Id, image.CoverType);
            var destination = Path.GetFileName(movie.Path) + Path.GetExtension(source);

            return new List<ImageFileResult> { new ImageFileResult(destination, source) };
        }

        private string GetMovieFileMetadataFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "xml");
        }

        private string GetMovieFileImageFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "jpg");
        }
    }
}
