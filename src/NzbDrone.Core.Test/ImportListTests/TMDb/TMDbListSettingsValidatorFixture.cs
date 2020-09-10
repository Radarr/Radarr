using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.TMDb.List;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.TMDb
{
    public class TMDbListSettingsValidatorFixture : CoreTest
    {
        [TestCase("")]
        [TestCase("0")]
        [TestCase("ls12345678")]
        [TestCase(null)]
        public void invalid_listId_should_not_validate(string listId)
        {
            var setting = new TMDbListSettings
            {
                ListId = listId,
            };

            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "ListId");
        }

        [TestCase("1")]
        [TestCase("706123")]
        public void valid_listId_should_validate(string listId)
        {
            var setting = new TMDbListSettings
            {
                ListId = listId,
            };

            setting.Validate().IsValid.Should().BeTrue();
            setting.Validate().Errors.Should().NotContain(c => c.PropertyName == "ListId");
        }
    }
}
