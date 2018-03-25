using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();


            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;


            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _movieFile = new MovieFile { Quality = new QualityModel(new QualityDefinition(Quality.HDTV720p)), ReleaseGroup = "SonarrTest" };

            /*Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => QualityDefinition.DefaultQualityDefinitions.First(c => c.Quality == v));*/
        }

        private void GivenProper()
        {
            _movieFile.Quality.Revision.Version = 2;
        }

        private void GivenReal()
        {
            _movieFile.Quality.Revision.Real = 1;
        }

        [Test]
        public void should_replace_Movie_space_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_Movie_underscore_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie_Title}";

            Subject.BuildFileName( _movie, _movieFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Movie_dot_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}";

            Subject.BuildFileName( _movie, _movieFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Movie_dash_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie-Title}";

            Subject.BuildFileName( _movie, _movieFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_all_caps()
        {
            _namingConfig.StandardMovieFormat = "{SERIES TITLE}";

            Subject.BuildFileName( _movie, _movieFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardMovieFormat = "{sErIES-tItLE}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(_movie.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_series_title_with_all_lower_case()
        {
            _namingConfig.StandardMovieFormat = "{series title}";

            Subject.BuildFileName( _movie, _movieFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_cleanup_Movie_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie.CleanTitle}";
            _movie.Title = "South Park (1997)";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.1997");
        }




        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardMovieFormat = "{Quality Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_replace_quality_proper_with_proper()
        {
            _namingConfig.StandardMovieFormat = "{Quality Proper}";
            GivenProper();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Proper");
        }

        [Test]
        public void should_replace_quality_real_with_real()
        {
            _namingConfig.StandardMovieFormat = "{Quality Real}";
            GivenReal();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("REAL");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Title}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameEpisodes = false;
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_movieFile.RelativePath));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameEpisodes = false;
            _movieFile.RelativePath = null;
            _movieFile.Path = @"C:\Test\Unsorted\Movie - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_movieFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameEpisodes = false;
            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }




        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardMovieFormat = "{Release Group}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(_movieFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat = "{Movie Title} - {Original Title}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30 Rock - 30.Rock.S01E01.xvid-LOL");
        }


        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.";

            Subject.BuildFileName(new Movie { Title = "Chicago P.D." }, _movieFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.S{season:00}E{episode:00}.{Episode.Title}";

            Subject.BuildFileName( new Movie { Title = "Chicago P.D.." }, _movieFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.S{season:00}E{episode:00}{_Episode.Title_}{Quality.Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.S15E06_City.Sushi_HDTV-720p");
        }

        [Test]
        public void should_format_mediainfo_properly()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MEDIAINFO.FULL}";

            _movieFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoCodec = "AVC",
                AudioFormat = "DTS",
                AudioLanguages = "English/Spanish",
                Subtitles = "English/Spanish/Italian"
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.X264.DTS[EN+ES].[EN+ES+IT]");
        }

        [Test]
        public void should_exclude_english_in_mediainfo_audio_language()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MEDIAINFO.FULL}";

            _movieFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoCodec = "AVC",
                AudioFormat = "DTS",
                AudioLanguages = "English",
                Subtitles = "English/Spanish/Italian"
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.X264.DTS.[EN+ES+IT]");
        }

        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _movie.Title = "Venture Bros.";
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{season}x{episode:00}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Venture.Bros.15x06");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameEpisodes = true;
            _namingConfig.StandardMovieFormat = "{Original Title}";

            _movieFile.SceneName = null;
            _movieFile.RelativePath = "existing.file.mkv";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_movieFile.RelativePath));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat = "{Original Title}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }



        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardMovieFormat= "{Quality Title} {Quality Proper}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_wrap_proper_in_square_brackets()
        {
            _namingConfig.StandardMovieFormat= "{Movie Title} - S{season:00}E{episode:00} [{Quality Title}] {[Quality Proper]}";

            GivenProper();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p] [Proper]");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardMovieFormat= "{Movie Title} - S{season:00}E{episode:00} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardMovieFormat= "{Movie Title} - S{season:00}E{episode:00} [{Quality Full}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_proper_only_when_a_proper()
        {
            _namingConfig.StandardMovieFormat= "{Movie Title} - S{season:00}E{episode:00} [{Quality Full}]";

            GivenProper();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p Proper]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_real_when_a_real()
        {
            _namingConfig.StandardMovieFormat= "{Movie Title} - S{season:00}E{episode:00} [{Quality Full}]";
            GivenReal();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p REAL]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardMovieFormat= string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("HDTV-720p");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardMovieFormat= string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Episode{0}Title}}", separator);

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(string.Format("HDTV-720p{0}City{0}Sushi", separator));
        }

        [Test]
        public void should_be_able_to_use_original_filename()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat= "{Movie Title} - {Original Filename}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30 Rock - 30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat= "{Original Filename}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Sonarr_as_release_group_when_not_available()
        {
            _movieFile.ReleaseGroup = null;
            _namingConfig.StandardMovieFormat= "{Release Group}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Radarr");
        }

        [TestCase("{Episode Title}{-Release Group}", "City Sushi")]
        [TestCase("{Episode Title}{ Release Group}", "City Sushi")]
        [TestCase("{Episode Title}{ [Release Group]}", "City Sushi")]
        public void should_not_use_Sonarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _movieFile.ReleaseGroup = null;
            _namingConfig.StandardMovieFormat= pattern;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _movieFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardMovieFormat= "{Release Group}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(releaseGroup);
        }


    }
}
