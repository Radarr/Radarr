using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class IndexerSpecificationValidator : AbstractValidator<IndexerSpecification>
    {
        public IndexerSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class IndexerSpecification : CustomFormatSpecificationBase
    {
        private static readonly IndexerSpecificationValidator Validator = new IndexerSpecificationValidator();

        public override int Order => 4;
        public override string ImplementationName => "Indexer";

        [FieldDefinition(1, Label = "Indexer", Type = FieldType.Select, SelectOptionsProviderAction = "indexers")]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.IndexerId == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
