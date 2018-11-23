using System.Collections.Generic;
using Radarr.Http.REST;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using Radarr.Http;

namespace NzbDrone.Api.ExtraFiles
{
    public class ExtraFileModule : RadarrRestModule<ExtraFileResource>
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IExtraFileService<OtherExtraFile> _otherFileService;

        public ExtraFileModule(IExtraFileService<SubtitleFile> subtitleFileService, IExtraFileService<MetadataFile> metadataFileService, IExtraFileService<OtherExtraFile> otherExtraFileService)
            : base("/extrafile")
        {
            _subtitleFileService = subtitleFileService;
            _metadataFileService = metadataFileService;
            _otherFileService = otherExtraFileService;
            GetResourceAll = GetFiles;
        }

        private List<ExtraFileResource> GetFiles()
        {
            if (!Request.Query.MovieId.HasValue)
            {
                throw new BadRequestException("MovieId is missing");
            }

            var extraFiles = new List<ExtraFileResource>();

            List<SubtitleFile> subtitleFiles = _subtitleFileService.GetFilesByMovie(Request.Query.MovieId);
            List<MetadataFile> metadataFiles = _metadataFileService.GetFilesByMovie(Request.Query.MovieId);
            List<OtherExtraFile> otherExtraFiles = _otherFileService.GetFilesByMovie(Request.Query.MovieId);

            extraFiles.AddRange(subtitleFiles.ToResource());
            extraFiles.AddRange(metadataFiles.ToResource());
            extraFiles.AddRange(otherExtraFiles.ToResource());

            return extraFiles;
        }
    }
}
