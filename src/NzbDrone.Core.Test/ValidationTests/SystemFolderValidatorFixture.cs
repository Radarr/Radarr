using System;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Test.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Test.ValidationTests
{
    public class SystemFolderValidatorFixture : CoreTest<SystemFolderValidator>
    {
        private TestValidator<Artist> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<Artist>
                            {
                                v => v.RuleFor(s => s.Path).SetValidator(Subject)
                            };
        }

        [Test]
        public void should_not_be_valid_if_set_to_windows_folder()
        {
            WindowsOnly();

            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.Path = Environment.GetFolderPath(Environment.SpecialFolder.Windows))
                                        .Build();

            _validator.Validate(artist).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_windows_folder()
        {
            WindowsOnly();

            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Test"))
                                        .Build();

            _validator.Validate(artist).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_set_to_bin_folder()
        {
            MonoOnly();

            var bin = OsInfo.IsOsx ? "/System" : "/bin";
            var artist = Builder<Artist>.CreateNew()
                                        .With(s => s.Path = bin)
                                        .Build();

            _validator.Validate(artist).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_bin_folder()
        {
            MonoOnly();

            var bin = OsInfo.IsOsx ? "/System" : "/bin";
            var artist = Builder<Artist>.CreateNew()
                .With(s => s.Path = Path.Combine(bin, "test"))
                .Build();

            _validator.Validate(artist).IsValid.Should().BeFalse();
        }
    }
}
