using FluentValidation.Results;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateMovieFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateMovieFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardMovieFormat", ERROR_MESSAGE);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(sampleResult.FileName);

            if (parsedMovieInfo == null)
            {
                return validationFailure;
            }

            return null;
        }
    }
}
