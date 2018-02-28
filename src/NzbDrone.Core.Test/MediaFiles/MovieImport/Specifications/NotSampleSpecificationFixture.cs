using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class NotSampleSpecificationFixture : CoreTest<NotSampleSpecification>
    {
        private Movie _movie;
        private LocalMovie _localEpisode;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .Build();

            _localEpisode = new LocalMovie
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Movie = _movie,
                                    Quality = new QualityModel(Quality.HDTV720p)
                                };
        }

        [Test]
        public void should_return_true_for_existing_file()
        {
            _localEpisode.ExistingFile = true;
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
