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
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;

        public XbmcMetadata(IMapCoversToLocal mediaCoverService,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _logger = logger;
        }

        private static readonly Regex ArtistImagesRegex = new Regex(@"^(?<type>poster|banner|fanart|logo)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex AlbumImagesRegex = new Regex(@"^season(?<season>\d{2,}|-all|-specials)-(?<type>poster|banner|fanart|cover)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EpisodeImageRegex = new Regex(@"-thumb\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Kodi (XBMC) / Emby";

        public override string GetFilenameAfterMove(Artist artist, TrackFile trackFile, MetadataFile metadataFile)
        {
            var trackFilePath = Path.Combine(artist.Path, trackFile.RelativePath);

            if (metadataFile.Type == MetadataType.TrackImage)
            {
                return GetEpisodeImageFilename(trackFilePath);
            }

            if (metadataFile.Type == MetadataType.TrackMetadata)
            {
                return GetEpisodeMetadataFilename(trackFilePath);
            }

            _logger.Debug("Unknown episode file metadata: {0}", metadataFile.RelativePath);
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

            var seasonMatch = AlbumImagesRegex.Match(filename);

            if (seasonMatch.Success)
            {
                metadata.Type = MetadataType.AlbumImage;

                var seasonNumberMatch = seasonMatch.Groups["season"].Value;
                int seasonNumber;

                if (seasonNumberMatch.Contains("specials"))
                {
                    metadata.AlbumId = 0;
                }

                else if (int.TryParse(seasonNumberMatch, out seasonNumber))
                {
                    metadata.AlbumId = seasonNumber;
                }

                else
                {
                    return null;
                }

                return metadata;
            }

            if (EpisodeImageRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.TrackImage;
                return metadata;
            }

            if (filename.Equals("artist.nfo", StringComparison.OrdinalIgnoreCase))
            {
                metadata.Type = MetadataType.ArtistMetadata;
                return metadata;
            }

            if (filename.Equals("album.nfo", StringComparison.OrdinalIgnoreCase))
            {
                metadata.Type = MetadataType.AlbumMetadata;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseMusicTitle(filename);

            if (parseResult != null &&
                Path.GetExtension(filename).Equals(".nfo", StringComparison.OrdinalIgnoreCase))
            {
                metadata.Type = MetadataType.TrackMetadata;
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

                if (artist.Ratings != null && artist.Ratings.Votes > 0)
                {
                    artistElement.Add(new XElement("rating", artist.Ratings.Value));
                }

                artistElement.Add(new XElement("musicbrainzartistid", artist.ForeignArtistId));
                artistElement.Add(new XElement("biography", artist.Overview));
                artistElement.Add(new XElement("outline", artist.Overview));
                //tvShow.Add(new XElement("episodeguide", new XElement("url", episodeGuideUrl)));
                //tvShow.Add(new XElement("episodeguideurl", episodeGuideUrl));

                //foreach (var genre in artist.Genres)
                //{
                //    tvShow.Add(new XElement("genre", genre));
                //}
                

                //foreach (var actor in artist.Members)
                //{
                //    var xmlActor = new XElement("actor",
                //        new XElement("name", actor.Name),
                //        new XElement("role", actor.Instrument));

                //    if (actor.Images.Any())
                //    {
                //        xmlActor.Add(new XElement("thumb", actor.Images.First().Url));
                //    }

                //    tvShow.Add(xmlActor);
                //}

                var doc = new XDocument(artistElement);
                doc.Save(xw);

                _logger.Debug("Saving artist.nfo for {0}", artist.Name);

                return new MetadataFileResult("artist.nfo", doc.ToString());
            }
        }

        public override MetadataFileResult AlbumMetadata(Artist artist, Album album)
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
                albumElement.Add(new XElement("artistdesc", artist.Overview));
                albumElement.Add(new XElement("releasedate", album.ReleaseDate.Value.ToShortDateString()));

                var doc = new XDocument(albumElement);
                doc.Save(xw);

                _logger.Debug("Saving album.nfo for {0}", artist.Name);

                return new MetadataFileResult("album.nfo", doc.ToString());
            }
        }

        public override MetadataFileResult TrackMetadata(Artist artist, TrackFile trackFile)
        {
            if (!Settings.TrackMetadata)
            {
                return null;
            }

            _logger.Debug("Generating Track Metadata for: {0}", Path.Combine(artist.Path, trackFile.RelativePath));

            var xmlResult = string.Empty;
            foreach (var episode in trackFile.Tracks.Value)
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();

                    var details = new XElement("episodedetails");
                    details.Add(new XElement("title", episode.Title));
                    details.Add(new XElement("episode", episode.TrackNumber));

                    //If trakt ever gets airs before information for specials we should add set it
                    details.Add(new XElement("displayseason"));
                    details.Add(new XElement("displayepisode"));

                    details.Add(new XElement("watched", "false"));

                    if (episode.Ratings != null && episode.Ratings.Votes > 0)
                    {
                        details.Add(new XElement("rating", episode.Ratings.Value));
                    }

                    if (trackFile.MediaInfo != null)
                    {
                        var fileInfo = new XElement("fileinfo");
                        var streamDetails = new XElement("streamdetails");

                        var video = new XElement("video");
                        video.Add(new XElement("aspect", (float)trackFile.MediaInfo.Width / (float)trackFile.MediaInfo.Height));
                        video.Add(new XElement("bitrate", trackFile.MediaInfo.VideoBitrate));
                        video.Add(new XElement("codec", trackFile.MediaInfo.VideoCodec));
                        video.Add(new XElement("framerate", trackFile.MediaInfo.VideoFps));
                        video.Add(new XElement("height", trackFile.MediaInfo.Height));
                        video.Add(new XElement("scantype", trackFile.MediaInfo.ScanType));
                        video.Add(new XElement("width", trackFile.MediaInfo.Width));

                        if (trackFile.MediaInfo.RunTime != null)
                        {
                            video.Add(new XElement("duration", trackFile.MediaInfo.RunTime.TotalMinutes));
                            video.Add(new XElement("durationinseconds", trackFile.MediaInfo.RunTime.TotalSeconds));
                        }

                        streamDetails.Add(video);

                        var audio = new XElement("audio");
                        audio.Add(new XElement("bitrate", trackFile.MediaInfo.AudioBitrate));
                        audio.Add(new XElement("channels", trackFile.MediaInfo.AudioChannels));
                        audio.Add(new XElement("codec", GetAudioCodec(trackFile.MediaInfo.AudioFormat)));
                        audio.Add(new XElement("language", trackFile.MediaInfo.AudioLanguages));
                        streamDetails.Add(audio);

                        if (trackFile.MediaInfo.Subtitles != null && trackFile.MediaInfo.Subtitles.Length > 0)
                        {
                            var subtitle = new XElement("subtitle");
                            subtitle.Add(new XElement("language", trackFile.MediaInfo.Subtitles));
                            streamDetails.Add(subtitle);
                        }

                        fileInfo.Add(streamDetails);
                        details.Add(fileInfo);
                    }

                    //Todo: get guest stars, writer and director
                    //details.Add(new XElement("credits", tvdbEpisode.Writer.FirstOrDefault()));
                    //details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            return new MetadataFileResult(GetEpisodeMetadataFilename(trackFile.RelativePath), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> ArtistImages(Artist artist)
        {
            if (!Settings.ArtistImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessArtistImages(artist).ToList();
        }

        public override List<ImageFileResult> AlbumImages(Artist artist, Album album)
        {
            if (!Settings.AlbumImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessAlbumImages(album).ToList();
        }

        public override List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile)
        {

            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessArtistImages(Artist artist)
        {
            foreach (var image in artist.Images)
            {
                var source = _mediaCoverService.GetCoverPath(artist.Id, image.CoverType);
                var destination = image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source);

                yield return new ImageFileResult(destination, source);
            }
        }

        private IEnumerable<ImageFileResult> ProcessAlbumImages(Album album)
        {
            foreach (var image in album.Images)
            {
                var destination = image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(image.Url);

                yield return new ImageFileResult(destination, image.Url);
            }
        }

        private string GetEpisodeMetadataFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "nfo");
        }

        private string GetEpisodeImageFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "").Trim('.') + "-thumb.jpg";
        }

        private string GetAudioCodec(string audioCodec)
        {
            if (audioCodec == "AC-3")
            {
                return "AC3";
            }

            return audioCodec;
        }
    }
}
