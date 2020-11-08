namespace NzbDrone.Core.ImportLists
{
    public enum CleanLibraryLevel
    {
        Disabled,
        LogOnly,
        KeepAndUnmonitor,
        RemoveAndKeep,
        RemoveAndDelete
    }
}
