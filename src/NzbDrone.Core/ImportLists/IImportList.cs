using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportList : IProvider
    {
        bool Enabled { get; }
        bool EnableAuto { get; }

        ImportListType ListType { get; }
        ImportListFetchResult Fetch();
    }
}
