using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    public class DatabaseFixture : DbTest
    {
        [Test]
        public void SingleOrDefault_should_return_null_on_empty_db()
        {
            Mocker.Resolve<IDatabase>()
                .OpenConnection().Query<Author>("SELECT * FROM Authors")
                .SingleOrDefault(c => c.CleanName == "SomeTitle")
                .Should()
                .BeNull();
        }

        [Test]
        public void vacuum()
        {
            Mocker.Resolve<IDatabase>().Vacuum();
        }

        [Test]
        public void get_version()
        {
            Mocker.Resolve<IDatabase>().Version.Should().BeGreaterThan(new Version("3.0.0"));
        }
    }
}
