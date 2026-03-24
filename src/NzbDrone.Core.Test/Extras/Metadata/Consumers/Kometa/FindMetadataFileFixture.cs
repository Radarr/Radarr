using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Kometa;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Kometa
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<KometaMetadata>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(s => s.Path = @"C:\Test\Movies\Movie.Title.2024".AsOsAgnostic())
                .Build();
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_movie.Path, "file.jpg");

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [TestCase(".jpg")]
        public void should_return_null_if_not_valid_file_for_movie(string extension)
        {
            var path = Path.Combine(_movie.Path, "movie.title.2024" + extension);

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [Test]
        public void should_not_return_metadata_if_image_file_is_a_thumb()
        {
            var path = Path.Combine(_movie.Path, "movie.title.2024-thumb.jpg");

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [TestCase("poster.jpg")]
        [TestCase("background.jpg")]
        public void should_return_movie_image_for_images_in_movie_folder(string filename)
        {
            var path = Path.Combine(_movie.Path, filename);

            Subject.FindMetadataFile(_movie, path).Type.Should().Be(MetadataType.MovieImage);
        }
    }
}
