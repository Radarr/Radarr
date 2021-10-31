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
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Tags;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;
        private readonly IDetectXbmcNfo _detectNfo;
        private readonly IDiskProvider _diskProvider;
        private readonly ICreditService _creditService;
        private readonly ITagService _tagService;
        private readonly IMovieTranslationService _movieTranslationsService;

        public XbmcMetadata(IDetectXbmcNfo detectNfo,
                            IDiskProvider diskProvider,
                            IMapCoversToLocal mediaCoverService,
                            ICreditService creditService,
                            ITagService tagService,
                            IMovieTranslationService movieTranslationsService,
                            Logger logger)
        {
            _logger = logger;
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _detectNfo = detectNfo;
            _creditService = creditService;
            _tagService = tagService;
            _movieTranslationsService = movieTranslationsService;
        }

        private static readonly Regex MovieImagesRegex = new Regex(@"^(?<type>poster|banner|fanart|clearart|discart|keyart|landscape|logo|backdrop|clearlogo)\.(?:png|jpe?g)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex MovieFileImageRegex = new Regex(@"(?<type>-thumb|-poster|-banner|-fanart|-clearart|-discart|-keyart|-landscape|-logo|-backdrop|-clearlogo)\.(?:png|jpe?g)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Kodi (XBMC) / Emby";

        public override string GetFilenameAfterMove(Movie movie, MovieFile movieFile, MetadataFile metadataFile)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);
            var metadataPath = Path.Combine(movie.Path, metadataFile.RelativePath);

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

            if (MovieImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.MovieImage;
                return metadata;
            }

            if (MovieFileImageRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.MovieImage;
                return metadata;
            }

            if (filename.Equals("movie.nfo", StringComparison.OrdinalIgnoreCase) &&
                _detectNfo.IsXbmcNfoFile(path))
            {
                metadata.Type = MetadataType.MovieMetadata;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseMovieTitle(filename);

            if (parseResult != null &&
                Path.GetExtension(filename).Equals(".nfo", StringComparison.OrdinalIgnoreCase) &&
                _detectNfo.IsXbmcNfoFile(path))
            {
                metadata.Type = MetadataType.MovieMetadata;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult MovieMetadata(Movie movie, MovieFile movieFile)
        {
            var xmlResult = string.Empty;
            if (Settings.MovieMetadata)
            {
                _logger.Debug("Generating Movie Metadata for: {0}", Path.Combine(movie.Path, movieFile.RelativePath));

                var movieMetadataLanguage = (Settings.MovieMetadataLanguage == (int)Language.Original) ?
                    (int)movie.OriginalLanguage :
                    Settings.MovieMetadataLanguage;

                var movieTranslations = _movieTranslationsService.GetAllTranslationsForMovie(movie.Id);
                var selectedSettingsLanguage = Language.FindById(movieMetadataLanguage);
                var movieTranslation = movieTranslations.FirstOrDefault(mt => mt.Language == selectedSettingsLanguage);

                var credits = _creditService.GetAllCreditsForMovie(movie.Id);

                var watched = GetExistingWatchedStatus(movie, movieFile.RelativePath);

                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();
                    var thumbnail = movie.Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);
                    var posters = movie.Images.Where(i => i.CoverType == MediaCoverTypes.Poster);
                    var fanarts = movie.Images.Where(i => i.CoverType == MediaCoverTypes.Fanart);

                    var details = new XElement("movie");

                    details.Add(new XElement("title", movieTranslation?.Title ?? movie.Title));

                    details.Add(new XElement("originaltitle", movie.OriginalTitle));

                    details.Add(new XElement("sorttitle", movie.SortTitle));

                    if (movie.Ratings != null && movie.Ratings.Votes > 0)
                    {
                        var setRating = new XElement("ratings");
                        var setRatethemoviedb = new XElement("rating", new XAttribute("name", "themoviedb"), new XAttribute("max", "10"), new XAttribute("default", "true"));
                        setRatethemoviedb.Add(new XElement("value", movie.Ratings.Value));
                        setRatethemoviedb.Add(new XElement("votes", movie.Ratings.Votes));
                        setRating.Add(setRatethemoviedb);
                        details.Add(setRating);
                    }

                    if (movie.Ratings != null && movie.Ratings.Votes > 0)
                    {
                        details.Add(new XElement("rating", movie.Ratings.Value));
                    }

                    details.Add(new XElement("userrating"));

                    details.Add(new XElement("top250"));

                    details.Add(new XElement("outline"));

                    details.Add(new XElement("plot", movieTranslation?.Overview ?? movie.Overview));

                    details.Add(new XElement("tagline"));

                    details.Add(new XElement("runtime", movie.Runtime));

                    if (thumbnail != null)
                    {
                        details.Add(new XElement("thumb", thumbnail.Url));
                    }

                    foreach (var poster in posters)
                    {
                        if (poster != null && poster.Url != null)
                        {
                            details.Add(new XElement("thumb", new XAttribute("aspect", "poster"), new XAttribute("preview", poster.Url), poster.Url));
                        }
                    }

                    if (fanarts.Any())
                    {
                        var fanartElement = new XElement("fanart");
                        foreach (var fanart in fanarts)
                        {
                            if (fanart != null && fanart.Url != null)
                            {
                                fanartElement.Add(new XElement("thumb", new XAttribute("preview", fanart.Url), fanart.Url));
                            }
                        }

                        details.Add(fanartElement);
                    }

                    if (movie.Certification.IsNotNullOrWhiteSpace())
                    {
                        details.Add(new XElement("mpaa", movie.Certification));
                    }

                    details.Add(new XElement("playcount"));

                    details.Add(new XElement("lastplayed"));

                    details.Add(new XElement("id", movie.TmdbId));

                    var uniqueId = new XElement("uniqueid", movie.TmdbId);
                    uniqueId.SetAttributeValue("type", "tmdb");
                    uniqueId.SetAttributeValue("default", true);
                    details.Add(uniqueId);

                    if (movie.ImdbId.IsNotNullOrWhiteSpace())
                    {
                        var imdbId = new XElement("uniqueid", movie.ImdbId);
                        imdbId.SetAttributeValue("type", "imdb");
                        details.Add(imdbId);
                    }

                    foreach (var genre in movie.Genres)
                    {
                        details.Add(new XElement("genre", genre));
                    }

                    details.Add(new XElement("country"));

                    if (movie.Collection?.Name != null)
                    {
                        var setElement = new XElement("set");

                        setElement.Add(new XElement("name", movie.Collection.Name));
                        setElement.Add(new XElement("overview"));

                        details.Add(setElement);
                    }

                    var tags = _tagService.GetTags(movie.Tags);

                    foreach (var tag in tags)
                    {
                        details.Add(new XElement("tag", tag.Label));
                    }

                    foreach (var credit in credits)
                    {
                        if (credit.Name != null && credit.Job == "Screenplay")
                        {
                            details.Add(new XElement("credits", credit.Name));
                        }
                    }

                    foreach (var credit in credits)
                    {
                        if (credit.Name != null && credit.Job == "Director")
                        {
                            details.Add(new XElement("director", credit.Name));
                        }
                    }

                    if (movie.InCinemas.HasValue)
                    {
                        details.Add(new XElement("premiered", movie.InCinemas.Value.ToString("yyyy-MM-dd")));
                    }

                    details.Add(new XElement("year", movie.Year));

                    details.Add(new XElement("studio", movie.Studio));

                    details.Add(new XElement("trailer", "plugin://plugin.video.youtube/play/?video_id=" + movie.YouTubeTrailerId));

                    if (movieFile.MediaInfo != null)
                    {
                        var sceneName = movieFile.GetSceneOrFileName();

                        var fileInfo = new XElement("fileinfo");
                        var streamDetails = new XElement("streamdetails");

                        var video = new XElement("video");
                        video.Add(new XElement("aspect", (float)movieFile.MediaInfo.Width / (float)movieFile.MediaInfo.Height));
                        video.Add(new XElement("bitrate", movieFile.MediaInfo.VideoBitrate));
                        video.Add(new XElement("codec", MediaInfoFormatter.FormatVideoCodec(movieFile.MediaInfo, sceneName)));
                        video.Add(new XElement("framerate", movieFile.MediaInfo.VideoFps));
                        video.Add(new XElement("height", movieFile.MediaInfo.Height));
                        video.Add(new XElement("scantype", movieFile.MediaInfo.ScanType));
                        video.Add(new XElement("width", movieFile.MediaInfo.Width));

                        if (movieFile.MediaInfo.RunTime != default)
                        {
                            video.Add(new XElement("duration", movieFile.MediaInfo.RunTime.TotalMinutes));
                            video.Add(new XElement("durationinseconds", Math.Round(movieFile.MediaInfo.RunTime.TotalSeconds)));
                        }

                        streamDetails.Add(video);

                        var audio = new XElement("audio");
                        var audioChannelCount = movieFile.MediaInfo.AudioChannels;
                        audio.Add(new XElement("bitrate", movieFile.MediaInfo.AudioBitrate));
                        audio.Add(new XElement("channels", audioChannelCount));
                        audio.Add(new XElement("codec", MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, sceneName)));
                        audio.Add(new XElement("language", movieFile.MediaInfo.AudioLanguages));
                        streamDetails.Add(audio);

                        if (movieFile.MediaInfo.Subtitles != null && movieFile.MediaInfo.Subtitles.Count > 0)
                        {
                            var subtitle = new XElement("subtitle");
                            subtitle.Add(new XElement("language", movieFile.MediaInfo.Subtitles));
                            streamDetails.Add(subtitle);
                        }

                        fileInfo.Add(streamDetails);
                        details.Add(fileInfo);

                        foreach (var credit in credits)
                        {
                            if (credit.Name != null && credit.Character != null)
                            {
                                var actorElement = new XElement("actor");

                                actorElement.Add(new XElement("name", credit.Name));
                                actorElement.Add(new XElement("role", credit.Character));
                                actorElement.Add(new XElement("order", credit.Order));

                                var headshot = credit.Images.FirstOrDefault(m => m.CoverType == MediaCoverTypes.Headshot);

                                if (headshot != null && headshot.Url != null)
                                {
                                    actorElement.Add(new XElement("thumb", headshot.Url));
                                }

                                details.Add(actorElement);
                            }
                        }
                    }

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            if (Settings.MovieMetadataURL)
            {
                xmlResult += "https://www.themoviedb.org/movie/" + movie.TmdbId;
                xmlResult += Environment.NewLine;

                xmlResult += "https://www.imdb.com/title/" + movie.ImdbId;
                xmlResult += Environment.NewLine;
            }

            var metadataFileName = GetMovieMetadataFilename(movieFile.RelativePath);

            return string.IsNullOrEmpty(xmlResult) ? null : new MetadataFileResult(metadataFileName, xmlResult.Trim(Environment.NewLine.ToCharArray()));
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
            if (Settings.UseMovieNfo)
            {
                return Path.Combine(Path.GetDirectoryName(movieFilePath), "movie.nfo");
            }
            else
            {
                return Path.ChangeExtension(movieFilePath, "nfo");
            }
        }

        private bool GetExistingWatchedStatus(Movie movie, string movieFilePath)
        {
            var fullPath = Path.Combine(movie.Path, GetMovieMetadataFilename(movieFilePath));

            if (!_diskProvider.FileExists(fullPath))
            {
                return false;
            }

            var fileContent = _diskProvider.ReadAllText(fullPath);

            return Regex.IsMatch(fileContent, "<watched>true</watched>");
        }
    }
}
