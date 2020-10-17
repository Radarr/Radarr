using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportList : IProvider
    {
        bool Enabled { get; }
        ImportListType EnableAuto { get; }

        ImportListSource ListType { get; }
        ImportListFetchResult Fetch();
    }
}
