using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddArtistFixture : CoreTest<AddArtistService>
    {
        private Artist _fakeArtist;

        [SetUp]
        public void Setup()
        {
            _fakeArtist = Builder<Artist>
                .CreateNew()
                .With(s => s.Path = null)
                .Build();
            _fakeArtist.Albums = new List<Album>();
        }

        private void GivenValidArtist(string lidarrId)
        {
            Mocker.GetMock<IProvideArtistInfo>()
                .Setup(s => s.GetArtistInfo(lidarrId, It.IsAny<int>()))
                .Returns(_fakeArtist);
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                  .Returns<Artist, NamingConfig>((c, n) => c.Name);

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Artist>()))
                  .Returns(new ValidationResult());
        }

        [Test]
        public void should_be_able_to_add_a_artist_without_passing_in_name()
        {
            var newArtist = new Artist
            {
                ForeignArtistId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                RootFolderPath = @"C:\Test\Music"
            };

            GivenValidArtist(newArtist.ForeignArtistId);
            GivenValidPath();

            var artist = Subject.AddArtist(newArtist);

            artist.Name.Should().Be(_fakeArtist.Name);
        }

        [Test]
        public void should_have_proper_path()
        {
            var newArtist = new Artist
                            {
                                ForeignArtistId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                                RootFolderPath = @"C:\Test\Music"
                            };

            GivenValidArtist(newArtist.ForeignArtistId);
            GivenValidPath();

            var artist = Subject.AddArtist(newArtist);

            artist.Path.Should().Be(Path.Combine(newArtist.RootFolderPath, _fakeArtist.Name));
        }

        [Test]
        public void should_throw_if_artist_validation_fails()
        {
            var newArtist = new Artist
            {
                ForeignArtistId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            GivenValidArtist(newArtist.ForeignArtistId);

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Artist>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddArtist(newArtist));
        }

        [Test]
        public void should_throw_if_artist_cannot_be_found()
        {
            var newArtist = new Artist
            {
                ForeignArtistId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            Mocker.GetMock<IProvideArtistInfo>()
                  .Setup(s => s.GetArtistInfo(newArtist.ForeignArtistId, newArtist.MetadataProfileId))
                  .Throws(new ArtistNotFoundException(newArtist.ForeignArtistId));

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Artist>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddArtist(newArtist));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
