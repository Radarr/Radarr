using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport;

public interface ILocalMovieCustomFormatCalculationService
{
    public List<CustomFormat> ParseMovieCustomFormats(LocalMovie localMovie);
    public void UpdateMovieCustomFormats(LocalMovie localMovie);
}

public class LocalMovieCustomFormatCalculationService : ILocalMovieCustomFormatCalculationService
{
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public LocalMovieCustomFormatCalculationService(IBuildFileNames fileNameBuilder, ICustomFormatCalculationService formatCalculator)
    {
        _fileNameBuilder = fileNameBuilder;
        _formatCalculator = formatCalculator;
    }

    public List<CustomFormat> ParseMovieCustomFormats(LocalMovie localMovie)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localMovie.Movie, localMovie.ToMovieFile());
        return _formatCalculator.ParseCustomFormat(localMovie, fileNameUsedForCustomFormatCalculation);
    }

    public void UpdateMovieCustomFormats(LocalMovie localMovie)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localMovie.Movie, localMovie.ToMovieFile());
        localMovie.CustomFormats = _formatCalculator.ParseCustomFormat(localMovie, fileNameUsedForCustomFormatCalculation);
        localMovie.FileNameUsedForCustomFormatCalculation = fileNameUsedForCustomFormatCalculation;
        localMovie.CustomFormatScore = localMovie.Movie.QualityProfile?.CalculateCustomFormatScore(localMovie.CustomFormats) ?? 0;

        localMovie.OriginalFileNameCustomFormats = _formatCalculator.ParseCustomFormat(localMovie, Path.GetFileName(localMovie.Path));
        localMovie.OriginalFileNameCustomFormatScore = localMovie.Movie.QualityProfile?.CalculateCustomFormatScore(localMovie.OriginalFileNameCustomFormats) ?? 0;
    }
}
