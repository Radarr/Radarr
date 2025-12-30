using FluentValidation;

namespace NzbDrone.Core.BookSeries
{
    public interface IAddBookSeriesValidator : IValidator<BookSeries>
    {
    }

    public class AddBookSeriesValidator : AbstractValidator<BookSeries>, IAddBookSeriesValidator
    {
        public AddBookSeriesValidator()
        {
            RuleFor(c => c.Title).NotEmpty();
        }
    }
}
