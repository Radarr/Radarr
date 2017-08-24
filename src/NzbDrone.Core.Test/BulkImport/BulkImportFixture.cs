using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Moq;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music.Events;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.BulkImport
{
    [TestFixture]
    public class BulkImportFixture : CoreTest<AddArtistService>
    {
        private List<Artist> fakeArtists;
        private FluentValidation.Results.ValidationResult fakeValidation;

        [SetUp]
        public void Setup()
        {
            fakeArtists = Builder<Artist>.CreateListOfSize(3).BuildList();
            fakeArtists.ForEach(m =>
            {
                m.Path = null;
                m.RootFolderPath = @"C:\Test\Music";
            });

            fakeValidation = Builder<FluentValidation.Results.ValidationResult>.CreateNew().Build();
        }
        [Test]
        public void artist_added_event_should_have_proper_path()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                  .Returns((Artist m, NamingConfig n) => m.Name);

            Mocker.GetMock<IAddArtistValidator>()
                .Setup(s => s.Validate(It.IsAny<Artist>()))
                .Returns(fakeValidation);

            var artists = Subject.AddArtists(fakeArtists);

            foreach (Artist artist in artists)
            {
                artist.Path.Should().NotBeNullOrEmpty();
            }
            
        }
    }
}