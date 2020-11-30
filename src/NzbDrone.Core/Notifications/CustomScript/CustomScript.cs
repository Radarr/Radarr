using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Books;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public CustomScript(IDiskProvider diskProvider, IProcessProvider processProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public override string Name => "Custom Script";

        public override string Link => "https://github.com/Readarr/Readarr/wiki/Custom-Post-Processing-Scripts";

        public override ProviderMessage Message => new ProviderMessage("Testing will execute the script with the EventType set to Test, ensure your script handles this correctly", ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var author = message.Author;
            var remoteBook = message.Book;
            var releaseGroup = remoteBook.ParsedBookInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Readarr_EventType", "Grab");
            environmentVariables.Add("Readarr_Author_Id", author.Id.ToString());
            environmentVariables.Add("Readarr_Author_Name", author.Metadata.Value.Name);
            environmentVariables.Add("Readarr_Author_GRId", author.Metadata.Value.ForeignAuthorId);
            environmentVariables.Add("Readarr_Release_BookCount", remoteBook.Books.Count.ToString());
            environmentVariables.Add("Readarr_Release_BookReleaseDates", string.Join(",", remoteBook.Books.Select(e => e.ReleaseDate)));
            environmentVariables.Add("Readarr_Release_BookTitles", string.Join("|", remoteBook.Books.Select(e => e.Title)));
            environmentVariables.Add("Readarr_Release_BookIds", string.Join("|", remoteBook.Books.Select(e => e.Id.ToString())));
            environmentVariables.Add("Readarr_Release_GRIds", remoteBook.Books.Select(x => x.Editions.Value.Single(e => e.Monitored).ForeignEditionId).ConcatToString("|"));
            environmentVariables.Add("Readarr_Release_Title", remoteBook.Release.Title);
            environmentVariables.Add("Readarr_Release_Indexer", remoteBook.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Readarr_Release_Size", remoteBook.Release.Size.ToString());
            environmentVariables.Add("Readarr_Release_Quality", remoteBook.ParsedBookInfo.Quality.Quality.Name);
            environmentVariables.Add("Readarr_Release_QualityVersion", remoteBook.ParsedBookInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Readarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Readarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Readarr_Download_Id", message.DownloadId ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            var author = message.Author;
            var book = message.Book;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Readarr_EventType", "Download");
            environmentVariables.Add("Readarr_Author_Id", author.Id.ToString());
            environmentVariables.Add("Readarr_Author_Name", author.Metadata.Value.Name);
            environmentVariables.Add("Readarr_Author_Path", author.Path);
            environmentVariables.Add("Readarr_Author_GRId", author.Metadata.Value.ForeignAuthorId);
            environmentVariables.Add("Readarr_Book_Id", book.Id.ToString());
            environmentVariables.Add("Readarr_Book_Title", book.Title);
            environmentVariables.Add("Readarr_Book_GRId", book.ForeignBookId);
            environmentVariables.Add("Readarr_Book_ReleaseDate", book.ReleaseDate.ToString());
            environmentVariables.Add("Readarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Readarr_Download_Id", message.DownloadId ?? string.Empty);

            if (message.BookFiles.Any())
            {
                environmentVariables.Add("Readarr_AddedBookPaths", string.Join("|", message.BookFiles.Select(e => e.Path)));
            }

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Readarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => e.Path)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Author author)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Readarr_EventType", "Rename");
            environmentVariables.Add("Readarr_Author_Id", author.Id.ToString());
            environmentVariables.Add("Readarr_Author_Name", author.Metadata.Value.Name);
            environmentVariables.Add("Readarr_Author_Path", author.Path);
            environmentVariables.Add("Readarr_Author_GRId", author.Metadata.Value.ForeignAuthorId);

            ExecuteScript(environmentVariables);
        }

        public override void OnBookRetag(BookRetagMessage message)
        {
            var author = message.Author;
            var book = message.Book;
            var bookFile = message.BookFile;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Readarr_EventType", "TrackRetag");
            environmentVariables.Add("Readarr_Author_Id", author.Id.ToString());
            environmentVariables.Add("Readarr_Author_Name", author.Metadata.Value.Name);
            environmentVariables.Add("Readarr_Author_Path", author.Path);
            environmentVariables.Add("Readarr_Author_GRId", author.Metadata.Value.ForeignAuthorId);
            environmentVariables.Add("Readarr_Book_Id", book.Id.ToString());
            environmentVariables.Add("Readarr_Book_Title", book.Title);
            environmentVariables.Add("Readarr_Book_GRId", book.ForeignBookId);
            environmentVariables.Add("Readarr_Book_ReleaseDate", book.ReleaseDate.ToString());
            environmentVariables.Add("Readarr_BookFile_Id", bookFile.Id.ToString());
            environmentVariables.Add("Readarr_BookFile_Path", bookFile.Path);
            environmentVariables.Add("Readarr_BookFile_Quality", bookFile.Quality.Quality.Name);
            environmentVariables.Add("Readarr_BookFile_QualityVersion", bookFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Readarr_BookFile_ReleaseGroup", bookFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Readarr_BookFile_SceneName", bookFile.SceneName ?? string.Empty);
            environmentVariables.Add("Readarr_Tags_Diff", message.Diff.ToJson());
            environmentVariables.Add("Readarr_Tags_Scrubbed", message.Scrubbed.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Readarr_EventType", "HealthIssue");
            environmentVariables.Add("Readarr_Health_Issue_Level", nameof(healthCheck.Type));
            environmentVariables.Add("Readarr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Readarr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Readarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new NzbDroneValidationFailure("Path", "File does not exist"));
            }

            try
            {
                var environmentVariables = new StringDictionary();
                environmentVariables.Add("Readarr_EventType", "Test");

                var processOutput = ExecuteScript(environmentVariables);

                if (processOutput.ExitCode != 0)
                {
                    failures.Add(new NzbDroneValidationFailure(string.Empty, $"Script exited with code: {processOutput.ExitCode}"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                failures.Add(new NzbDroneValidationFailure(string.Empty, ex.Message));
            }

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug($"Script Output: {System.Environment.NewLine}{string.Join(System.Environment.NewLine, processOutput.Lines)}");

            return processOutput;
        }
    }
}
