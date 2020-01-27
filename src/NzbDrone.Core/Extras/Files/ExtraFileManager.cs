using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Files
{
    public interface IManageExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles);
        IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile);
        IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, string movieFolder);
        IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles);
        ExtraFile Import(Movie movie, MovieFile movieFile, string path, string extension, bool readOnly);
    }

    public abstract class ExtraFileManager<TExtraFile> : IManageExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public ExtraFileManager(IConfigService configService,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService,
                                Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = logger;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles);
        public abstract IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile);
        public abstract IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, string movieFolder);
        public abstract IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles);
        public abstract ExtraFile Import(Movie movie, MovieFile movieFile, string path, string extension, bool readOnly);

        protected TExtraFile ImportFile(Movie movie, MovieFile movieFile, string path, bool readOnly, string extension, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(Path.Combine(movie.Path, movieFile.RelativePath));
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(movieFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extension);

            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());
            var transferMode = TransferMode.Move;

            if (readOnly)
            {
                transferMode = _configService.CopyUsingHardlinks ? TransferMode.HardLinkOrCopy : TransferMode.Copy;
            }

            _diskTransferService.TransferFile(path, newFileName, transferMode, true, false);

            return new TExtraFile
            {
                MovieId = movie.Id,
                MovieFileId = movieFile.Id,
                RelativePath = movie.Path.GetRelativePath(newFileName),
                Extension = extension
            };
        }

        protected TExtraFile MoveFile(Movie movie, MovieFile movieFile, TExtraFile extraFile, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(Path.Combine(movie.Path, movieFile.RelativePath));
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(movieFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extraFile.Extension);

            var existingFileName = Path.Combine(movie.Path, extraFile.RelativePath);
            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());

            if (newFileName.PathNotEquals(existingFileName))
            {
                try
                {
                    _diskProvider.MoveFile(existingFileName, newFileName);
                    extraFile.RelativePath = movie.Path.GetRelativePath(newFileName);

                    return extraFile;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to move file after rename: {0}", existingFileName);
                }
            }

            return null;
        }
    }
}
