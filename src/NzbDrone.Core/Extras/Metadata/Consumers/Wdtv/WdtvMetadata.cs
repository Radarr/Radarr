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

        private static readonly Regex SeasonImagesRegex = new Regex(@"^(season (?<season>\d+))|(?<specials>specials)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "WDTV";

        public override string GetFilenameAfterMove(Artist artist, TrackFile trackFile, MetadataFile metadataFile)
        {
            var trackFilePath = Path.Combine(artist.Path, trackFile.RelativePath);

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

            //Series and season images are both named folder.jpg, only season ones sit in season folders
            if (Path.GetFileName(filename).Equals("folder.jpg", StringComparison.InvariantCultureIgnoreCase))
            {
                var parentdir = Directory.GetParent(path);
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

            _logger.Debug("Generating Track Metadata for: {0}", Path.Combine(artist.Path, trackFile.RelativePath));

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
                    details.Add(new XElement("artist_name", artist.Name));
                    details.Add(new XElement("track_name", track.Title));
                    details.Add(new XElement("track_number", track.AbsoluteTrackNumber.ToString("00")));
                    details.Add(new XElement("member", string.Join(" / ", artist.Members.ConvertAll(c => c.Name + " - " + c.Instrument))));


                    //Todo: get guest stars, writer and director
                    //details.Add(new XElement("credits", tvdbEpisode.Writer.FirstOrDefault()));
                    //details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            var filename = GetTrackMetadataFilename(trackFile.RelativePath);

            return new MetadataFileResult(filename, xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> ArtistImages(Artist artist)
        {
            if (!Settings.ArtistImages)
            {
                return new List<ImageFileResult>();
            }

            //Because we only support one image, attempt to get the Poster type, then if that fails grab the first
            var image = artist.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? artist.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Artist image for artist {0}.", artist.Name);
                return new List<ImageFileResult>();
            }

            var source = _mediaCoverService.GetCoverPath(artist.Id, image.CoverType);
            var destination = "folder" + Path.GetExtension(source);

            return new List<ImageFileResult>
                   {
                       new ImageFileResult(destination, source)
                   };
        }

        public override List<ImageFileResult> AlbumImages(Artist artist, Album album, string albumFolder)
        {
            if (!Settings.AlbumImages)
            {
                return new List<ImageFileResult>();
            }
            
            var seasonFolders = GetAlbumFolders(artist);

            //Work out the path to this season - if we don't have a matching path then skip this season.
            string seasonFolder;
            if (!seasonFolders.TryGetValue(album.Id, out seasonFolder))
            {
                _logger.Trace("Failed to find album folder for artist {0}, album {1}.", artist.Name, album.Title);
                return new List<ImageFileResult>();
            }

            //WDTV only supports one season image, so first of all try for poster otherwise just use whatever is first in the collection
            var image = album.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? album.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable album image for artist {0}, album {1}.", artist.Name, album.Title);
                return new List<ImageFileResult>();
            }

            var path = Path.Combine(seasonFolder, "folder.jpg");

            return new List<ImageFileResult>{ new ImageFileResult(path, image.Url) };
        }

        public override List<ImageFileResult> TrackImages(Artist artist, TrackFile trackFile)
        {

            return new List<ImageFileResult>();
        }

        private string GetTrackMetadataFilename(string trackFilePath)
        {
            return Path.ChangeExtension(trackFilePath, "xml");
        }

        private string GetTrackImageFilename(string trackFilePath)
        {
            return Path.ChangeExtension(trackFilePath, "metathumb");
        }

        private Dictionary<int, string> GetAlbumFolders(Artist artist)
        {
            var seasonFolderMap = new Dictionary<int, string>();

            foreach (var folder in _diskProvider.GetDirectories(artist.Path))
            {
                var directoryinfo = new DirectoryInfo(folder);
                var seasonMatch = SeasonImagesRegex.Match(directoryinfo.Name);

                if (seasonMatch.Success)
                {
                    var seasonNumber = seasonMatch.Groups["season"].Value;

                    if (seasonNumber.Contains("specials"))
                    {
                        seasonFolderMap[0] = folder;
                    }
                    else
                    {
                        int matchedSeason;
                        if (int.TryParse(seasonNumber, out matchedSeason))
                        {
                            seasonFolderMap[matchedSeason] = folder;
                        }
                        else
                        {
                            _logger.Debug("Failed to parse season number from {0} for artist {1}.", folder, artist.Name);
                        }
                    }
                }

                else
                {
                    _logger.Debug("Rejecting folder {0} for artist {1}.", Path.GetDirectoryName(folder), artist.Name);
                }
            }

            return seasonFolderMap;
        }
    }
}
