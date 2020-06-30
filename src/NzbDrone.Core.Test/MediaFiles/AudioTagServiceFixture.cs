using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.AudioTagServiceFixture
{
    [TestFixture]
    [Ignore("Readarr doesn't currently support audio")]
    public class AudioTagServiceFixture : CoreTest<AudioTagService>
    {
        public static class TestCaseFactory
        {
            private static readonly string[] MediaFiles = new[] { "nin.mp2", "nin.mp3", "nin.flac", "nin.m4a", "nin.wma", "nin.ape", "nin.opus" };

            private static readonly string[] SkipProperties = new[] { "IsValid", "Duration", "Quality", "MediaInfo", "ImageFile" };
            private static readonly Dictionary<string, string[]> SkipPropertiesByFile = new Dictionary<string, string[]>
            {
                { "nin.mp2", new[] { "OriginalReleaseDate" } }
            };

            public static IEnumerable TestCases
            {
                get
                {
                    foreach (var file in MediaFiles)
                    {
                        var toSkip = SkipProperties;
                        if (SkipPropertiesByFile.ContainsKey(file))
                        {
                            toSkip = toSkip.Union(SkipPropertiesByFile[file]).ToArray();
                        }

                        yield return new TestCaseData(file, toSkip).SetName($"{{m}}_{file.Replace("nin.", "")}");
                    }
                }
            }
        }

        private readonly string _testdir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media");
        private string _copiedFile;
        private AudioTag _testTags;
        private IDiskProvider _diskProvider;

        [SetUp]
        public void Setup()
        {
            _diskProvider = Mocker.Resolve<IDiskProvider>("ActualDiskProvider");

            Mocker.SetConstant<IDiskProvider>(_diskProvider);

            Mocker.GetMock<IConfigService>()
                .Setup(x => x.WriteAudioTags)
                .Returns(WriteAudioTagsType.Sync);

            var imageFile = Path.Combine(_testdir, "nin.png");
            var imageSize = _diskProvider.GetFileSize(imageFile);

            // have to manually set the arrays of string parameters and integers to values > 1
            _testTags = Builder<AudioTag>.CreateNew()
                .With(x => x.Track = 2)
                .With(x => x.TrackCount = 33)
                .With(x => x.Disc = 44)
                .With(x => x.DiscCount = 55)
                .With(x => x.Date = new DateTime(2019, 3, 1))
                .With(x => x.Year = 2019)
                .With(x => x.OriginalReleaseDate = new DateTime(2009, 4, 1))
                .With(x => x.OriginalYear = 2009)
                .With(x => x.Performers = new[] { "Performer1" })
                .With(x => x.AlbumArtists = new[] { "방탄소년단" })
                .With(x => x.Genres = new[] { "Genre1", "Genre2" })
                .With(x => x.ImageFile = imageFile)
                .With(x => x.ImageSize = imageSize)
                .Build();
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_copiedFile))
            {
                File.Delete(_copiedFile);
            }
        }

        private void GivenFileCopy(string filename)
        {
            var original = Path.Combine(_testdir, filename);
            var tempname = $"temp_{Path.GetRandomFileName()}{Path.GetExtension(filename)}";
            _copiedFile = Path.Combine(_testdir, tempname);

            File.Copy(original, _copiedFile);
        }

        private void VerifyDifferent(AudioTag a, AudioTag b, string[] skipProperties)
        {
            foreach (var property in typeof(AudioTag).GetProperties())
            {
                if (skipProperties.Contains(property.Name))
                {
                    continue;
                }

                if (property.CanRead)
                {
                    if (property.PropertyType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>)) ||
                        Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        var val1 = property.GetValue(a, null);
                        var val2 = property.GetValue(b, null);
                        val1.Should().NotBe(val2, $"{property.Name} should not be equal.  Found {val1.NullSafe()} for both tags");
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var val1 = (IEnumerable)property.GetValue(a, null);
                        var val2 = (IEnumerable)property.GetValue(b, null);

                        if (val1 != null && val2 != null)
                        {
                            val1.Should().NotBeEquivalentTo(val2, $"{property.Name} should not be equal");
                        }
                    }
                }
            }
        }

        private void VerifySame(AudioTag a, AudioTag b, string[] skipProperties)
        {
            foreach (var property in typeof(AudioTag).GetProperties())
            {
                if (skipProperties.Contains(property.Name))
                {
                    continue;
                }

                if (property.CanRead)
                {
                    if (property.PropertyType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>)) ||
                        Nullable.GetUnderlyingType(property.PropertyType) != null)
                    {
                        var val1 = property.GetValue(a, null);
                        var val2 = property.GetValue(b, null);
                        val1.Should().Be(val2, $"{property.Name} should be equal");
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var val1 = (IEnumerable)property.GetValue(a, null);
                        var val2 = (IEnumerable)property.GetValue(b, null);

                        if (val1 != null || val2 != null)
                        {
                            val1.Should().BeEquivalentTo(val2, $"{property.Name} should be equal");
                        }
                    }
                }
            }
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_duration(string filename, string[] ignored)
        {
            var path = Path.Combine(_testdir, filename);

            var tags = Subject.ReadTags(path);

            tags.Duration.Should().BeCloseTo(new TimeSpan(0, 0, 1, 25, 130), 100);
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_write_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = _copiedFile;

            var initialtags = Subject.ReadAudioTag(path);

            VerifyDifferent(initialtags, _testTags, skipProperties);

            _testTags.Write(path);

            var writtentags = Subject.ReadAudioTag(path);

            VerifySame(writtentags, _testTags, skipProperties);
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_audiotag_from_file_with_no_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = _copiedFile;

            Subject.RemoveAllTags(path);

            var tag = Subject.ReadAudioTag(path);
            var expected = new AudioTag()
            {
                Performers = new string[0],
                AlbumArtists = new string[0],
                Genres = new string[0]
            };

            VerifySame(tag, expected, skipProperties);
            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_parsedtrackinfo_from_file_with_no_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = _copiedFile;

            Subject.RemoveAllTags(path);

            var tag = Subject.ReadTags(path);

            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_set_quality_and_mediainfo_for_corrupt_file(string filename, string[] skipProperties)
        {
            // use missing to simulate corrupt
            var tag = Subject.ReadAudioTag(filename.Replace("nin", "missing"));
            var expected = new AudioTag();

            VerifySame(tag, expected, skipProperties);
            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_file_with_only_title_tag(string filename, string[] ignored)
        {
            GivenFileCopy(filename);
            var path = _copiedFile;

            Subject.RemoveAllTags(path);

            var nametag = new AudioTag();
            nametag.Title = "test";
            nametag.Write(path);

            var tag = Subject.ReadTags(path);
            tag.Title.Should().Be("test");

            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_remove_date_from_tags_when_not_in_metadata(string filename, string[] ignored)
        {
            GivenFileCopy(filename);
            var path = _copiedFile;

            _testTags.Write(path);

            _testTags.Date = null;
            _testTags.OriginalReleaseDate = null;

            _testTags.Write(path);

            var onDisk = Subject.ReadAudioTag(path);

            onDisk.Date.HasValue.Should().BeFalse();
            onDisk.OriginalReleaseDate.HasValue.Should().BeFalse();
        }

        [Test]
        public void should_ignore_non_parsable_id3v23_date()
        {
            GivenFileCopy("nin.mp2");

            using (var file = TagLib.File.Create(_copiedFile))
            {
                var id3tag = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
                id3tag.SetTextFrame("TORY", "0");
                file.Save();
            }

            var tag = Subject.ReadAudioTag(_copiedFile);
            tag.OriginalReleaseDate.HasValue.Should().BeFalse();
        }

        private BookFile GivenPopulatedTrackfile(int mediumOffset)
        {
            var meta = Builder<AuthorMetadata>.CreateNew().Build();
            var artist = Builder<Author>.CreateNew()
                .With(x => x.Metadata = meta)
                .Build();

            var album = Builder<Book>.CreateNew()
                .With(x => x.Author = artist)
                .Build();

            var edition = Builder<Edition>.CreateNew()
                .With(x => x.Book = album)
                .Build();

            var file = Builder<BookFile>.CreateNew()
                .With(x => x.Edition = edition)
                .With(x => x.Author = artist)
                .Build();

            return file;
        }

        [Test]
        public void get_metadata_should_not_fail_with_missing_country()
        {
            var file = GivenPopulatedTrackfile(0);
            var tag = Subject.GetTrackMetadata(file);
        }

        [Test]
        public void should_not_fail_if_media_has_been_omitted()
        {
            // make sure that we aren't relying on index of items in
            // Media being the same as the medium number
            var file = GivenPopulatedTrackfile(100);
            var tag = Subject.GetTrackMetadata(file);

            tag.Media.Should().NotBeNull();
        }

        [TestCase("nin.mp3")]
        public void write_tags_should_update_trackfile_size_and_modified(string filename)
        {
            Mocker.GetMock<IConfigService>()
                .Setup(x => x.ScrubAudioTags)
                .Returns(true);

            GivenFileCopy(filename);

            var file = GivenPopulatedTrackfile(0);

            file.Path = _copiedFile;
            Subject.WriteTags(file, false, true);

            var fileInfo = _diskProvider.GetFileInfo(file.Path);
            file.Modified.Should().Be(fileInfo.LastWriteTimeUtc);
            file.Size.Should().Be(fileInfo.Length);
        }
    }
}
