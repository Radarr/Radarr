using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using Radarr.Http;

namespace Radarr.Api.V3.ExtraFiles
{
    [V3ApiController("extrafile")]
    public class ExtraFileController : Controller
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IExtraFileService<OtherExtraFile> _otherFileService;

        public ExtraFileController(IExtraFileService<SubtitleFile> subtitleFileService, IExtraFileService<MetadataFile> metadataFileService, IExtraFileService<OtherExtraFile> otherExtraFileService)
        {
            _subtitleFileService = subtitleFileService;
            _metadataFileService = metadataFileService;
            _otherFileService = otherExtraFileService;
        }

        [HttpGet]
        public List<ExtraFileResource> GetFiles(int movieId)
        {
            var extraFiles = new List<ExtraFileResource>();

            List<SubtitleFile> subtitleFiles = _subtitleFileService.GetFilesByMovie(movieId);
            List<MetadataFile> metadataFiles = _metadataFileService.GetFilesByMovie(movieId);
            List<OtherExtraFile> otherExtraFiles = _otherFileService.GetFilesByMovie(movieId);

            extraFiles.AddRange(subtitleFiles.ToResource());
            extraFiles.AddRange(metadataFiles.ToResource());
            extraFiles.AddRange(otherExtraFiles.ToResource());

            return extraFiles;
        }
    }
}
