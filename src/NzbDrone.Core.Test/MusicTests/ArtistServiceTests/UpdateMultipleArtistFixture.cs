using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests.AuthorServiceTests
{
    [TestFixture]
    public class UpdateMultipleAuthorFixture : CoreTest<AuthorService>
    {
        private List<Author> _authors;

        [SetUp]
        public void Setup()
        {
            _authors = Builder<Author>.CreateListOfSize(5)
                .All()
                .With(s => s.QualityProfileId = 1)
                .With(s => s.Monitored)
                .With(s => s.Path = @"C:\Test\name".AsOsAgnostic())
                .With(s => s.RootFolderPath = "")
                .Build().ToList();
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateAuthors(_authors, false);

            Mocker.GetMock<IAuthorRepository>().Verify(v => v.UpdateMany(_authors), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns<Author, NamingConfig>((c, n) => c.Name);

            var newRoot = @"C:\Test\Music2".AsOsAgnostic();
            _authors.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildAuthorPaths>()
                .Setup(s => s.BuildPath(It.IsAny<Author>(), false))
                .Returns<Author, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Name));

            Subject.UpdateAuthors(_authors, false).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_when_rootFolderPath_is_empty()
        {
            Subject.UpdateAuthors(_authors, false).ForEach(s =>
            {
                var expectedPath = _authors.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });
        }

        [Test]
        public void should_be_able_to_update_many_author()
        {
            var author = Builder<Author>.CreateListOfSize(50)
                                        .All()
                                        .With(s => s.Path = (@"C:\Test\Music\" + s.Path).AsOsAgnostic())
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns<Author, NamingConfig>((c, n) => c.Name);

            var newRoot = @"C:\Test\Music2".AsOsAgnostic();
            author.ForEach(s => s.RootFolderPath = newRoot);

            Subject.UpdateAuthors(author, false);
        }
    }
}
