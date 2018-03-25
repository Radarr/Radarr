using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.MediaInfo;
using System;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetMovieSample(NamingConfig nameSpec);
        string GetMovieFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private readonly IQualityDefinitionService _definitionService;

        private static MovieFile _movieFile;
        private static Movie _movie;

        public FileNameSampleService(IBuildFileNames buildFileNames, IQualityDefinitionService qualityDefinitionService)
        {
            _buildFileNames = buildFileNames;
            _definitionService = qualityDefinitionService;

            var mediaInfo = new MediaInfoModel()
            {
                VideoCodec = "AVC",
                VideoBitDepth = 8,
                AudioFormat = "DTS",
                AudioChannels = 6,
                AudioChannelPositions = "3/2/0.1",
                AudioLanguages = "English",
                Subtitles = "English/German"
            };

            var mediaInfoAnime = new MediaInfoModel()
            {
                VideoCodec = "AVC",
                VideoBitDepth = 8,
                AudioFormat = "DTS",
                AudioChannels = 6,
                AudioChannelPositions = "3/2/0.1",
                AudioLanguages = "Japanese",
                Subtitles = "Japanese/English"
            };

            _movieFile = new MovieFile
            {
                Quality = new QualityModel(_definitionService.Get(Quality.Bluray1080p), new Revision(2)),
                RelativePath = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE.mkv",
                SceneName = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE",
                ReleaseGroup = "EVOLVE",
                MediaInfo = mediaInfo,
                Edition = "Ultimate extended edition",
            };

            _movie = new Movie
            {
                Title = "The Movie: Title",
                Year = 2010,
                ImdbId = "tt0066921",
                MovieFile = _movieFile,
                MovieFileId = 1,
            };
        }

        public SampleResult GetMovieSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_movie, _movieFile, nameSpec),
            };

            return result;
        }

        public string GetMovieFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetMovieFolder(_movie, nameSpec);
        }

        private string BuildSample(Movie movie, MovieFile movieFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildFileName(movie, movieFile, nameSpec);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
