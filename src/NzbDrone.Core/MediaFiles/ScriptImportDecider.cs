using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tags;

namespace NzbDrone.Core.MediaFiles
{
    public interface IImportScript
    {
        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalMovie localMovie, MovieFile movieFile, TransferMode mode);
    }

    public class ImportScriptService : IImportScript
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IProcessProvider _processProvider;
        private readonly IConfigService _configService;
        private readonly ITagRepository _tagRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public ImportScriptService(IProcessProvider processProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IConfigService configService,
                                   IConfigFileProvider configFileProvider,
                                   ITagRepository tagRepository,
                                   IDiskProvider diskProvider,
                                   Logger logger)
        {
            _processProvider = processProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _configFileProvider = configFileProvider;
            _tagRepository = tagRepository;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex OutputRegex = new Regex(@"^(?:\[(?:(?<mediaFile>MediaFile)|(?<extraFile>ExtraFile))\]\s?(?<fileName>.+)|(?<preventExtraImport>\[PreventExtraImport\])|\[MoveStatus\]\s?(?:(?<deferMove>DeferMove)|(?<moveComplete>MoveComplete)|(?<renameRequested>RenameRequested)))$", RegexOptions.Compiled);

        private ScriptImportInfo ProcessOutput(List<ProcessOutputLine> processOutputLines)
        {
            var possibleExtraFiles = new List<string>();
            string mediaFile = null;
            var decision = ScriptImportDecision.MoveComplete;
            var importExtraFiles = true;

            foreach (var line in processOutputLines)
            {
                var match = OutputRegex.Match(line.Content);

                if (match.Groups["mediaFile"].Success)
                {
                    if (mediaFile is not null)
                    {
                        throw new ScriptImportException("Script output contains multiple media files. Only one media file can be returned.");
                    }

                    mediaFile = match.Groups["fileName"].Value;

                    if (!MediaFileExtensions.Extensions.Contains(Path.GetExtension(mediaFile)))
                    {
                        throw new ScriptImportException("Script output contains invalid media file: {0}", mediaFile);
                    }
                    else if (!_diskProvider.FileExists(mediaFile))
                    {
                        throw new ScriptImportException("Script output contains non-existent media file: {0}", mediaFile);
                    }
                }
                else if (match.Groups["extraFile"].Success)
                {
                    var fileName = match.Groups["fileName"].Value;

                    if (!_diskProvider.FileExists(fileName))
                    {
                        _logger.Warn("Script output contains non-existent possible extra file: {0}", fileName);
                    }

                    possibleExtraFiles.Add(fileName);
                }
                else if (match.Groups["moveComplete"].Success)
                {
                    decision = ScriptImportDecision.MoveComplete;
                }
                else if (match.Groups["renameRequested"].Success)
                {
                    decision = ScriptImportDecision.RenameRequested;
                }
                else if (match.Groups["deferMove"].Success)
                {
                    decision = ScriptImportDecision.DeferMove;
                }
                else if (match.Groups["preventExtraImport"].Success)
                {
                    importExtraFiles = false;
                }
            }

            return new ScriptImportInfo(possibleExtraFiles, mediaFile, decision, importExtraFiles);
        }

        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalMovie localMovie, MovieFile movieFile, TransferMode mode)
        {
            var movie = localMovie.Movie;
            var oldFiles = localMovie.OldFiles;
            var downloadClientInfo = localMovie.DownloadItem?.DownloadClientInfo;
            var downloadId = localMovie.DownloadItem?.DownloadId;

            if (!_configService.UseScriptImport)
            {
                return ScriptImportDecision.DeferMove;
            }

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_SourcePath", sourcePath);
            environmentVariables.Add("Radarr_DestinationPath", destinationFilePath);

            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_TransferMode", mode.ToString());

            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_OriginalLanguage", IsoLanguages.Get(movie.MovieMetadata.Value.OriginalLanguage).ThreeLetterCode);
            environmentVariables.Add("Radarr_Movie_Genres", string.Join("|", movie.MovieMetadata.Value.Genres));
            environmentVariables.Add("Radarr_Movie_Tags", string.Join("|", movie.Tags.Select(t => _tagRepository.Get(t).Label)));

            environmentVariables.Add("Radarr_Movie_In_Cinemas_Date", movie.MovieMetadata.Value.InCinemas.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Physical_Release_Date", movie.MovieMetadata.Value.PhysicalRelease.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);
            environmentVariables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            environmentVariables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            environmentVariables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            environmentVariables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            environmentVariables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);

            environmentVariables.Add("Radarr_Download_Client", downloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Client_Type", downloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Id", downloadId ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(localMovie.MediaInfo).ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, null));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioLanguages", movieFile.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Languages", movieFile.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Height", movieFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Width", movieFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Subtitles", movieFile.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(movieFile.MediaInfo, null));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(movieFile.MediaInfo));

            environmentVariables.Add("Radarr_MovieFile_CustomFormat", string.Join("|", localMovie.CustomFormats));
            environmentVariables.Add("Radarr_MovieFile_CustomFormatScore", localMovie.CustomFormatScore.ToString());

            if (oldFiles.Any())
            {
                environmentVariables.Add("Radarr_DeletedRelativePaths", string.Join("|", oldFiles.Select(e => e.MovieFile.RelativePath)));
                environmentVariables.Add("Radarr_DeletedPaths", string.Join("|", oldFiles.Select(e => Path.Combine(movie.Path, e.MovieFile.RelativePath))));
                environmentVariables.Add("Radarr_DeletedDateAdded", string.Join("|", oldFiles.Select(e => e.MovieFile.DateAdded)));
            }

            _logger.Debug("Executing external script: {0}", _configService.ScriptImportPath);

            var processOutput = _processProvider.StartAndCapture(_configService.ScriptImportPath, $"\"{sourcePath}\" \"{destinationFilePath}\"", environmentVariables);

            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            if (processOutput.ExitCode != 0)
            {
                throw new ScriptImportException("Script exited with non-zero exit code: {0}", processOutput.ExitCode);
            }

            var scriptImportInfo = ProcessOutput(processOutput.Lines);

            var mediaFile = scriptImportInfo.MediaFile ?? destinationFilePath;
            localMovie.PossibleExtraFiles = scriptImportInfo.PossibleExtraFiles;

            movieFile.RelativePath = movie.Path.GetRelativePath(mediaFile);
            movieFile.Path = mediaFile;

            var exitCode = processOutput.ExitCode;

            localMovie.ShouldImportExtras = scriptImportInfo.ImportExtraFiles;

            if (scriptImportInfo.Decision != ScriptImportDecision.DeferMove)
            {
                localMovie.ScriptImported = true;
            }

            if (scriptImportInfo.Decision == ScriptImportDecision.RenameRequested)
            {
                movieFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(mediaFile);
                movieFile.Path = null;
            }

            return scriptImportInfo.Decision;
        }
    }
}
