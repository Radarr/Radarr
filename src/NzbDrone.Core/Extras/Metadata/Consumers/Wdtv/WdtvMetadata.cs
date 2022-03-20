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

            var metadata = new MetadataFile
            {
                MovieId = movie.Id,
                Consumer = GetType().Name,
                RelativePath = movie.Path.GetRelativePath(path)
            };

            if (Path.GetFileName(filename).Equals("folder.jpg", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.MovieImage;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseMovieTitle(filename);

            if (parseResult != null)
            {
                switch (Path.GetExtension(filename).ToLowerInvariant())
                {
                    case ".xml":
                        metadata.Type = MetadataType.MovieMetadata;
                        return metadata;
                    case ".metathumb":
                        metadata.Type = MetadataType.MovieImage;
                        return metadata;
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

            _logger.Debug("Generating Movie File Metadata for: {0}", Path.Combine(movie.Path, movieFile.RelativePath));

            var xmlResult = string.Empty;

            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var doc = new XDocument();

                var details = new XElement("details");
                details.Add(new XElement("id", movie.Id));
                details.Add(new XElement("title", movie.Title));
                details.Add(new XElement("genre", string.Join(" / ", movie.MovieMetadata.Value.Genres)));
                details.Add(new XElement("overview", movie.MovieMetadata.Value.Overview));

                doc.Add(details);
                doc.Save(xw);

                xmlResult += doc.ToString();
                xmlResult += Environment.NewLine;
            }

            var filename = GetMovieFileMetadataFilename(movieFile.RelativePath);

            return new MetadataFileResult(filename, xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> MovieImages(Movie movie)
        {
            if (!Settings.MovieImages)
            {
                return new List<ImageFileResult>();
            }

            //Because we only support one image, attempt to get the Poster type, then if that fails grab the first
            var image = movie.MovieMetadata.Value.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? movie.MovieMetadata.Value.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Movie image for movie {0}.", movie.Title);
                return new List<ImageFileResult>();
            }

            var source = _mediaCoverService.GetCoverPath(movie.Id, image.CoverType);
            var destination = "folder" + Path.GetExtension(source);

            return new List<ImageFileResult>
                   {
                       new ImageFileResult(destination, source)
                   };
        }

        private string GetMovieFileMetadataFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "xml");
        }

        private string GetMovieFileImageFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "metathumb");
        }
    }
}
