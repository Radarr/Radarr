using System;
using System.IO;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace Radarr.Api.V3.Movies
{
    public class MovieFolderAsRootFolderValidator : PropertyValidator
    {
        private readonly IBuildFileNames _fileNameBuilder;

        public MovieFolderAsRootFolderValidator(IBuildFileNames fileNameBuilder)
        {
            _fileNameBuilder = fileNameBuilder;
        }

        protected override string GetDefaultMessageTemplate() => "Root folder path contains movie folder";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var movieResource = context.InstanceToValidate as MovieResource;

            if (movieResource == null)
            {
                return true;
            }

            var rootFolderPath = context.PropertyValue.ToString();

            if (rootFolderPath.IsNullOrWhiteSpace())
            {
                return true;
            }

            var rootFolder = new DirectoryInfo(rootFolderPath!).Name;
            var movie = movieResource.ToModel();
            var movieFolder = _fileNameBuilder.GetMovieFolder(movie);

            if (movieFolder == rootFolder)
            {
                return false;
            }

            var distance = movieFolder.LevenshteinDistance(rootFolder);

            return distance >= Math.Max(1, movieFolder.Length * 0.2);
        }
    }
}
