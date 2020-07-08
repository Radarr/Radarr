using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Localization
{
    [TestFixture]
    public class LocalizationServiceFixture : CoreTest<LocalizationService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().Setup(m => m.MovieInfoLanguage).Returns((int)Language.English);

            Mocker.GetMock<IAppFolderInfo>().Setup(m => m.StartUpFolder).Returns(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void should_get_string_in_dictionary_if_lang_exists_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("BackupNow");

            localizedString.Should().Be("Backup Now");
        }

        [Test]
        public void should_get_string_in_default_dictionary_if_no_lang_exists_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("BackupNow", "an");

            localizedString.Should().Be("Backup Now");

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_get_string_in_default_dictionary_if_lang_empty_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("BackupNow", "");

            localizedString.Should().Be("Backup Now");
        }

        [Test]
        public void should_return_argument_if_string_doesnt_exists()
        {
            var localizedString = Subject.GetLocalizedString("BadString", "en");

            localizedString.Should().Be("BadString");
        }

        [Test]
        public void should_return_argument_if_string_doesnt_exists_default_lang()
        {
            var localizedString = Subject.GetLocalizedString("BadString");

            localizedString.Should().Be("BadString");
        }

        [Test]
        public void should_throw_if_empty_string_passed()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetLocalizedString(""));
        }

        [Test]
        public void should_throw_if_null_string_passed()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetLocalizedString(null));
        }
    }
}
