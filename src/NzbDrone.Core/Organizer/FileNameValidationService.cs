using FluentValidation.Results;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateTrackFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateTrackFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardBookFormat", ERROR_MESSAGE);

            //TODO Add Validation for TrackFilename
            //var parsedEpisodeInfo = Parser.Parser.ParseTitle(sampleResult.FileName);

            //if (parsedEpisodeInfo == null)
            //{
            //    return validationFailure;
            //}

            //if (!ValidateSeasonAndEpisodeNumbers(sampleResult.Episodes, parsedEpisodeInfo))
            //{
            //    return validationFailure;
            //}
            return null;
        }
    }
}
