using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Kometa
{
    public class KometaMetadata : MetadataBase<KometaMetadataSettings>
    {
        private static readonly Regex MovieImagesRegex = new (@"^(?:poster|background)\.(?:png|jpe?g)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ILocalizationService _localizationService;

        public override string Name => "Kometa";

        public override ProviderMessage Message => new (_localizationService.GetLocalizedString("MetadataKometaDeprecated"), ProviderMessageType.Warning);

        public KometaMetadata(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public override MetadataFile FindMetadataFile(Movie movie, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var metadata = new MetadataFile
            {
                MovieId = movie.Id,
                Consumer = GetType().Name,
                RelativePath = movie.Path.GetRelativePath(path)
            };

            if (MovieImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.MovieImage;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult MovieMetadata(Movie movie, MovieFile movieFile)
        {
            return null;
        }

        public override List<ImageFileResult> MovieImages(Movie movie)
        {
            return new List<ImageFileResult>();
        }
    }
}
