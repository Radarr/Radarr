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
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;

        public XbmcMetadata(IMapCoversToLocal mediaCoverService,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _logger = logger;
        }

        private static readonly Regex MovieImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Kodi (XBMC) / Emby";

        public override string GetFilenameAfterMove(Movie movie, MovieFile movieFile, MetadataFile metadataFile)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);

            if (metadataFile.Type == MetadataType.MovieImage)
            {
                return GetMovieImageFilename(movieFilePath);
            }

            if (metadataFile.Type == MetadataType.MovieMetadata)
            {
                return GetMovieMetadataFilename(movieFilePath);
            }

            _logger.Debug("Unknown movie file metadata: {0}", metadataFile.RelativePath);
            return Path.Combine(movie.Path, metadataFile.RelativePath);
        }

        public override MetadataFile FindMetadataFile(Movie movie, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null) return null;

            var metadata = new MetadataFile
            {
                MovieId = movie.Id,
                Consumer = GetType().Name,
                RelativePath = movie.Path.GetRelativePath(path)
            };

            if (MovieImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.MovieImage;
                return metadata;
            }

            if (filename.Equals("movie.nfo", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.MovieMetadata;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseMovieTitle(filename);

            if (parseResult != null && Path.GetExtension(filename) == ".nfo")
            {
                metadata.Type = MetadataType.MovieMetadata;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult MovieMetadata(Movie movie)
        {
            if (!Settings.MovieMetadata)
            {
                return null;
            }

            _logger.Debug("Generating movie.nfo for: {0}", movie.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var movieNfo = new XElement("movie");

                movieNfo.Add(new XElement("title", movie.Title));

                if (movie.Ratings != null && movie.Ratings.Votes > 0)
                {
                    movieNfo.Add(new XElement("rating", movie.Ratings.Value));
                }

                movieNfo.Add(new XElement("plot", movie.Overview));
                //tvShow.Add(new XElement("episodeguide", new XElement("url", episodeGuideUrl)));
                //tvShow.Add(new XElement("episodeguideurl", episodeGuideUrl));
                movieNfo.Add(new XElement("mpaa", movie.Certification));
                movieNfo.Add(new XElement("id", movie.ImdbId));

                foreach (var genre in movie.Genres)
                {
                    movieNfo.Add(new XElement("genre", genre));
                }

                //if (series.FirstAired.HasValue)
                //{
                //    movieNfo.Add(new XElement("premiered", series.FirstAired.Value.ToString("yyyy-MM-dd")));
                //}

                movieNfo.Add(new XElement("sorttitle", movie.SortTitle));

                if (movie.Studio != null)
                {
                    movieNfo.Add(new XElement("studio", movie.Studio));
                }
                

                movieNfo.Add(new XElement("runtime", movie.Runtime));

                if (movie.YouTubeTrailerId != null)
                {
                    movieNfo.Add(new XElement("trailer", movie.YouTubeTrailerId));
                }

                foreach (var actor in movie.Actors)
                {
                    var xmlActor = new XElement("actor",
                        new XElement("name", actor.Name),
                        new XElement("role", actor.Character));

                    if (actor.Images.Any())
                    {
                        xmlActor.Add(new XElement("thumb", actor.Images.First().Url));
                    }

                    movieNfo.Add(xmlActor);
                }

                var doc = new XDocument(movieNfo);
                doc.Save(xw);

                _logger.Debug("Saving movie.nfo for {0}", movie.Title);

                return new MetadataFileResult("movie.nfo", doc.ToString());
            }
        }

        public override List<ImageFileResult> MovieImages(Movie movie)
        {
            if (!Settings.MovieImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessMovieImages(movie).ToList();
        }

        private IEnumerable<ImageFileResult> ProcessMovieImages(Movie movie)
        {
            foreach (var image in movie.Images)
            {
                var source = _mediaCoverService.GetCoverPath(movie.Id, image.CoverType);
                var destination = image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source);

                yield return new ImageFileResult(destination, source);
            }
        }

        private string GetMovieMetadataFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "nfo");
        }

        private string GetMovieImageFilename(string movieFilePath)
        {
            return Path.ChangeExtension(movieFilePath, "").Trim('.') + "-thumb.jpg";
        }

        private string GetAudioCodec(string audioCodec)
        {
            if (audioCodec == "AC-3")
            {
                return "AC3";
            }

            return audioCodec;
        }

        // Series Deprecation Below

        public override string GetFilenameAfterMove(Series series, EpisodeFile episodeFile, MetadataFile metadataFile)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override MetadataFileResult SeriesMetadata(Series series)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override List<ImageFileResult> SeriesImages(Series series)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override List<ImageFileResult> SeasonImages(Series series, Season season)
        {
            throw new Exception("Metadata not enabled for movies");
        }

        public override List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile)
        {
            throw new Exception("Metadata not enabled for movies");
        }


    }
}
