using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Movies;

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

            if(parsedMovieInfo == null)
            {
                return validationFailure;
            }

            return null;
        }
    }
}
