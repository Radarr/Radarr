using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Lyrics
{
    public class LyricService : ExtraFileManager<LyricFile>
    {
        private readonly ILyricFileService _lyricFileService;
        private readonly Logger _logger;

        public LyricService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               ILyricFileService lyricFileService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _lyricFileService = lyricFileService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> CreateAfterArtistScan(Artist artist, List<TrackFile> trackFiles)
        {
            return Enumerable.Empty<LyricFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Artist artist, TrackFile trackFile)
        {
            return Enumerable.Empty<LyricFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Artist artist, Album album, string artistFolder, string albumFolder)
        {
            return Enumerable.Empty<LyricFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Artist artist, List<TrackFile> trackFiles)
        {
            var subtitleFiles = _lyricFileService.GetFilesByArtist(artist.Id);

            var movedFiles = new List<LyricFile>();

            foreach (var trackFile in trackFiles)
            {
                var groupedExtraFilesForTrackFile = subtitleFiles.Where(m => m.TrackFileId == trackFile.Id)
                                                            .GroupBy(s => s.Extension).ToList();

                foreach (var group in groupedExtraFilesForTrackFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple lyric files found with the same extension for {0}", trackFile.Path);
                    }

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(copy, groupCount > 1);
                        movedFiles.AddIfNotNull(MoveFile(artist, trackFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _lyricFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Artist artist, TrackFile trackFile, string path, string extension, bool readOnly)
        {
            if (LyricFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                var suffix = GetSuffix(1, false);
                var subtitleFile = ImportFile(artist, trackFile, path, readOnly, extension, suffix);

                _lyricFileService.Upsert(subtitleFile);

                return subtitleFile;
            }

            return null;
        }

        private string GetSuffix(int copy, bool multipleCopies = false)
        {
            var suffixBuilder = new StringBuilder();

            if (multipleCopies)
            {
                suffixBuilder.Append(".");
                suffixBuilder.Append(copy);
            }

            return suffixBuilder.ToString();
        }
    }
}
