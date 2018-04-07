using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Wdtv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Wdtv
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<WdtvMetadata>
    {
        private Artist _artist;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Music\The.Artist".AsOsAgnostic())
                                     .Build();
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_artist.Path, "file.jpg");

            Subject.FindMetadataFile(_artist, path).Should().BeNull();
        }

        [TestCase(".xml", MetadataType.TrackMetadata)]
        public void should_return_metadata_for_track_if_valid_file_for_track(string extension, MetadataType type)
        {
            var path = Path.Combine(_artist.Path, "the.artist.s01e01.track" + extension);

            Subject.FindMetadataFile(_artist, path).Type.Should().Be(type);
        }

        [Ignore("Need Updated")]
        [TestCase(".xml")]
        [TestCase(".metathumb")]
        public void should_return_null_if_not_valid_file_for_track(string extension)
        {
            var path = Path.Combine(_artist.Path, "the.artist.track" + extension);

            Subject.FindMetadataFile(_artist, path).Should().BeNull();
        }
    }
}
