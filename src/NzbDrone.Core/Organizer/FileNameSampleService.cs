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

        public MovieFile MovieFile
        {
            get
            {
                if (_movieFile == null)
                {
                    _movieFile = new MovieFile
                    {
                        Quality = new QualityModel(_definitionService.Get(Quality.Bluray1080p), new Revision(2)),
                        RelativePath = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE.mkv",
                        SceneName = "The.Movie.Title.2010.1080p.BluRay.DTS.x264-EVOLVE",
                        ReleaseGroup = "EVOLVE",
                        MediaInfo = new MediaInfoModel()
                        {
                            VideoCodec = "AVC",
                            VideoBitDepth = 8,
                            AudioFormat = "DTS",
                            AudioChannels = 6,
                            AudioChannelPositions = "3/2/0.1",
                            AudioLanguages = "English",
                            Subtitles = "English/German"
                        },
                        Edition = "Ultimate extended edition",
                    };
                }

                return _movieFile;
            }
            set => _movieFile = value;
        }

        private static MovieFile _movieFile;

        public Movie Movie
        {
            get
            {
                if (_movie == null)
                {
                    _movie = new Movie
                    {
                        Title = "The Movie: Title",
                        Year = 2010,
                        ImdbId = "tt0066921",
                        MovieFile = MovieFile,
                        MovieFileId = 1,
                    };
                }

                return _movie;
            }
            set => _movie = value;
        }

        private static Movie _movie;

        public FileNameSampleService(IBuildFileNames buildFileNames, IQualityDefinitionService qualityDefinitionService)
        {
            _buildFileNames = buildFileNames;
            _definitionService = qualityDefinitionService;
        }

        public SampleResult GetMovieSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(Movie, MovieFile, nameSpec),
            };

            return result;
        }

        public string GetMovieFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetMovieFolder(Movie, nameSpec);
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
