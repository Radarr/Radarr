using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ValidationTests
{
    public class GuidValidationFixture : CoreTest<GuidValidator>
    {
        private TestValidator<ImportListExclusion> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<ImportListExclusion>
                            {
                                v => v.RuleFor(s => s.ForeignId).SetValidator(Subject)
                            };
        }

        [Test]
        public void should_not_be_valid_if_invalid_guid()
        {
            var listExclusion = Builder<ImportListExclusion>.CreateNew()
                                        .With(s => s.ForeignId = "e1f1e33e-2e4c-4d43-b91b-7064068d328")
                                        .Build();

            _validator.Validate(listExclusion).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_be_valid_if_valid_guid()
        {
            var listExclusion = Builder<ImportListExclusion>.CreateNew()
                                        .With(s => s.ForeignId = "e1f1e33e-2e4c-4d43-b91b-7064068d3283")
                                        .Build();

            _validator.Validate(listExclusion).IsValid.Should().BeTrue();
        }
    }
}
