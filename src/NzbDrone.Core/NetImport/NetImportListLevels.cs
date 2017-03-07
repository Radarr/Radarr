namespace NzbDrone.Core.NetImport
{
    public enum NetImportCleanLibraryLevels
    {
        Disabled,
        LogOnly,
        KeepAndUnmonitor,
        RemoveAndKeep,
        RemoveAndDelete
    }
}
