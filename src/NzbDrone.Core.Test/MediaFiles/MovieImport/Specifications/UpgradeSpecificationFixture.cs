using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Movie _movie;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _localMovie = new LocalMovie()
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                    Movie = _movie
                                };
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile()
        {
            _localMovie.Movie.MovieFile = null;
            _localMovie.Movie.MovieFileId = 0;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile()
        {

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                    new MovieFile
                    {
                        Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                    };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }


        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile()
        {
            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                new MovieFile
                {
                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }
    }
}
