using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]

    public class ProperSpecificationFixture : CoreTest<ProperSpecification>
    {
        private RemoteMovie _parseResultSingle;
        private MovieFile _firstFile;
        private MovieFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new MovieFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)), DateAdded = DateTime.Now };
            _secondFile = new MovieFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)), DateAdded = DateTime.Now };

            var fakeSeries = Builder<Movie>.CreateNew()
                         .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p.Id })
                         .With(c => c.MovieFile = _firstFile)
                         .Build();

            _parseResultSingle = new RemoteMovie
            {
                Movie = fakeSeries,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.SDTV);
        }

        private void GivenAutoDownloadPropers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.AutoDownloadPropers)
                  .Returns(true);
        }

        [Test]
        public void should_return_false_when_movieFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_movieFile_was_added_more_than_7_days_ago_but_proper_is_for_better_quality()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_episodeFile_was_added_more_than_7_days_ago_but_is_for_search()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new MovieSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_proper_but_auto_download_propers_is_false()
        {
            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_movieFile_was_added_today()
        {
            GivenAutoDownloadPropers();

            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}
