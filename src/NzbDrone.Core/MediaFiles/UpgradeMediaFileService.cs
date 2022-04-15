using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        MovieFileMoveResult UpgradeMovieFile(MovieFile movieFile, LocalMovie localMovie, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _customFormatCalculator;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveMovieFiles _movieFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IUpgradableSpecification upgradableSpecification,
                                       ICustomFormatCalculationService customFormatCalculator,
                                       IMediaFileService mediaFileService,
                                       IMoveMovieFiles movieFileMover,
                                       IDiskProvider diskProvider,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _upgradableSpecification = upgradableSpecification;
            _customFormatCalculator = customFormatCalculator;
            _mediaFileService = mediaFileService;
            _movieFileMover = movieFileMover;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public MovieFileMoveResult UpgradeMovieFile(MovieFile movieFile, LocalMovie localMovie, bool copyOnly = false)
        {
            _logger.Trace("Upgrading existing movie file.");
            var moveFileResult = new MovieFileMoveResult();

            var existingFiles = localMovie.Movie.MovieFiles.Value;

            var rootFolder = _diskProvider.GetParentFolder(localMovie.Movie.Path);

            // If there are existing movie files and the root folder is missing, throw, so the old file isn't left behind during the import process.
            if (existingFiles.Count > 0 && !_diskProvider.FolderExists(rootFolder))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolder}' was not found.");
            }

            if (existingFiles.Any())
            {
                var profileBests = new Dictionary<int, MovieFile>();

                foreach (var profile in localMovie.Movie.QualityProfiles.Value)
                {
                    profileBests.Add(profile.Id, CalculateBestFileForProfile(profile, movieFile, existingFiles));
                }

                foreach (var file in existingFiles)
                {
                    if (!profileBests.Any(f => f.Value.Id == file.Id))
                    {
                        var movieFilePath = Path.Combine(localMovie.Movie.Path, file.RelativePath);
                        var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(movieFilePath));

                        if (_diskProvider.FileExists(movieFilePath))
                        {
                            _logger.Debug("Removing existing movie file: {0}", file);
                            _recycleBinProvider.DeleteFile(movieFilePath, subfolder);
                        }

                        moveFileResult.OldFiles.Add(file);
                        _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
                    }
                }
            }

            if (copyOnly)
            {
                moveFileResult.MovieFile = _movieFileMover.CopyMovieFile(movieFile, localMovie);
            }
            else
            {
                moveFileResult.MovieFile = _movieFileMover.MoveMovieFile(movieFile, localMovie);
            }

            return moveFileResult;
        }

        private MovieFile CalculateBestFileForProfile(Profile profile, MovieFile newFile, List<MovieFile> existingFiles)
        {
            var files = existingFiles;

            files.ForEach(x => x.Movie = newFile.Movie);

            var bestFileForProfile = newFile;
            var bestCustomFormats = _customFormatCalculator.ParseCustomFormat(bestFileForProfile);

            foreach (var file in files)
            {
                // Is file quality allowed in profile, if not skip this as best
                var qualityIndex = profile.GetIndex(file.Quality.Quality);
                var qualityOrGroup = profile.Items[qualityIndex.Index];

                if (!qualityOrGroup.Allowed)
                {
                    continue;
                }

                var currentCustomFormats = _customFormatCalculator.ParseCustomFormat(file);

                if (_upgradableSpecification.IsUpgradable(profile, bestFileForProfile.Quality, bestCustomFormats, file.Quality, currentCustomFormats))
                {
                    bestFileForProfile = file;
                    bestCustomFormats = currentCustomFormats;
                }
            }

            return bestFileForProfile;
        }
    }
}
