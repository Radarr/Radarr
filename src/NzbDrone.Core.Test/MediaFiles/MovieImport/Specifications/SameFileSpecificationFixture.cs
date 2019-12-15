using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class SameFileSpecificationFixture : CoreTest<SameFileSpecification>
    {
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Size = 150.Megabytes())
                                                 .Build();
        }

        [Test]
        public void should_be_accepted_if_no_existing_file()
        {
            _localMovie.Movie = Builder<Movie>.CreateNew()
                                                     .With(e => e.MovieFileId = 0)
                                                     .Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_size_is_different()
        {
            _localMovie.Movie = Builder<Movie>.CreateNew()
                .With(e => e.MovieFileId = 1)
                .With(e => e.MovieFile =
                    new MovieFile
                    {
                        Size = _localMovie.Size + 100.Megabytes()
                    })
                .Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_reject_if_file_size_is_the_same()
        {
            _localMovie.Movie = Builder<Movie>.CreateNew()
                .With(e => e.MovieFileId = 1)
                .With(e => e.MovieFile =
                    new MovieFile
                    {
                        Size = _localMovie.Size
                    })
                .Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_cannot_be_fetched()
        {
            _localMovie.Movie = Builder<Movie>.CreateNew()
                .With(e => e.MovieFileId = 1)
                .With(e => e.MovieFile = null)
                .Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }
    }
}
