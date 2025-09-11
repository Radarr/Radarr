using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class DownloadProtocolSpecificationValidator : AbstractValidator<DownloadProtocolSpecification>
    {
        public DownloadProtocolSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class DownloadProtocolSpecification : CustomFormatSpecificationBase
    {
        private static readonly DownloadProtocolSpecificationValidator Validator = new DownloadProtocolSpecificationValidator();

        public override int Order => 11;
        public override string ImplementationName => "Download Protocol";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationDownloadProtocol", Type = FieldType.Select, SelectOptions = typeof(DownloadProtocol))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.DownloadProtocol == (DownloadProtocol)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
