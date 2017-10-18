using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Datastore.PagingSpecExtensionsTests
{
    public class ToSortDirectionFixture
    {
        [Test]
        public void should_convert_default_to_asc()
        {
            var pagingSpec = new PagingSpec<Album>
            {
                Page = 1,
                PageSize = 10,
                SortDirection = SortDirection.Default,
                SortKey = "ReleaseDate"
            };

            pagingSpec.ToSortDirection().Should().Be(Marr.Data.QGen.SortDirection.Asc);
        }

        [Test]
        public void should_convert_ascending_to_asc()
        {
            var pagingSpec = new PagingSpec<Album>
                {
                    Page = 1,
                    PageSize = 10,
                    SortDirection = SortDirection.Ascending,
                    SortKey = "ReleaseDate"
            };

            pagingSpec.ToSortDirection().Should().Be(Marr.Data.QGen.SortDirection.Asc);
        }

        [Test]
        public void should_convert_descending_to_desc()
        {
            var pagingSpec = new PagingSpec<Album>
            {
                Page = 1,
                PageSize = 10,
                SortDirection = SortDirection.Descending,
                SortKey = "ReleaseDate"
            };

            pagingSpec.ToSortDirection().Should().Be(Marr.Data.QGen.SortDirection.Desc);
        }
    }
}
