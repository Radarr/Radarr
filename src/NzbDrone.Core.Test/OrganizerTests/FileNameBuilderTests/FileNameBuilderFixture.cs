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
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Artist _artist;
        private Album _album;
        private Medium _medium;
        private AlbumRelease _release;
        private Track _track1;
        private TrackFile _trackFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>
                    .CreateNew()
                    .With(s => s.Name = "Linkin Park")
                    .With(s => s.Metadata = new ArtistMetadata {
                        Disambiguation = "US Rock Band",
                        Name = "Linkin Park"
                    })
                    .Build();

            _medium = Builder<Medium>
                .CreateNew()
                .With(m => m.Number = 3)
                .Build();

            _release = Builder<AlbumRelease>
                .CreateNew()
                .With(s => s.Media = new List<Medium> { _medium })
                .With(s => s.Monitored = true)
                .Build();

            _album = Builder<Album>
                .CreateNew()
                .With(s => s.Title = "Hybrid Theory")
                .With(s => s.AlbumType = "Album")
                .With(s => s.Disambiguation = "The Best Album")
                .Build();


            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameTracks = true;


            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _track1 = Builder<Track>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.AbsoluteTrackNumber = 6)
                            .With(e => e.AlbumRelease = _release)
                            .With(e => e.MediumNumber = _medium.Number)
                            .Build();

            _trackFile = Builder<TrackFile>.CreateNew()
                .With(e => e.Quality = new QualityModel(Quality.MP3_256))
                .With(e => e.ReleaseGroup = "LidarrTest")
                .With(e => e.MediaInfo = new Parser.Model.MediaInfoModel {
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
        public void should_replace_Artist_space_Name()
        {
            _namingConfig.StandardTrackFormat = "{Artist Name}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("Linkin Park");
        }

        [Test]
        public void should_replace_Artist_underscore_Name()
        {
            _namingConfig.StandardTrackFormat = "{Artist_Name}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("Linkin_Park");
        }

        [Test]
        public void should_replace_Artist_dot_Name()
        {
            _namingConfig.StandardTrackFormat = "{Artist.Name}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("Linkin.Park");
        }

        [Test]
        public void should_replace_Artist_dash_Name()
        {
            _namingConfig.StandardTrackFormat = "{Artist-Name}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("Linkin-Park");
        }

        [Test]
        public void should_replace_ARTIST_NAME_with_all_caps()
        {
            _namingConfig.StandardTrackFormat = "{ARTIST NAME}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("LINKIN PARK");
        }

        [Test]
        public void should_replace_ARTIST_NAME_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardTrackFormat = "{aRtIST-nAmE}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(_artist.Name.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_artist_name_with_all_lower_case()
        {
            _namingConfig.StandardTrackFormat = "{artist name}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("linkin park");
        }

        [Test]
        public void should_cleanup_Artist_Name()
        {
            _namingConfig.StandardTrackFormat = "{Artist.CleanName}";
            _artist.Name = "Linkin Park (1997)";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin.Park.1997");
        }

        [Test]
        public void should_replace_Artist_Disambiguation()
        {
            _namingConfig.StandardTrackFormat = "{Artist Disambiguation}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("US Rock Band");
        }

        [Test]
        public void should_replace_Album_space_Title()
        {
            _namingConfig.StandardTrackFormat = "{Album Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Hybrid Theory");
        }

        [Test]
        public void should_replace_Album_Type()
        {
            _namingConfig.StandardTrackFormat = "{Album Type}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                .Should().Be("Album");
        }

        [Test]
        public void should_replace_Album_Disambiguation()
        {
            _namingConfig.StandardTrackFormat = "{Album Disambiguation}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                .Should().Be("The Best Album");
        }

        [Test]
        public void should_replace_Album_underscore_Title()
        {
            _namingConfig.StandardTrackFormat = "{Album_Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Hybrid_Theory");
        }

        [Test]
        public void should_replace_Album_dot_Title()
        {
            _namingConfig.StandardTrackFormat = "{Album.Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Hybrid.Theory");
        }

        [Test]
        public void should_replace_Album_dash_Title()
        {
            _namingConfig.StandardTrackFormat = "{Album-Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Hybrid-Theory");
        }

        [Test]
        public void should_replace_ALBUM_TITLE_with_all_caps()
        {
            _namingConfig.StandardTrackFormat = "{ALBUM TITLE}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("HYBRID THEORY");
        }

        [Test]
        public void should_replace_ALBUM_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardTrackFormat = "{aLbUM-tItLE}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(_album.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_album_title_with_all_lower_case()
        {
            _namingConfig.StandardTrackFormat = "{album title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("hybrid theory");
        }

        [Test]
        public void should_cleanup_Album_Title()
        {
            _namingConfig.StandardTrackFormat = "{Artist.CleanName}";
            _artist.Name = "Hybrid Theory (2000)";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Hybrid.Theory.2000");
        }

        [Test]
        public void should_replace_track_title()
        {
            _namingConfig.StandardTrackFormat = "{Track Title}";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_replace_track_title_if_pattern_has_random_casing()
        {
            _namingConfig.StandardTrackFormat = "{tRaCK-TitLe}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("City-Sushi");
        }

        [Test]
        public void should_replace_track_number_with_single_digit()
        {
            _track1.AbsoluteTrackNumber = 1;
            _namingConfig.StandardTrackFormat = "{track}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("1");
        }

        [Test]
        public void should_replace_track00_number_with_two_digits()
        {
            _track1.AbsoluteTrackNumber = 1;
            _namingConfig.StandardTrackFormat = "{track:00}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("01");
        }

        [Test]
        public void should_replace_medium_number_with_single_digit()
        {
            _namingConfig.StandardTrackFormat = "{medium}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("3");
        }

        [Test]
        public void should_replace_medium00_number_with_two_digits()
        {
            _namingConfig.StandardTrackFormat = "{medium:00}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("03");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardTrackFormat = "{Quality Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("MP3-256");
        }

        [Test]
        public void should_replace_media_info_audio_codec()
        {
            _namingConfig.StandardTrackFormat = "{MediaInfo AudioCodec}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("FLAC");
        }

        [Test]
        public void should_replace_media_info_audio_bitrate()
        {
            _namingConfig.StandardTrackFormat = "{MediaInfo AudioBitRate}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("320 kbps");
        }

        [Test]
        public void should_replace_media_info_audio_channels()
        {
            _namingConfig.StandardTrackFormat = "{MediaInfo AudioChannels}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("2.0");
        }

        [Test]
        public void should_replace_media_info_bits_per_sample()
        {
            _namingConfig.StandardTrackFormat = "{MediaInfo AudioBitsPerSample}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("16bit");
        }

        [Test]
        public void should_replace_media_info_sample_rate()
        {
            _namingConfig.StandardTrackFormat = "{MediaInfo AudioSampleRate}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("44.1kHz");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardTrackFormat = "{Artist Name} - {Album Title} - {track:00} - {Track Title} [{Quality Title}]";

            Subject.BuildTrackFileName(new List<Track> {_track1}, _artist, _album, _trackFile)
                   .Should().Be("Linkin Park - Hybrid Theory - 06 - City Sushi [MP3-256]");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameTracks = false;
            _trackFile.Path = "Linkin Park - 06 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }
        
        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameTracks = false;
            _trackFile.Path = "Linkin Park - 06 - Test";
            _trackFile.SceneName = "SceneName";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameTracks = false;
            _trackFile.Path = @"C:\Test\Unsorted\Artist - 01 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void should_not_clean_track_title_if_there_is_only_one()
        {
            var title = "City Sushi (1)";
            _track1.Title = title;

            _namingConfig.StandardTrackFormat = "{Track Title}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(title);
        }

        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardTrackFormat = "{Release Group}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(_trackFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _artist.Name = "Linkin Park";
            _namingConfig.StandardTrackFormat = "{Artist Name} - {Original Title} - {Track Title}";

            _trackFile.SceneName = "Linkin.Park.Meteora.320-LOL";
            _trackFile.Path = "30 Rock - 01 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin Park - Linkin.Park.Meteora.320-LOL - City Sushi");
        }

        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardTrackFormat = "{Artist.Name}.{track:00}.{Track.Title}";

            var track = Builder<Track>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.AbsoluteTrackNumber = 6)
                            .With(e => e.AlbumRelease = _release)
                            .Build();

            Subject.BuildTrackFileName(new List<Track> { track }, new Artist { Name = "In The Woods." }, new Album { Title = "30 Rock" }, _trackFile)
                   .Should().Be("In.The.Woods.06.Part.1");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardTrackFormat = "{Artist.Name}.{track:00}.{Track.Title}";

            var track = Builder<Track>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.AbsoluteTrackNumber = 6)
                            .With(e => e.AlbumRelease = _release)
                            .Build();

            Subject.BuildTrackFileName(new List<Track> { track }, new Artist { Name = "In The Woods..." }, new Album { Title = "30 Rock" }, _trackFile)
                   .Should().Be("In.The.Woods.06.Part.1");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardTrackFormat = "{Artist.Name}.{track:00}{_Track.Title_}{Quality.Title}";
            
            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin.Park.06_City.Sushi_MP3-256");
        }

        [Test]
        public void should_not_include_affixes_if_value_empty()
        {
            _namingConfig.StandardTrackFormat = "{Artist.Name}.{track:00}{_Track.Title_}";

            _track1.Title = "";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin.Park.06");
        }

        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _artist.Name = "Venture Bros.";
            _namingConfig.StandardTrackFormat = "{Artist.Name}.{Album.Title}-{track:00}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Venture.Bros.Hybrid.Theory-06");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameTracks = true;
            _namingConfig.StandardTrackFormat = "{Original Title}";

            _trackFile.SceneName = null;
            _trackFile.Path = "existing.file.mkv";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_trackFile.Path));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _artist.Name = "30 Rock";
            _namingConfig.StandardTrackFormat = "{Original Title}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardTrackFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("MP3-256");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardTrackFormat = "{Artist Name} - {track:00} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin Park - 06 [MP3-256]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardTrackFormat = "{Artist Name} - {track:00} [{Quality Full}]";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Linkin Park - 06 [MP3-256]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardTrackFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("MP3-256");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardTrackFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Track{0}Title}}", separator);

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(string.Format("MP3-256{0}City{0}Sushi", separator));
        }



        [Test]
        public void should_be_able_to_use_original_filename()
        {
            _artist.Name = "30 Rock";
            _namingConfig.StandardTrackFormat = "{Artist Name} - {Original Filename}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("30 Rock - 30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _artist.Name = "30 Rock";
            _namingConfig.StandardTrackFormat = "{Original Filename}";

            _trackFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _trackFile.Path = "30 Rock - S01E01 - Test";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Lidarr_as_release_group_when_not_available()
        {
            _trackFile.ReleaseGroup = null;
            _namingConfig.StandardTrackFormat = "{Release Group}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be("Lidarr");
        }

        [TestCase("{Track Title}{-Release Group}", "City Sushi")]
        [TestCase("{Track Title}{ Release Group}", "City Sushi")]
        [TestCase("{Track Title}{ [Release Group]}", "City Sushi")]
        public void should_not_use_Lidarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _trackFile.ReleaseGroup = null;
            _namingConfig.StandardTrackFormat = pattern;

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _trackFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardTrackFormat = "{Release Group}";

            Subject.BuildTrackFileName(new List<Track> { _track1 }, _artist, _album, _trackFile)
                   .Should().Be(releaseGroup);
        }
    }
}
