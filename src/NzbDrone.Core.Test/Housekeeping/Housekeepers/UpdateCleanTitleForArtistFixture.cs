using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class UpdateCleanTitleForArtistFixture : CoreTest<UpdateCleanTitleForArtist>
    {
        [Test]
        public void should_update_clean_title()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.Name = "Full Name")
                                        .With(s => s.CleanName = "unclean")
                                        .Build();

            Mocker.GetMock<IArtistRepository>()
                 .Setup(s => s.All())
                 .Returns(new[] { artist });

            Subject.Clean();

            Mocker.GetMock<IArtistRepository>()
                .Verify(v => v.Update(It.Is<Artist>(s => s.CleanName == "fullname")), Times.Once());
        }

        [Test]
        public void should_not_update_unchanged_title()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.Name = "Full Name")
                                        .With(s => s.CleanName = "fullname")
                                        .Build();

            Mocker.GetMock<IArtistRepository>()
                 .Setup(s => s.All())
                 .Returns(new[] { artist });

            Subject.Clean();

            Mocker.GetMock<IArtistRepository>()
                .Verify(v => v.Update(It.Is<Artist>(s => s.CleanName == "fullname")), Times.Never());
        }
    }
}
