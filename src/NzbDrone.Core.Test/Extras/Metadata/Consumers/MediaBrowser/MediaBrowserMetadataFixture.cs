namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.MediaBrowser
{
    using FizzWare.NBuilder;
    using FluentAssertions;
    using NUnit.Framework;
    using NzbDrone.Core.Extras.Metadata;
    using NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser;
    using NzbDrone.Core.Movies;
    using NzbDrone.Core.Test.Framework;
    using NzbDrone.Test.Common;

    [TestFixture]
    public class MediaBrowserMetadataFixture : CoreTest<MediaBrowserMetadata>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Movies\The.Movie".AsOsAgnostic())
                                     .Build();

            Subject.Definition = new MetadataDefinition
            {
                Settings = new MediaBrowserMetadataSettings()
            };
        }

        private MediaBrowserMetadataSettings Settings => (MediaBrowserMetadataSettings)Subject.Definition.Settings;

        [Test]
        public void should_always_support_metadata_without_video_file()
        {
            Subject.SupportsMetadataWithoutVideoFile.Should().BeTrue();
        }

        [Test]
        public void MovieMetadata_should_return_result_when_movieFile_is_null()
        {
            Settings.MovieMetadata = true;

            var result = Subject.MovieMetadata(_movie, null);

            result.Should().NotBeNull();
            result.RelativePath.Should().Be("movie.xml");
        }
    }
}
