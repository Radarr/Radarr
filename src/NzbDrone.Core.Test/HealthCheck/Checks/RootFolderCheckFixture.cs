using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RootFolderCheckFixture : CoreTest<RootFolderCheck>
    {
        private void GivenMissingRootFolder()
        {
            var artist = Builder<Author>.CreateListOfSize(1)
                                        .Build()
                                        .ToList();

            var importList = Builder<ImportListDefinition>.CreateListOfSize(1)
                .Build()
                .ToList();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.AllAuthorPaths())
                  .Returns(artist.ToDictionary(x => x.Id, x => x.Path));

            Mocker.GetMock<IImportListFactory>()
                .Setup(s => s.All())
                .Returns(importList);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(artist.First().Path))
                  .Returns(@"C:\Music");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);
        }

        [Test]
        public void should_not_return_error_when_no_artist()
        {
            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.AllAuthorPaths())
                  .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IImportListFactory>()
                .Setup(s => s.All())
                .Returns(new List<ImportListDefinition>());

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_if_artist_parent_is_missing()
        {
            GivenMissingRootFolder();

            Subject.Check().ShouldBeError();
        }
    }
}
