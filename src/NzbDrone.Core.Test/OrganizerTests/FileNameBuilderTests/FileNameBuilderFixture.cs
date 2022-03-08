using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [Platform(Exclude = "Win")]
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private List<MovieTranslation> _movieTranslations;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movieTranslations = new List<MovieTranslation>
            {
                new MovieTranslation
                {
                    Language = Language.German,
                    Title = "German South Park"
                },

                new MovieTranslation
                {
                    Language = Language.French,
                    Title = "French South Park"
                }
            };

            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .With(s => s.MovieMetadata.Value.OriginalTitle = "South of the Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RadarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                .Setup(v => v.All())
                .Returns(new List<CustomFormat>());

            Mocker.GetMock<IMovieTranslationService>()
                .Setup(v => v.GetAllTranslationsForMovieMetadata(It.IsAny<int>()))
                .Returns(_movieTranslations);
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

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Movie_dot_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Movie_dash_Title()
        {
            _namingConfig.StandardMovieFormat = "{Movie-Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_MOVIE_TITLE_with_all_caps()
        {
            _namingConfig.StandardMovieFormat = "{MOVIE TITLE}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_MOVIE_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardMovieFormat = "{mOvIe-tItLE}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(_movie.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_movie_title_with_all_lower_case()
        {
            _namingConfig.StandardMovieFormat = "{movie title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_replace_translated_movie_title()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title:FR}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("French South Park");
        }

        [Test]
        public void should_replace_translated_movie_title_with_base_title_if_invalid_code()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title:JP}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_translated_movie_title_with_base_title_if_no_translation_exists()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title:JA}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_translated_movie_title_with_fallback_if_no_translation_exists()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title:JP|FR}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("French South Park");
        }

        [Test]
        public void should_replace_translated_movie_title_with_original_if_no_translation_or_fallback_exists()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title:JP|CN}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park");
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
        public void should_replace_movie_original_title()
        {
            _namingConfig.StandardMovieFormat = "{Movie OriginalTitle}";
            _movie.MovieMetadata.Value.OriginalTitle = "South of the Park";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South of the Park");
        }

        [Test]
        public void should_replace_movie_certification()
        {
            _namingConfig.StandardMovieFormat = "{Movie Certification}";
            _movie.MovieMetadata.Value.Certification = "R";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("R");
        }

        [Test]
        public void should_replace_movie_collection()
        {
            _namingConfig.StandardMovieFormat = "{Movie Collection}";
            _movie.MovieMetadata.Value.CollectionTitle = "South Part Collection";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Part Collection");
        }

        [Test]
        public void should_be_empty_for_null_collection()
        {
            _namingConfig.StandardMovieFormat = "{Movie Collection}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().BeEmpty();
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
                   .Should().Be("South Park [HDTV-720p]");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameMovies = false;
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_movieFile.RelativePath));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameMovies = false;
            _movieFile.RelativePath = null;
            _movieFile.Path = @"C:\Test\Unsorted\Movie - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_movieFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameMovies = false;
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

        //TODO: Update this test or fix the underlying issue!
        /*
        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.";

            Subject.BuildFileName(new Movie { Title = "Chicago P.D." }, _movieFile)
                   .Should().Be("Chicago.P.D.");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}";

            Subject.BuildFileName( new Movie { Title = "Chicago P.D.." }, _movieFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }*/

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{_Quality.Title_}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park._HDTV-720p");
        }

        [Test]
        public void should_format_mediainfo_properly()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{MEDIAINFO.FULL}";

            _movieFile.MediaInfo = new MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioLanguages = new List<string> { "eng", "spa" },
                Subtitles = new List<string> { "eng", "spa", "ita" }
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.H264.DTS[EN+ES].[EN+ES+IT]");
        }

        [TestCase("nob", "NB")]
        [TestCase("swe", "SV")]
        [TestCase("zho", "ZH")]
        [TestCase("chi", "ZH")]
        [TestCase("fre", "FR")]
        [TestCase("rum", "RO")]
        [TestCase("per", "FA")]
        [TestCase("ger", "DE")]
        [TestCase("cze", "CS")]
        [TestCase("ice", "IS")]
        [TestCase("dut", "NL")]
        [TestCase("nor", "NO")]
        public void should_format_languagecodes_properly(string language, string code)
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{MEDIAINFO.FULL}";

            _movieFile.MediaInfo = new MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioChannels = 6,
                AudioLanguages = new List<string> { "eng" },
                Subtitles = new List<string> { language },
                SchemaRevision = 3
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be($"South.Park.H264.DTS.[{code}]");
        }

        [Test]
        public void should_exclude_english_in_mediainfo_audio_language()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{MEDIAINFO.FULL}";

            _movieFile.MediaInfo = new MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioLanguages = new List<string> { "eng" },
                Subtitles = new List<string> { "eng", "spa", "ita" }
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.H264.DTS.[EN+ES+IT]");
        }

        [Ignore("not currently supported")]
        [Test]
        public void should_format_mediainfo_3d_properly()
        {
            _namingConfig.StandardMovieFormat = "{Movie.Title}.{MEDIAINFO.3D}.{MediaInfo.Simple}";

            _movieFile.MediaInfo = new MediaInfoModel()
            {
                VideoFormat = "h264",
                VideoMultiViewCount = 2,
                AudioFormat = "dts",
                AudioLanguages = new List<string> { "eng" },
                Subtitles = new List<string> { "eng", "spa", "ita" }
            };

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South.Park.3D.h264.DTS");
        }

        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _movie.Title = "Venture Bros.";
            _namingConfig.StandardMovieFormat = "{Movie.Title}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Venture.Bros");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameMovies = true;
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
            _namingConfig.StandardMovieFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_wrap_proper_in_square_brackets()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Title}] {[Quality Proper]}";

            GivenProper();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [HDTV-720p] [Proper]");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Full}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_proper_only_when_a_proper()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Full}]";

            GivenProper();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [HDTV-720p Proper]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_real_when_a_real()
        {
            _namingConfig.StandardMovieFormat = "{Movie Title} [{Quality Full}]";
            GivenReal();

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park [HDTV-720p REAL]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardMovieFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("HDTV-720p");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardMovieFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Movie{0}Title}}", separator);

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(string.Format("HDTV-720p{0}South{0}Park", separator));
        }

        [Test]
        public void should_be_able_to_use_original_filename()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat = "{Movie Title} - {Original Filename}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30 Rock - 30 Rock - S01E01 - Test");
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("az")]
        [TestCase("tr-TR")]
        public void should_replace_all_tokens_for_different_cultures(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            _movie.TmdbId = 124578;
            _movie.Year = 2020;
            GivenMediaInfoModel();

            _namingConfig.StandardMovieFormat = "{Movie CleanTitle} ({Release Year}) [{Quality Title}] [tmdb-{TmdbId}] [{MediaInfo AudioCodec}]";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("South Park (2020) [HDTV-720p] [tmdb-124578] [DTS]");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _movie.Title = "30 Rock";
            _namingConfig.StandardMovieFormat = "{Original Filename}";

            _movieFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _movieFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Radarr_as_release_group_when_not_available()
        {
            _movieFile.ReleaseGroup = null;
            _namingConfig.StandardMovieFormat = "{Release Group}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be("Radarr");
        }

        [TestCase("{Movie Title}{-Release Group}", "South Park")]
        [TestCase("{Movie Title}{ Release Group}", "South Park")]
        [TestCase("{Movie Title}{ [Release Group]}", "South Park")]
        public void should_not_use_Radarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _movieFile.ReleaseGroup = null;
            _namingConfig.StandardMovieFormat = pattern;

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _movieFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardMovieFormat = "{Release Group}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(releaseGroup);
        }

        [TestCase("eng", "")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages(string audioLanguages, string expected)
        {
            _movieFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardMovieFormat = "{MediaInfo AudioLanguages}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }

        [TestCase("eng", "[EN]")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages_all(string audioLanguages, string expected)
        {
            _movieFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardMovieFormat = "{MediaInfo AudioLanguagesAll}";

            Subject.BuildFileName(_movie, _movieFile)
                   .Should().Be(expected);
        }

        [TestCase(HdrFormat.None, "South.Park")]
        [TestCase(HdrFormat.Hlg10, "South.Park.HDR")]
        [TestCase(HdrFormat.Hdr10, "South.Park.HDR")]
        public void should_include_hdr_for_mediainfo_videodynamicrange_with_valid_properties(HdrFormat hdrFormat, string expectedName)
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(hdrFormat: hdrFormat);

            Subject.BuildFileName(_movie, _movieFile)
                .Should().Be(expectedName);
        }

        [Test]
        public void should_update_media_info_if_token_configured_and_revision_is_old()
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(_movie, _movieFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_movieFile, _movie), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_no_movie_path_available()
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);
            _movie.Path = null;

            Subject.BuildFileName(_movie, _movieFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_movieFile, _movie), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_not_configured_and_revision_is_old()
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(_movie, _movieFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_movieFile, _movie), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_current()
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 5);

            Subject.BuildFileName(_movie, _movieFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_movieFile, _movie), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_newer()
        {
            _namingConfig.StandardMovieFormat =
                "{Movie.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 8);

            Subject.BuildFileName(_movie, _movieFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_movieFile, _movie), Times.Never());
        }

        private void GivenMediaInfoModel(string videoCodec = "h264",
            string audioCodec = "dts",
            int audioChannels = 6,
            int videoBitDepth = 8,
            HdrFormat hdrFormat = HdrFormat.None,
            string audioLanguages = "eng",
            string subtitles = "eng/spa/ita",
            int schemaRevision = 5)
        {
            _movieFile.MediaInfo = new MediaInfoModel
            {
                VideoFormat = videoCodec,
                AudioFormat = audioCodec,
                AudioChannels = audioChannels,
                AudioLanguages = audioLanguages.Split("/").ToList(),
                Subtitles = subtitles.Split("/").ToList(),
                VideoBitDepth = videoBitDepth,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = schemaRevision
            };
        }
    }
}
