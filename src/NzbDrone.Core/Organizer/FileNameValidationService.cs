using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateStandardFilename(SampleResult sampleResult);
        ValidationFailure ValidateDailyFilename(SampleResult sampleResult);
        ValidationFailure ValidateAnimeFilename(SampleResult sampleResult);
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

        public ValidationFailure ValidateStandardFilename(SampleResult sampleResult)
        {
           

            return null;
        }

        public ValidationFailure ValidateDailyFilename(SampleResult sampleResult)
        {
            

            return null;
        }

        public ValidationFailure ValidateAnimeFilename(SampleResult sampleResult)
        {
            

            return null;
        }

        private bool ValidateSeasonAndEpisodeNumbers(List<Episode> episodes, ParsedEpisodeInfo parsedEpisodeInfo)
        {
            

            return true;
        }
    }
}
