using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    [TestFixture]
    public class AuthorSearchServiceFixture : CoreTest<AuthorSearchService>
    {
        private Author _author;

        [SetUp]
        public void Setup()
        {
            _author = new Author();

            Mocker.GetMock<IAuthorService>()
                .Setup(s => s.GetAuthor(It.IsAny<int>()))
                .Returns(_author);

            Mocker.GetMock<ISearchForNzb>()
                .Setup(s => s.AuthorSearch(_author.Id, false, true, false))
                .Returns(new List<DownloadDecision>());

            Mocker.GetMock<IProcessDownloadDecisions>()
                .Setup(s => s.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(), new List<DownloadDecision>()));
        }

        [Test]
        public void should_only_include_monitored_books()
        {
            _author.Books = new List<Book>
            {
                new Book { Monitored = false },
                new Book { Monitored = true }
            };

            Subject.Execute(new AuthorSearchCommand { AuthorId = _author.Id, Trigger = CommandTrigger.Manual });

            Mocker.GetMock<ISearchForNzb>()
                .Verify(v => v.AuthorSearch(_author.Id, false, true, false),
                    Times.Exactly(_author.Books.Value.Count(s => s.Monitored)));
        }
    }
}
