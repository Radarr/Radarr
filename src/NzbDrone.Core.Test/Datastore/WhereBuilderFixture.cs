using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class WhereBuilderFixture : CoreTest
    {
        private WhereBuilder _subject;

        [OneTimeSetUp]
        public void MapTables()
        {
            // Generate table mapping
            Mocker.Resolve<DbFactory>();
        }

        private WhereBuilder Where(Expression<Func<Author, bool>> filter)
        {
            return new WhereBuilder(filter, true, 0);
        }

        private WhereBuilder WhereMetadata(Expression<Func<AuthorMetadata, bool>> filter)
        {
            return new WhereBuilder(filter, true, 0);
        }

        [Test]
        public void where_equal_const()
        {
            _subject = Where(x => x.Id == 10);

            _subject.ToString().Should().Be($"(\"Authors\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(10);
        }

        [Test]
        public void where_equal_variable()
        {
            var id = 10;
            _subject = Where(x => x.Id == id);

            _subject.ToString().Should().Be($"(\"Authors\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(id);
        }

        [Test]
        public void where_equal_property()
        {
            var artist = new Author { Id = 10 };
            _subject = Where(x => x.Id == artist.Id);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"Authors\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(artist.Id);
        }

        [Test]
        public void where_equal_lazy_property()
        {
            _subject = Where(x => x.QualityProfile.Value.Id == 1);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"QualityProfiles\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(1);
        }

        [Test]
        public void where_throws_without_concrete_condition_if_requiresConcreteCondition()
        {
            Expression<Func<Author, Author, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilder(filter, true, 0);
            Assert.Throws<InvalidOperationException>(() => _subject.ToString());
        }

        [Test]
        public void where_allows_abstract_condition_if_not_requiresConcreteCondition()
        {
            Expression<Func<Author, Author, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilder(filter, false, 0);
            _subject.ToString().Should().Be($"(\"Authors\".\"Id\" = \"Authors\".\"Id\")");
        }

        [Test]
        public void where_string_is_null()
        {
            _subject = Where(x => x.CleanName == null);

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" IS NULL)");
        }

        [Test]
        public void where_string_is_null_value()
        {
            string imdb = null;
            _subject = Where(x => x.CleanName == imdb);

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" IS NULL)");
        }

        [Test]
        public void where_equal_null_property()
        {
            var artist = new Author { CleanName = null };
            _subject = Where(x => x.CleanName == artist.CleanName);

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" IS NULL)");
        }

        [Test]
        public void where_column_contains_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanName.Contains(test));

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" LIKE '%' || @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_string_contains_column()
        {
            var test = "small";
            _subject = Where(x => test.Contains(x.CleanName));

            _subject.ToString().Should().Be($"(@Clause1_P1 LIKE '%' || \"Authors\".\"CleanName\" || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_column_starts_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanName.StartsWith(test));

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" LIKE @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_column_ends_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanName.EndsWith(test));

            _subject.ToString().Should().Be($"(\"Authors\".\"CleanName\" LIKE '%' || @Clause1_P1)");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void where_in_list()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => list.Contains(x.Id));

            _subject.ToString().Should().Be($"(\"Authors\".\"Id\" IN (1, 2, 3))");

            _subject.Parameters.ParameterNames.Should().BeEmpty();
        }

        [Test]
        public void where_in_list_2()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => x.CleanName == "test" && list.Contains(x.Id));

            _subject.ToString().Should().Be($"((\"Authors\".\"CleanName\" = @Clause1_P1) AND (\"Authors\".\"Id\" IN (1, 2, 3)))");
        }

        [Test]
        public void enum_as_int()
        {
            _subject = WhereMetadata(x => x.Status == ArtistStatusType.Continuing);

            _subject.ToString().Should().Be($"(\"AuthorMetadata\".\"Status\" = @Clause1_P1)");
        }

        [Test]
        public void enum_in_list()
        {
            var allowed = new List<ArtistStatusType> { ArtistStatusType.Continuing, ArtistStatusType.Ended };
            _subject = WhereMetadata(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"AuthorMetadata\".\"Status\" IN @Clause1_P1)");
        }

        [Test]
        public void enum_in_array()
        {
            var allowed = new ArtistStatusType[] { ArtistStatusType.Continuing, ArtistStatusType.Ended };
            _subject = WhereMetadata(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"AuthorMetadata\".\"Status\" IN @Clause1_P1)");
        }
    }
}
