using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Xbmc;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Xbmc
{
    [TestFixture]
    public class XbmcMetadataFixture : CoreTest<XbmcMetadata>
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
                Settings = new XbmcMetadataSettings()
            };
        }

        private XbmcMetadataSettings Settings => (XbmcMetadataSettings)Subject.Definition.Settings;

        [Test]
        public void should_support_metadata_without_video_file_when_UseMovieNfo_is_true()
        {
            Settings.UseMovieNfo = true;
            Subject.SupportsMetadataWithoutVideoFile.Should().BeTrue();
        }

        [Test]
        public void should_not_support_metadata_without_video_file_when_UseMovieNfo_is_false()
        {
            Settings.UseMovieNfo = false;
            Subject.SupportsMetadataWithoutVideoFile.Should().BeFalse();
        }

        [Test]
        public void MovieMetadata_should_return_result_when_movieFile_is_null_and_UseMovieNfo_is_true()
        {
            Settings.UseMovieNfo = true;
            Settings.MovieMetadata = true;

            Mocker.GetMock<ICreditService>()
                  .Setup(v => v.GetAllCreditsForMovieMetadata(It.IsAny<int>()))
                  .Returns(new List<Credit>());

            var result = Subject.MovieMetadata(_movie, null);

            result.Should().NotBeNull();
            result.RelativePath.Should().Be("movie.nfo");
        }

        [Test]
        public void MovieMetadata_should_throw_ArgumentException_when_movieFile_is_null_and_UseMovieNfo_is_false()
        {
            Settings.UseMovieNfo = false;
            Settings.MovieMetadata = true;

            Assert.Throws<ArgumentException>(() => Subject.MovieMetadata(_movie, null));
        }
    }
}
