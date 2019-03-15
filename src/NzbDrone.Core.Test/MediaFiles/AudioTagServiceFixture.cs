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

namespace NzbDrone.Core.Test.MediaFiles.AudioTagServiceFixture
{
    [TestFixture]
    public class AudioTagServiceFixture : CoreTest<AudioTagService>
    {
        public static class TestCaseFactory
        {
            private static readonly string[] MediaFiles = new [] { "nin.mp2", "nin.mp3", "nin.flac", "nin.m4a", "nin.wma", "nin.ape", "nin.opus" };

            private static readonly string[] SkipProperties = new [] { "IsValid", "Duration", "Quality", "MediaInfo" };
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
        
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(x => x.WriteAudioTags)
                .Returns(WriteAudioTagsType.Sync);

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
                        val1.Should().BeEquivalentTo(val2, $"{property.Name} should be equal");
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
                Artist = new Artist {
                    Path = Path.GetDirectoryName(path)
                },
                RelativePath = Path.GetFileName(path)
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
    }
}
