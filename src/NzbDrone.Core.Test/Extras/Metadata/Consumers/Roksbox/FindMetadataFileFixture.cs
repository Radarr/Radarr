using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Roksbox;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Roksbox
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<RoksboxMetadata>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Movies\The.Movie.2011".AsOsAgnostic())
                                     .Build();
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_movie.Path, "file.jpg");

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [TestCase(".xml", MetadataType.MovieMetadata)]
        [TestCase(".jpg", MetadataType.MovieImage)]
        public void should_return_metadata_for_movie_if_valid_file_for_movie(string extension, MetadataType type)
        {
            var path = Path.Combine(_movie.Path, "the.movie.2011" + extension);

            Subject.FindMetadataFile(_movie, path).Type.Should().Be(type);
        }

        [TestCase(".xml")]
        [TestCase(".jpg")]
        public void should_return_null_if_not_valid_file_for_movie(string extension)
        {
            var path = Path.Combine(_movie.Path, "the.movie.here" + extension);

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [Test]
        public void should_not_return_metadata_if_image_file_is_a_thumb()
        {
            var path = Path.Combine(_movie.Path, "the.movie.2011-thumb.jpg");

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [Test]
        public void should_return_movie_image_for_folder_jpg_in_movie_folder()
        {
            var path = Path.Combine(_movie.Path, new DirectoryInfo(_movie.Path).Name + ".jpg");

            Subject.FindMetadataFile(_movie, path).Type.Should().Be(MetadataType.MovieImage);
        }
    }
}
