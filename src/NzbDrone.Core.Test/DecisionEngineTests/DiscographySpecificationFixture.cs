using System;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using FizzWare.NBuilder;
using System.Linq;
using FluentAssertions;
using NzbDrone.Core.Music;
using Moq;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class DiscographySpecificationFixture : CoreTest<DiscographySpecification>
    {
        private RemoteAlbum _remoteAlbum;

        [SetUp]
        public void Setup()
        {
            var artist = Builder<Artist>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteAlbum = new RemoteAlbum
            {
                ParsedAlbumInfo = new ParsedAlbumInfo
                {
                    Discography = true
                },
                Albums = Builder<Album>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.ArtistId = artist.Id)
                                           .BuildList(),
                Artist = artist,
                Release = new ReleaseInfo
                {
                    Title = "Artist.Discography.1978.2005.FLAC-RlsGrp"
                }
            };

            Mocker.GetMock<IAlbumService>().Setup(s => s.AlbumsBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), false))
                                             .Returns(new List<Album>());
        }

        [Test]
        public void should_return_true_if_is_not_a_discography()
        {
            _remoteAlbum.ParsedAlbumInfo.Discography = false;
            _remoteAlbum.Albums.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
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
            _remoteAlbum.Albums.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_an_album_does_not_have_an_release_date()
        {
            _remoteAlbum.Albums.Last().ReleaseDate = null;
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
