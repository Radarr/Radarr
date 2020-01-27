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
            var validationFailure = new ValidationFailure("MovieFormat", ERROR_MESSAGE);
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(sampleResult.FileName, false); //We are not lenient when testing naming schemes

            if (parsedMovieInfo == null)
            {
                return validationFailure;
            }

            return null;
        }
    }
}
