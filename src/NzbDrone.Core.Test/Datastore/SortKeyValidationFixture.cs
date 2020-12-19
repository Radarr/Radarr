using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class SortKeyValidationFixture : DbTest
    {
        [TestCase("amissingcolumn")]
        [TestCase("amissingtable.id")]
        [TestCase("table.table.column")]
        [TestCase("column; DROP TABLE Commands;--")]
        public void should_return_false_for_invalid_sort_key(string sortKey)
        {
            TableMapping.Mapper.IsValidSortKey(sortKey).Should().BeFalse();
        }

        //[TestCase("authors.sortName")] TODO: Figure out why Authors table properties don't get mapped
        [TestCase("Id")]
        [TestCase("id")]
        [TestCase("commands.id")]
        public void should_return_true_for_valid_sort_key(string sortKey)
        {
            TableMapping.Mapper.IsValidSortKey(sortKey).Should().BeTrue();
        }
    }
}
