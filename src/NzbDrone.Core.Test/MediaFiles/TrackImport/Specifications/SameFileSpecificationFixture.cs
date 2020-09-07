using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.BookImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Specifications
{
    [TestFixture]
    public class SameFileSpecificationFixture : CoreTest<SameFileSpecification>
    {
        private LocalBook _localTrack;

        [SetUp]
        public void Setup()
        {
            _localTrack = Builder<LocalBook>.CreateNew()
                                                 .With(l => l.Size = 150.Megabytes())
                                                 .Build();
        }

        [Test]
        public void should_be_accepted_if_no_existing_file()
        {
            _localTrack.Book = Builder<Book>.CreateNew()
                .Build();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        /*
        [Test]
        public void should_be_accepted_if_multiple_existing_files()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.TrackFileId = 1)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Size = _localTrack.Size
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.TrackFileId = 2)
                                                     .With(e => e.TrackFile = new LazyLoaded<TrackFile>(
                                                                                new TrackFile
                                                                                {
                                                                                    Size = _localTrack.Size
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }*/

        [Test]
        public void should_be_accepted_if_file_size_is_different()
        {
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(
                          new List<BookFile>
                          {
                              new BookFile
                              {
                                  Size = _localTrack.Size + 100.Megabytes()
                              }
                          }))
                .Build();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_reject_if_file_size_is_the_same()
        {
            _localTrack.Book = Builder<Book>.CreateNew()
                .With(e => e.BookFiles = new LazyLoaded<List<BookFile>>(
                          new List<BookFile>
                          {
                              new BookFile
                              {
                                  Size = _localTrack.Size
                              }
                          }))
                .Build();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_cannot_be_fetched()
        {
            _localTrack.Tracks = Builder<Track>.CreateListOfSize(1)
                .TheFirst(1)
                .With(e => e.TrackFileId = 1)
                .With(e => e.TrackFile = new LazyLoaded<TrackFile>((TrackFile)null))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }
    }
}
