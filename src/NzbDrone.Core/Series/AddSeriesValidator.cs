using FluentValidation;

namespace NzbDrone.Core.Series
{
    public interface IAddSeriesValidator : IValidator<Series>
    {
    }

    public class AddSeriesValidator : AbstractValidator<Series>, IAddSeriesValidator
    {
        public AddSeriesValidator()
        {
            RuleFor(c => c.Title).NotEmpty();
        }
    }
}
