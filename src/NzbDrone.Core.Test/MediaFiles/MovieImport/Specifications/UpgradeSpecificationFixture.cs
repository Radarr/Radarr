using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

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
                                     .With(e => e.QualityProfiles = new List<Profile> { new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() } })
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
            _localMovie.Movie.MovieFiles = new List<MovieFile> { };

            Subject.IsSatisfiedBy(_localMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile()
        {
            var movieFile =
                    new MovieFile
                    {
                        Id = 1,
                        Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                    };

            _localMovie.Movie.MovieFiles = new List<MovieFile> { movieFile };

            Subject.IsSatisfiedBy(_localMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile()
        {
            var movieFile =
                new MovieFile
                {
                    Id = 1,
                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                };

            _localMovie.Movie.MovieFiles = new List<MovieFile> { movieFile };

            Subject.IsSatisfiedBy(_localMovie, null).Should().OnlyContain(x => x.Accepted == false);
        }
    }
}
