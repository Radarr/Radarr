using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DeletedTrackFileSpecificationFixture : CoreTest<DeletedBookFileSpecification>
    {
        private RemoteBook _parseResultMulti;
        private RemoteBook _parseResultSingle;
        private BookFile _firstFile;
        private BookFile _secondFile;

        [SetUp]
        public void Setup()
        {
            _firstFile =
                new BookFile
                {
                    Id = 1,
                    Path = "/My.Artist.S01E01.mp3",
                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)),
                    DateAdded = DateTime.Now,
                    BookId = 1
                };
            _secondFile =
                new BookFile
                {
                    Id = 2,
                    Path = "/My.Artist.S01E02.mp3",
                    Quality = new QualityModel(Quality.FLAC, new Revision(version: 1)),
                    DateAdded = DateTime.Now,
                    BookId = 2
                };

            var singleAlbumList = new List<Book> { new Book { Id = 1 } };
            var doubleAlbumList = new List<Book>
            {
                new Book { Id = 1 },
                new Book { Id = 2 }
            };

            var fakeArtist = Builder<Author>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.FLAC.Id })
                         .With(c => c.Path = @"C:\Music\My.Artist".AsOsAgnostic())
                         .Build();

            _parseResultMulti = new RemoteBook
            {
                Author = fakeArtist,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Books = doubleAlbumList
            };

            _parseResultSingle = new RemoteBook
            {
                Author = fakeArtist,
                ParsedBookInfo = new ParsedBookInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                Books = singleAlbumList
            };

            GivenUnmonitorDeletedTracks(true);
        }

        private void GivenUnmonitorDeletedTracks(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.AutoUnmonitorPreviouslyDownloadedTracks)
                  .Returns(enabled);
        }

        private void SetupMediaFile(List<BookFile> files)
        {
            Mocker.GetMock<IMediaFileService>()
                              .Setup(v => v.GetFilesByBook(It.IsAny<int>()))
                              .Returns(files);
        }

        private void WithExistingFile(BookFile trackFile)
        {
            var path = trackFile.Path;

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_return_true_when_unmonitor_deleted_tracks_is_off()
        {
            GivenUnmonitorDeletedTracks(false);

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_searching()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new AuthorSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_file_exists()
        {
            WithExistingFile(_firstFile);
            SetupMediaFile(new List<BookFile> { _firstFile });

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_is_missing()
        {
            SetupMediaFile(new List<BookFile> { _firstFile });
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_both_of_multiple_episode_exist()
        {
            WithExistingFile(_firstFile);
            WithExistingFile(_secondFile);
            SetupMediaFile(new List<BookFile> { _firstFile, _secondFile });

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_of_multiple_episode_is_missing()
        {
            WithExistingFile(_firstFile);
            SetupMediaFile(new List<BookFile> { _firstFile, _secondFile });

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }
    }
}
