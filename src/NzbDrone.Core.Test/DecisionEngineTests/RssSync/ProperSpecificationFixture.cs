using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]

    public class ProperSpecificationFixture : CoreTest<ProperSpecification>
    {
        private RemoteBook _parseResultMulti;
        private RemoteBook _parseResultSingle;
        private BookFile _firstFile;
        private BookFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new BookFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)), DateAdded = DateTime.Now };
            _secondFile = new BookFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)), DateAdded = DateTime.Now };

            var singleBookList = new List<Book> { new Book { }, new Book { } };
            var doubleBookList = new List<Book> { new Book { }, new Book { }, new Book { } };

            var fakeAuthor = Builder<Author>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.FLAC.Id })
                         .Build();

            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.GetFilesByBook(It.IsAny<int>()))
                .Returns(new List<BookFile> { _firstFile, _secondFile });

            _parseResultMulti = new RemoteBook
            {
                Author = fakeAuthor,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MOBI, new Revision(version: 2)) },
                Books = doubleBookList
            };

            _parseResultSingle = new RemoteBook
            {
                Author = fakeAuthor,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MOBI, new Revision(version: 2)) },
                Books = singleBookList
            };
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.PDF);
        }

        [Test]
        public void should_return_false_when_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MOBI;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MOBI;
            _secondFile.Quality.Quality = Quality.MOBI;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_trackFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.MOBI;
            _secondFile.Quality.Quality = Quality.MOBI;

            _secondFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_more_than_7_days_ago_but_proper_is_for_better_quality()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_more_than_7_days_ago_but_is_for_search()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new BookSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_proper_but_auto_download_propers_is_false()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotUpgrade);

            _firstFile.Quality.Quality = Quality.MOBI;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_trackFile_was_added_today()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _firstFile.Quality.Quality = Quality.MOBI;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_propers_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _firstFile.Quality.Quality = Quality.MOBI;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}
