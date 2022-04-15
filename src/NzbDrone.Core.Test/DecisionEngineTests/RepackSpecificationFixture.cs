using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedMovieInfo _parsedMovieInfo;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _parsedMovieInfo = Builder<ParsedMovieInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.SDTV,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Radarr")
                                                           .Build();

            _movie = Builder<Movie>.CreateNew()
                                        .Build();
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_true_if_there_are_is_no_movie_file()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            _movie.MovieFiles = new List<MovieFile> { };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_a_different_quality()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;
            var moviefile = Builder<MovieFile>.CreateNew()
                                              .With(e => e.Quality = new QualityModel(Quality.DVD))
                                              .With(e => e.ReleaseGroup = "Radarr")
                                              .Build();

            _movie.MovieFiles = new List<MovieFile> { moviefile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_existing_file()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                                 .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                 .With(e => e.ReleaseGroup = "Radarr")
                                                 .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_a_different_file()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                                 .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                 .With(e => e.ReleaseGroup = "NotRadarr")
                                                 .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_false_if_release_group_for_existing_file_is_unknown()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                                 .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                 .With(e => e.ReleaseGroup = "")
                                                 .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_false_if_release_group_for_release_is_unknown()
        {
            _parsedMovieInfo.Quality.Revision.IsRepack = true;
            _parsedMovieInfo.ReleaseGroup = null;

            var movieFile = Builder<MovieFile>.CreateNew()
                                                 .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                                 .With(e => e.ReleaseGroup = "Radarr")
                                                 .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_false_when_repack_but_auto_download_repack_is_false()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotUpgrade);

            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                     .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                     .With(e => e.ReleaseGroup = "Radarr")
                                     .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_return_true_when_repack_but_auto_download_repack_is_true()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                     .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                     .With(e => e.ReleaseGroup = "Radarr")
                                     .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_return_true_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _parsedMovieInfo.Quality.Revision.IsRepack = true;

            var movieFile = Builder<MovieFile>.CreateNew()
                                     .With(e => e.Quality = new QualityModel(Quality.SDTV))
                                     .With(e => e.ReleaseGroup = "Radarr")
                                     .Build();

            _movie.MovieFiles = new List<MovieFile> { movieFile };

            var remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                      .With(e => e.ParsedMovieInfo = _parsedMovieInfo)
                                                      .With(e => e.Movie = _movie)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }
    }
}
