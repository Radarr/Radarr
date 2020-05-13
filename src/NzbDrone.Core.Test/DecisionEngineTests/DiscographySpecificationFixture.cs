using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class DiscographySpecificationFixture : CoreTest<DiscographySpecification>
    {
        private RemoteBook _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            var artist = Builder<Author>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteAlbum = new RemoteBook
            {
                ParsedBookInfo = new ParsedBookInfo
                {
                    Discography = true
                },
                Books = Builder<Book>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.AuthorId = artist.Id)
                                           .BuildList(),
                Author = artist,
                Release = new ReleaseInfo
                {
                    Title = "Artist.Discography.1978.2005.FLAC-RlsGrp"
                }
            };

            Mocker.GetMock<IBookService>().Setup(s => s.BooksBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), false))
                                             .Returns(new List<Book>());
        }

        [Test]
        public void should_return_true_if_is_not_a_discography()
        {
            _remoteAlbum.ParsedBookInfo.Discography = false;
            _remoteAlbum.Books.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_albums_have_released()
        {
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_album_has_not_released()
        {
            _remoteAlbum.Books.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_an_album_does_not_have_an_release_date()
        {
            _remoteAlbum.Books.Last().ReleaseDate = null;
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
