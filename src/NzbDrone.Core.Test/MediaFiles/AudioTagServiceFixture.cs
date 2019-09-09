using System.IO;
using NUnit.Framework;
using FluentAssertions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Configuration;
using FizzWare.NBuilder;
using System;
using System.Collections;
using System.Linq;
using NzbDrone.Common.Extensions;
using System.Collections.Generic;
using NzbDrone.Test.Common;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Test.MediaFiles.AudioTagServiceFixture
{
    [TestFixture]
    public class AudioTagServiceFixture : CoreTest<AudioTagService>
    {
        public static class TestCaseFactory
        {
            private static readonly string[] MediaFiles = new [] { "nin.mp2", "nin.mp3", "nin.flac", "nin.m4a", "nin.wma", "nin.ape", "nin.opus" };

            private static readonly string[] SkipProperties = new [] { "IsValid", "Duration", "Quality", "MediaInfo", "ImageFile" };
            private static readonly Dictionary<string, string[]> SkipPropertiesByFile = new Dictionary<string, string[]> {
                { "nin.mp2", new [] {"OriginalReleaseDate"} }
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

        private readonly string testdir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media");
        private string copiedFile;
        private AudioTag testTags;
        private IDiskProvider _diskProvider;
        
        [SetUp]
        public void Setup()
        {
            _diskProvider = Mocker.Resolve<IDiskProvider>("ActualDiskProvider");

            Mocker.SetConstant<IDiskProvider>(_diskProvider);
            
            Mocker.GetMock<IConfigService>()
                .Setup(x => x.WriteAudioTags)
                .Returns(WriteAudioTagsType.Sync);

            var imageFile = Path.Combine(testdir, "nin.png");
            var imageSize = _diskProvider.GetFileSize(imageFile);

            // have to manually set the arrays of string parameters and integers to values > 1
            testTags = Builder<AudioTag>.CreateNew()
                .With(x => x.Track = 2)
                .With(x => x.TrackCount = 33)
                .With(x => x.Disc = 44)
                .With(x => x.DiscCount = 55)
                .With(x => x.Date = new DateTime(2019, 3, 1))
                .With(x => x.Year = 2019)
                .With(x => x.OriginalReleaseDate = new DateTime(2009, 4, 1))
                .With(x => x.OriginalYear = 2009)
                .With(x => x.Performers = new [] { "Performer1" })
                .With(x => x.AlbumArtists = new [] { "방탄소년단" })
                .With(x => x.Genres = new [] { "Genre1", "Genre2" })
                .With(x => x.ImageFile = imageFile)
                .With(x => x.ImageSize = imageSize)
                .Build();
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(copiedFile))
            {
                File.Delete(copiedFile);
            }
        }

        private void GivenFileCopy(string filename)
        {
            var original = Path.Combine(testdir, filename);
            var tempname = $"temp_{Path.GetRandomFileName()}{Path.GetExtension(filename)}";
            copiedFile = Path.Combine(testdir, tempname);

            File.Copy(original, copiedFile);
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
                        var val1 = (IEnumerable) property.GetValue(a, null);
                        var val2 = (IEnumerable) property.GetValue(b, null);

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
                        var val1 = (IEnumerable) property.GetValue(a, null);
                        var val2 = (IEnumerable) property.GetValue(b, null);
                        
                        if (val1 != null || val2 != null)
                        {
                            val1.Should().BeEquivalentTo(val2, $"{property.Name} should be equal");
                        }
                    }
                }
            }
        }

        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_duration(string filename, string[] ignored)
        {
            var path = Path.Combine(testdir, filename);

            var tags = Subject.ReadTags(path);

            tags.Duration.Should().BeCloseTo(new TimeSpan(0, 0, 1, 25, 130), 100);
        }

        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_write_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = copiedFile;

            var initialtags = Subject.ReadAudioTag(path);

            VerifyDifferent(initialtags, testTags, skipProperties);

            testTags.Write(path);

            var writtentags = Subject.ReadAudioTag(path);

            VerifySame(writtentags, testTags, skipProperties);
        }

        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_remove_mb_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = copiedFile;

            var track = new TrackFile {
                Path = path
            };

            testTags.Write(path);

            var withmb = Subject.ReadAudioTag(path);

            VerifySame(withmb, testTags, skipProperties);

            Subject.RemoveMusicBrainzTags(track);

            var tag = Subject.ReadAudioTag(path);

            tag.MusicBrainzReleaseCountry.Should().BeNull();
            tag.MusicBrainzReleaseStatus.Should().BeNull();
            tag.MusicBrainzReleaseType.Should().BeNull();
            tag.MusicBrainzReleaseId.Should().BeNull();
            tag.MusicBrainzArtistId.Should().BeNull();
            tag.MusicBrainzReleaseArtistId.Should().BeNull();
            tag.MusicBrainzReleaseGroupId.Should().BeNull();
            tag.MusicBrainzTrackId.Should().BeNull();
            tag.MusicBrainzAlbumComment.Should().BeNull();
            tag.MusicBrainzReleaseTrackId.Should().BeNull();
        }
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_audiotag_from_file_with_no_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = copiedFile;

            Subject.RemoveAllTags(path);

            var tag = Subject.ReadAudioTag(path);
            var expected = new AudioTag() {
                Performers = new string[0],
                AlbumArtists = new string[0],
                Genres = new string[0]
            };

            VerifySame(tag, expected, skipProperties);
            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_parsedtrackinfo_from_file_with_no_tags(string filename, string[] skipProperties)
        {
            GivenFileCopy(filename);
            var path = copiedFile;

            Subject.RemoveAllTags(path);

            var tag = Subject.ReadTags(path);

            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
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
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_read_file_with_only_title_tag(string filename, string[] ignored)
        {
            GivenFileCopy(filename);
            var path = copiedFile;

            Subject.RemoveAllTags(path);
            
            var nametag = new AudioTag();
            nametag.Title = "test";
            nametag.Write(path);

            var tag = Subject.ReadTags(path);
            tag.Title.Should().Be("test");
            
            tag.Quality.Should().NotBeNull();
            tag.MediaInfo.Should().NotBeNull();
        }
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_remove_date_from_tags_when_not_in_metadata(string filename, string[] ignored)
        {
            GivenFileCopy(filename);
            var path = copiedFile;
            
            testTags.Write(path);
            
            testTags.Date = null;
            testTags.OriginalReleaseDate = null;
            
            testTags.Write(path);
            
            var onDisk = Subject.ReadAudioTag(path);
            
            onDisk.Date.HasValue.Should().BeFalse();
            onDisk.OriginalReleaseDate.HasValue.Should().BeFalse();
        }

        [Test]
        public void should_ignore_non_parsable_id3v23_date()
        {
            GivenFileCopy("nin.mp2");

            using(var file = TagLib.File.Create(copiedFile))
            {
                var id3tag = (TagLib.Id3v2.Tag) file.GetTag(TagLib.TagTypes.Id3v2);
                id3tag.SetTextFrame("TORY", "0");
                file.Save();
            }

            var tag = Subject.ReadAudioTag(copiedFile);
            tag.OriginalReleaseDate.HasValue.Should().BeFalse();
        }

        private TrackFile GivenPopulatedTrackfile(int mediumOffset)
        {
            var meta = Builder<ArtistMetadata>.CreateNew().Build();
            var artist = Builder<Artist>.CreateNew()
                .With(x => x.Metadata = meta)
                .Build();
            
            var album = Builder<Album>.CreateNew()
                .With(x => x.Artist = artist)
                .Build();

            var media = Builder<Medium>.CreateListOfSize(2).Build() as List<Medium>;
            media.ForEach(x => x.Number += mediumOffset);

            var release = Builder<AlbumRelease>.CreateNew()
                .With(x => x.Album = album)
                .With(x => x.Media = media)
                .With(x => x.Country = new List<string>())
                .With(x => x.Label = new List<string>())
                .Build();
            
            var tracks = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumRelease = release)
                .With(x => x.ArtistMetadata = meta)
                .TheFirst(5)
                .With(x => x.MediumNumber = 1 + mediumOffset)
                .TheNext(5)
                .With(x => x.MediumNumber = 2 + mediumOffset)
                .Build() as List<Track>;
            release.Tracks = tracks;

            var file = Builder<TrackFile>.CreateNew()
                .With(x => x.Tracks = new List<Track> { tracks[0] })
                .With(x => x.Artist = artist)
                .Build();

            return file;
        }

        [Test]
        public void get_metadata_should_not_fail_with_missing_country()
        {
            var file = GivenPopulatedTrackfile(0);
            var tag = Subject.GetTrackMetadata(file);
        
            tag.MusicBrainzReleaseCountry.Should().BeNull();
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

            file.Path = copiedFile;
            Subject.WriteTags(file, false, true);

            var fileInfo = _diskProvider.GetFileInfo(file.Path);
            file.Modified.Should().Be(fileInfo.LastWriteTimeUtc);
            file.Size.Should().Be(fileInfo.Length);
        }
    }
}
