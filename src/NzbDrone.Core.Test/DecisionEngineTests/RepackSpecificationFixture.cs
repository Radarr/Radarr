using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedBookInfo _parsedBookInfo;
        private List<Book> _albums;
        private List<BookFile> _trackFiles;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _parsedBookInfo = Builder<ParsedBookInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.FLAC,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Readarr")
                                                           .Build();

            _albums = Builder<Book>.CreateListOfSize(1)
                                        .All()
                                        .BuildList();

            _trackFiles = Builder<BookFile>.CreateListOfSize(3)
                                            .All()
                                            .With(t => t.EditionId = _albums.First().Id)
                                            .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByBook(It.IsAny<int>()))
                  .Returns(_trackFiles);
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_there_are_is_no_track_files()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByBook(It.IsAny<int>()))
                  .Returns(new List<BookFile>());

            _parsedBookInfo.Quality.Revision.IsRepack = true;

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_a_different_quality()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Readarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.MP3_320);
                return c;
            }).ToList();

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_all_existing_files()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Readarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_some_but_not_all_trackfiles()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Readarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            _trackFiles.First().ReleaseGroup = "NotReadarr";

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_different_group()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "NotReadarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_existing_file_is_unknown()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_release_is_unknown()
        {
            _parsedBookInfo.Quality.Revision.IsRepack = true;
            _parsedBookInfo.ReleaseGroup = null;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Readarr";
                return c;
            }).ToList();

            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteBook = Builder<RemoteBook>.CreateNew()
                                                      .With(e => e.ParsedBookInfo = _parsedBookInfo)
                                                      .With(e => e.Books = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteBook, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }
    }
}
