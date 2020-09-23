using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Metadata
{
    public class MetadataService : ExtraFileManager<MetadataFile>
    {
        private readonly IMetadataFactory _metadataFactory;
        private readonly ICleanMetadataService _cleanMetadataService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IOtherExtraFileRenamer _otherExtraFileRenamer;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IHttpClient _httpClient;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly Logger _logger;

        public MetadataService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               IRecycleBinProvider recycleBinProvider,
                               IOtherExtraFileRenamer otherExtraFileRenamer,
                               IMetadataFactory metadataFactory,
                               ICleanMetadataService cleanMetadataService,
                               IHttpClient httpClient,
                               IMediaFileAttributeService mediaFileAttributeService,
                               IMetadataFileService metadataFileService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _metadataFactory = metadataFactory;
            _cleanMetadataService = cleanMetadataService;
            _otherExtraFileRenamer = otherExtraFileRenamer;
            _recycleBinProvider = recycleBinProvider;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _httpClient = httpClient;
            _mediaFileAttributeService = mediaFileAttributeService;
            _metadataFileService = metadataFileService;
            _logger = logger;
        }

        public override int Order => 0;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Movie movie)
        {
            _logger.Debug("Creating Movie Metadata after Cover Update for: {0}", Path.Combine(movie.Path, movie.MovieFile?.RelativePath ?? string.Empty));

            var metadataFiles = _metadataFileService.GetFilesByMovie(movie.Id);
            _cleanMetadataService.Clean(movie);

            if (!_diskProvider.FolderExists(movie.Path))
            {
                _logger.Info("Movie folder does not exist, skipping metadata image creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                files.AddRange(ProcessMovieImages(consumer, movie, movie.MovieFile, consumerFiles));
                files.AddIfNotNull(ProcessMovieMetadata(consumer, movie, movie.MovieFile, consumerFiles));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles)
        {
            var movieRelativePaths = string.Join(", ", movieFiles.Select(movieFile => Path.Combine(movie.Path, movieFile.RelativePath)));
            _logger.Debug("Creating Movie Metadata after Movie Scan for: {0}", movieRelativePaths);

            var metadataFiles = _metadataFileService.GetFilesByMovie(movie.Id);
            _cleanMetadataService.Clean(movie);

            if (!_diskProvider.FolderExists(movie.Path))
            {
                _logger.Info("Movie folder does not exist, skipping metadata creation");
                return Enumerable.Empty<MetadataFile>();
            }

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = GetMetadataFilesForConsumer(consumer, metadataFiles);

                foreach (var movieFile in movieFiles)
                {
                    files.AddRange(ProcessMovieImages(consumer, movie, movieFile, consumerFiles));
                    files.AddIfNotNull(ProcessMovieMetadata(consumer, movie, movieFile, consumerFiles));
                }
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile)
        {
            _logger.Debug("Creating Movie Metadata after Movie Import for: {0}", Path.Combine(movie.Path, movieFile.RelativePath));

            var files = new List<MetadataFile>();

            foreach (var consumer in _metadataFactory.Enabled())
            {
                var consumerFiles = new List<MetadataFile>();

                files.AddRange(ProcessMovieImages(consumer, movie, movieFile, consumerFiles));
                files.AddIfNotNull(ProcessMovieMetadata(consumer, movie, movieFile, consumerFiles));
            }

            _metadataFileService.Upsert(files);

            return files;
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieFolder(Movie movie, string movieFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles)
        {
            var movieRelativePaths = string.Join(", ", movieFiles.Select(movieFile => Path.Combine(movie.Path, movieFile.RelativePath)));
            _logger.Debug("Move Movie Files after Movie Rename for: {0}", movieRelativePaths);

            var metadataFiles = _metadataFileService.GetFilesByMovie(movie.Id);
            var movedFiles = new List<MetadataFile>();

            // TODO: Move EpisodeImage and EpisodeMetadata metadata files, instead of relying on consumers to do it
            // (Xbmc's EpisodeImage is more than just the extension)
            foreach (var consumer in _metadataFactory.GetAvailableProviders())
            {
                foreach (var movieFile in movieFiles)
                {
                    var metadataFilesForConsumer = GetMetadataFilesForConsumer(consumer, metadataFiles).Where(m => m.MovieFileId == movieFile.Id).ToList();

                    foreach (var metadataFile in metadataFilesForConsumer)
                    {
                        var newFileName = consumer.GetFilenameAfterMove(movie, movieFile, metadataFile);
                        var existingFileName = Path.Combine(movie.Path, metadataFile.RelativePath);

                        if (newFileName.PathNotEquals(existingFileName))
                        {
                            try
                            {
                                _diskProvider.MoveFile(existingFileName, newFileName);
                                metadataFile.RelativePath = movie.Path.GetRelativePath(newFileName);
                                movedFiles.Add(metadataFile);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to move metadata file after rename: {0}", existingFileName);
                            }
                        }
                    }
                }
            }

            _metadataFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalMovie localMovie, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            return false;
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalMovie localMovie, MovieFile movieFile, List<string> files, bool isReadOnly)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        private List<MetadataFile> GetMetadataFilesForConsumer(IMetadata consumer, List<MetadataFile> movieMetadata)
        {
            return movieMetadata.Where(c => c.Consumer == consumer.GetType().Name).ToList();
        }

        private MetadataFile ProcessMovieMetadata(IMetadata consumer, Movie movie, MovieFile movieFile, List<MetadataFile> existingMetadataFiles)
        {
            _logger.Debug("Processing Movie Metadata for: {0}", Path.Combine(movie.Path, movieFile.RelativePath));

            var movieFileMetadata = consumer.MovieMetadata(movie, movieFile);

            if (movieFileMetadata == null)
            {
                return null;
            }

            var fullPath = Path.Combine(movie.Path, movieFileMetadata.RelativePath);

            _otherExtraFileRenamer.RenameOtherExtraFile(movie, fullPath);

            var existingMetadata = GetMetadataFile(movie, existingMetadataFiles, c => c.Type == MetadataType.MovieMetadata &&
                                                                                  c.MovieFileId == movieFile.Id);

            if (existingMetadata != null)
            {
                var existingFullPath = Path.Combine(movie.Path, existingMetadata.RelativePath);
                if (fullPath.PathNotEquals(existingFullPath))
                {
                    _diskTransferService.TransferFile(existingFullPath, fullPath, TransferMode.Move);
                    existingMetadata.RelativePath = movieFileMetadata.RelativePath;
                }
            }

            var hash = movieFileMetadata.Contents.SHA256Hash();

            var metadata = existingMetadata ??
                           new MetadataFile
                           {
                               MovieId = movie.Id,
                               MovieFileId = movieFile.Id,
                               Consumer = consumer.GetType().Name,
                               Type = MetadataType.MovieMetadata,
                               RelativePath = movieFileMetadata.RelativePath,
                               Extension = Path.GetExtension(fullPath)
                           };

            if (hash == metadata.Hash)
            {
                return null;
            }

            _logger.Debug("Writing Movie File Metadata to: {0}", fullPath);
            SaveMetadataFile(fullPath, movieFileMetadata.Contents);

            metadata.Hash = hash;

            return metadata;
        }

        private List<MetadataFile> ProcessMovieImages(IMetadata consumer, Movie movie, MovieFile movieFile, List<MetadataFile> existingMetadataFiles)
        {
            _logger.Debug("Processing Movie Images for: {0}", Path.Combine(movie.Path, movieFile.RelativePath));

            var result = new List<MetadataFile>();

            foreach (var image in consumer.MovieImages(movie, movieFile))
            {
                var fullPath = Path.Combine(movie.Path, image.RelativePath);

                if (_diskProvider.FileExists(fullPath))
                {
                    _logger.Debug("Movie image already exists: {0}", fullPath);
                    continue;
                }

                _otherExtraFileRenamer.RenameOtherExtraFile(movie, fullPath);

                var hash = image.Url.SHA256Hash();

                var existingMetadata = GetMetadataFile(movie, existingMetadataFiles, c => c.Type == MetadataType.MovieImage &&
                                                                                  c.Hash == hash);

                if (existingMetadata != null)
                {
                    var existingFullPath = Path.Combine(movie.Path, existingMetadata.RelativePath);
                    if (fullPath.PathNotEquals(existingFullPath))
                    {
                        _diskTransferService.TransferFile(existingFullPath, fullPath, TransferMode.Move);
                        existingMetadata.RelativePath = image.RelativePath;
                    }
                }

                var metadata = existingMetadata ??
                               new MetadataFile
                               {
                                   MovieId = movie.Id,
                                   MovieFileId = movieFile.Id,
                                   Consumer = consumer.GetType().Name,
                                   Type = MetadataType.MovieImage,
                                   RelativePath = image.RelativePath,
                                   Extension = Path.GetExtension(fullPath)
                               };

                if (hash == metadata.Hash)
                {
                    return null;
                }

                DownloadImage(movie, image);

                metadata.Hash = hash;

                result.Add(metadata);
            }

            return result;
        }

        private void DownloadImage(Movie movie, ImageFileResult image)
        {
            _logger.Debug("Download Movie Image for: {0}", Path.Combine(movie.Path, movie.MovieFile?.RelativePath ?? string.Empty));

            var fullPath = Path.Combine(movie.Path, image.RelativePath);

            try
            {
                if (image.Url.StartsWith("http"))
                {
                    _httpClient.DownloadFile(image.Url, fullPath);
                }
                else
                {
                    _diskProvider.CopyFile(image.Url, fullPath);
                }

                _mediaFileAttributeService.SetFilePermissions(fullPath);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex, "Couldn't download image {0} for {1}. {2}", image.Url, movie, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex, "Couldn't download image {0} for {1}. {2}", image.Url, movie, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Couldn't download image {0} for {1}. {2}", image.Url, movie, ex.Message);
            }
        }

        private void SaveMetadataFile(string path, string contents)
        {
            _diskProvider.WriteAllText(path, contents);
            _mediaFileAttributeService.SetFilePermissions(path);
        }

        private MetadataFile GetMetadataFile(Movie movie, List<MetadataFile> existingMetadataFiles, Func<MetadataFile, bool> predicate)
        {
            var matchingMetadataFiles = existingMetadataFiles.Where(predicate).ToList();

            if (matchingMetadataFiles.Empty())
            {
                return null;
            }

            // Remove duplicate metadata files from DB and disk
            foreach (var file in matchingMetadataFiles.Skip(1))
            {
                var path = Path.Combine(movie.Path, file.RelativePath);

                _logger.Debug("Removing duplicate Metadata file: {0}", path);

                _recycleBinProvider.DeleteFile(path);
                _metadataFileService.Delete(file.Id);
            }

            return matchingMetadataFiles.First();
        }
    }
}
