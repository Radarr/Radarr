using System;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ValidationTests
{
    public class SystemFolderValidatorFixture : CoreTest<SystemFolderValidator>
    {
        private TestValidator<Movie> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<Movie>
                            {
                                v => v.RuleFor(s => s.Path).SetValidator(Subject)
                            };
        }

        [Test]
        public void should_not_be_valid_if_set_to_windows_folder()
        {
            WindowsOnly();

            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.Path = Environment.GetFolderPath(Environment.SpecialFolder.Windows))
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_windows_folder()
        {
            WindowsOnly();

            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Test"))
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_set_to_bin_folder()
        {
            MonoOnly();

            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.Path = "/bin")
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_bin_folder()
        {
            MonoOnly();

            var movie = Builder<Movie>.CreateNew()
                                        .With(s => s.Path = "/bin/test")
                                        .Build();

            _validator.Validate(movie).IsValid.Should().BeFalse();
        }
    }
}
