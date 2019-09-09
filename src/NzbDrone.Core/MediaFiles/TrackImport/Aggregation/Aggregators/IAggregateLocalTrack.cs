namespace NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators
{
    public interface IAggregate<T>
    {
        T Aggregate(T item, bool otherFiles);
    }
}
