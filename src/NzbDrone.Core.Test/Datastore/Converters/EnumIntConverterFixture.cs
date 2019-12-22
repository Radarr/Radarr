using System;
using System.Reflection;
using FluentAssertions;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Download.Pending;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class EnumIntConverterFixture : CoreTest<Core.Datastore.Converters.EnumIntConverter>
    {
        [Test]
        public void should_return_int_when_saving_enum_to_db()
        {
            Subject.ToDB(PendingReleaseReason.Delay).Should().Be((int)PendingReleaseReason.Delay);
        }

        [Test]
        public void should_return_db_null_for_null_value_when_saving_to_db()
        {
            Subject.ToDB(null).Should().Be(DBNull.Value);
        }

        [Test]
        public void should_return_enum_when_getting_int_from_db()
        {
            var mockMemberInfo = new Mock<MemberInfo>();
            mockMemberInfo.SetupGet(s => s.DeclaringType).Returns(typeof(PendingRelease));
            mockMemberInfo.SetupGet(s => s.Name).Returns("Reason");

            var expected = PendingReleaseReason.Delay;

            var context = new ConverterContext
                          {
                              ColumnMap = new ColumnMap(mockMemberInfo.Object) { FieldType = typeof(PendingReleaseReason) },
                              DbValue = (long)expected
                          };

            Subject.FromDB(context).Should().Be(expected);
        }

        [Test]
        public void should_return_null_for_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(null);
        }
    }
}
