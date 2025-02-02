namespace NzbDrone.Core.MediaFiles.MovieImport;

public enum ImportRejectionReason
{
    Unknown,
    FileLocked,
    UnknownMovie,
    ExecutableFile,
    ArchiveFile,
    MovieFolder,
    InvalidFilePath,
    UnsupportedExtension,
    InvalidMovie,
    UnableToParse,
    Error,
    DecisionError,
    MovieAlreadyImported,
    MinimumFreeSpace,
    NoAudio,
    MovieNotFoundInRelease,
    Sample,
    SampleIndeterminate,
    Unpacking,
    MultiPartMovie,
    NotQualityUpgrade,
    NotRevisionUpgrade,
    NotCustomFormatUpgrade
}
