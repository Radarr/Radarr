using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Author _author;
        private Book _book;
        private Edition _edition;
        private BookFile _trackFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>
                    .CreateNew()
                    .With(s => s.Name = "Linkin Park")
                    .With(s => s.Metadata = new AuthorMetadata
                    {
                        Disambiguation = "US Rock Band",
                        Name = "Linkin Park"
                    })
                    .Build();

            _book = Builder<Book>
                .CreateNew()
                .With(s => s.Title = "Hybrid Theory")
                .Build();

            _edition = Builder<Edition>
                .CreateNew()
                .With(s => s.Title = _book.Title)
                .With(s => s.Disambiguation = "The Best Book")
                .With(s => s.Book = _book)
                .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameBooks = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _trackFile = Builder<BookFile>.CreateNew()
                .With(e => e.Quality = new QualityModel(Quality.MP3_320))
                .With(e => e.ReleaseGroup = "ReadarrTest")
                .With(e => e.MediaInfo = new Parser.Model.MediaInfoModel
                {
                    AudioBitrate = 320,
                    AudioBits = 16,
                    AudioChannels = 2,
                    AudioFormat = "Flac Audio",
                    AudioSampleRate = 44100
                }).Build();

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        private void GivenProper()
        {
            _trackFile.Quality.Revision.Version = 2;
        }

        private void GivenReal()
        {
            _trackFile.Quality.Revision.Real = 1;
        }

        [Test]
        public void should_replace_Author_space_Name()
        {
            _namingConfig.StandardBookFormat = "{Author Name}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin Park");
        }

        [Test]
        public void should_replace_Author_underscore_Name()
        {
            _namingConfig.StandardBookFormat = "{Author_Name}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin_Park");
        }

        [Test]
        public void should_replace_Author_dot_Name()
        {
            _namingConfig.StandardBookFormat = "{Author.Name}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin.Park");
        }

        [Test]
        public void should_replace_Author_dash_Name()
        {
            _namingConfig.StandardBookFormat = "{Author-Name}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin-Park");
        }

        [Test]
        public void should_replace_ARTIST_NAME_with_all_caps()
        {
            _namingConfig.StandardBookFormat = "{AUTHOR NAME}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("LINKIN PARK");
        }

        [Test]
        public void should_replace_ARTIST_NAME_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardBookFormat = "{aUtHoR-nAmE}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(_author.Name.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_author_name_with_all_lower_case()
        {
            _namingConfig.StandardBookFormat = "{author name}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("linkin park");
        }

        [Test]
        public void should_cleanup_Author_Name()
        {
            _namingConfig.StandardBookFormat = "{Author.CleanName}";
            _author.Name = "Linkin Park (1997)";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin.Park.1997");
        }

        [Test]
        public void should_replace_Author_Disambiguation()
        {
            _namingConfig.StandardBookFormat = "{Author Disambiguation}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("US Rock Band");
        }

        [Test]
        public void should_replace_edition_space_Title()
        {
            _namingConfig.StandardBookFormat = "{Book Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Hybrid Theory");
        }

        [Test]
        public void should_replace_Book_Disambiguation()
        {
            _namingConfig.StandardBookFormat = "{Book Disambiguation}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                .Should().Be("The Best Book");
        }

        [Test]
        public void should_replace_Book_underscore_Title()
        {
            _namingConfig.StandardBookFormat = "{Book_Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Hybrid_Theory");
        }

        [Test]
        public void should_replace_Book_dot_Title()
        {
            _namingConfig.StandardBookFormat = "{Book.Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Hybrid.Theory");
        }

        [Test]
        public void should_replace_Book_dash_Title()
        {
            _namingConfig.StandardBookFormat = "{Book-Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Hybrid-Theory");
        }

        [Test]
        public void should_replace_ALBUM_TITLE_with_all_caps()
        {
            _namingConfig.StandardBookFormat = "{BOOK TITLE}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("HYBRID THEORY");
        }

        [Test]
        public void should_replace_ALBUM_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardBookFormat = "{bOoK-tItLE}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(_book.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_book_title_with_all_lower_case()
        {
            _namingConfig.StandardBookFormat = "{book title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("hybrid theory");
        }

        [Test]
        public void should_cleanup_Book_Title()
        {
            _namingConfig.StandardBookFormat = "{Author.CleanName}";
            _author.Name = "Hybrid Theory (2000)";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Hybrid.Theory.2000");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardBookFormat = "{Quality Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("MP3-320");
        }

        [Test]
        public void should_replace_media_info_audio_codec()
        {
            _namingConfig.StandardBookFormat = "{MediaInfo AudioCodec}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("FLAC");
        }

        [Test]
        public void should_replace_media_info_audio_bitrate()
        {
            _namingConfig.StandardBookFormat = "{MediaInfo AudioBitRate}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("320 kbps");
        }

        [Test]
        public void should_replace_media_info_audio_channels()
        {
            _namingConfig.StandardBookFormat = "{MediaInfo AudioChannels}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("2.0");
        }

        [Test]
        public void should_replace_media_info_bits_per_sample()
        {
            _namingConfig.StandardBookFormat = "{MediaInfo AudioBitsPerSample}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("16bit");
        }

        [Test]
        public void should_replace_media_info_sample_rate()
        {
            _namingConfig.StandardBookFormat = "{MediaInfo AudioSampleRate}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("44.1kHz");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardBookFormat = "{Author Name} - {Book Title} - [{Quality Title}]";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin Park - Hybrid Theory - [MP3-320]");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameBooks = false;
            _trackFile.Path = "Linkin Park - 06 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameBooks = false;
            _trackFile.Path = "Linkin Park - 06 - Test";
            _trackFile.SceneName = "SceneName";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameBooks = false;
            _trackFile.Path = @"C:\Test\Unsorted\Author - 01 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardBookFormat = "{Release Group}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(_trackFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _author.Name = "Linkin Park";
            _namingConfig.StandardBookFormat = "{Author Name} - {Original Title}";

            _trackFile.SceneName = "Linkin.Park.Meteora.320-LOL";
            _trackFile.Path = "30 Rock - 01 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin Park - Linkin.Park.Meteora.320-LOL");
        }

        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardBookFormat = "{Author.Name}.{Book.Title}";

            Subject.BuildBookFileName(new Author { Name = "In The Woods." }, new Edition { Title = "30 Rock" }, _trackFile)
                   .Should().Be("In.The.Woods.30.Rock");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardBookFormat = "{Author.Name}.{Book.Title}";

            Subject.BuildBookFileName(new Author { Name = "In The Woods..." }, new Edition { Title = "30 Rock" }, _trackFile)
                   .Should().Be("In.The.Woods.30.Rock");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardBookFormat = "{Author.Name}{_Book.Title_}{Quality.Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin.Park_Hybrid.Theory_MP3-320");
        }

        [Test]
        public void should_not_include_affixes_if_value_empty()
        {
            _namingConfig.StandardBookFormat = "{Author.Name}{_Book.Title_}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin.Park_Hybrid.Theory");
        }

        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _author.Name = "Venture Bros.";
            _namingConfig.StandardBookFormat = "{Author.Name}.{Book.Title}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Venture.Bros.Hybrid.Theory");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameBooks = true;
            _namingConfig.StandardBookFormat = "{Original Title}";

            _trackFile.SceneName = null;
            _trackFile.Path = "existing.file.mkv";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _author.Name = "30 Rock";
            _namingConfig.StandardBookFormat = "{Original Title}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardBookFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("MP3-320");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardBookFormat = "{Author Name} - {Book Title} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin Park - Hybrid Theory [MP3-320]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardBookFormat = "{Author Name} - {Book Title} [{Quality Full}]";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Linkin Park - Hybrid Theory [MP3-320]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardBookFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("MP3-320");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardBookFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Book{0}Title}}", separator);

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(string.Format("MP3-320{0}Hybrid{0}Theory", separator));
        }

        [Test]
        public void should_be_able_to_use_original_filename()
        {
            _author.Name = "30 Rock";
            _namingConfig.StandardBookFormat = "{Author Name} - {Original Filename}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("30 Rock - 30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _author.Name = "30 Rock";
            _namingConfig.StandardBookFormat = "{Original Filename}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Readarr_as_release_group_when_not_available()
        {
            _trackFile.ReleaseGroup = null;
            _namingConfig.StandardBookFormat = "{Release Group}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be("Readarr");
        }

        [TestCase("{Book Title}{-Release Group}", "Hybrid Theory")]
        [TestCase("{Book Title}{ Release Group}", "Hybrid Theory")]
        [TestCase("{Book Title}{ [Release Group]}", "Hybrid Theory")]
        public void should_not_use_Readarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _trackFile.ReleaseGroup = null;
            _namingConfig.StandardBookFormat = pattern;

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _trackFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardBookFormat = "{Release Group}";

            Subject.BuildBookFileName(_author, _edition, _trackFile)
                   .Should().Be(releaseGroup);
        }
    }
}
