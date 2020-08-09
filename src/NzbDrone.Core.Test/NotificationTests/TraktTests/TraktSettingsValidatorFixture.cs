using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.TraktTests
{
    [TestFixture]
    public class TraktSettingsValidatorFixture : CoreTest<TraktSettingsValidator>
    {
        private TraktSettings _traktSettings;
        private TestValidator<TraktSettings> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<TraktSettings>
                            {
                                v => v.RuleFor(s => s).SetValidator(Subject)
                            };

            _traktSettings = Builder<TraktSettings>.CreateNew()
                                        .With(s => s.AccessToken = "sometoken")
                                        .With(s => s.RefreshToken = "sometoken")
                                        .With(s => s.Expires = DateTime.Now.AddDays(2))
                                        .Build();
        }

        [Test]
        public void should_be_valid_if_all_settings_valid()
        {
            _validator.Validate(_traktSettings).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_not_be_valid_if_port_is_out_of_range()
        {
            _traktSettings.AccessToken = "";

            _validator.Validate(_traktSettings).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_server_is_empty()
        {
            _traktSettings.RefreshToken = "";

            _validator.Validate(_traktSettings).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_from_is_empty()
        {
            _traktSettings.Expires = default(DateTime);

            _validator.Validate(_traktSettings).IsValid.Should().BeFalse();
        }
    }
}
