using System;
using System.Collections.Generic;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Series
{
    [Obsolete("SeriesModule is Obsolete, Remove with new UI")]
    public class SeriesModule : NzbDroneRestModuleWithSignalR<SeriesResource, Core.Tv.Series>

    {
        public SeriesModule(IBroadcastSignalRMessage signalRBroadcaster
        )
            : base(signalRBroadcaster)
        {
            GetResourceAll = AllSeries;
            GetResourceById = GetSeries;
            CreateResource = AddSeries;
            UpdateResource = UpdateSeries;
            DeleteResource = DeleteSeries;
        }

        private SeriesResource GetSeries(int id)
        {
            return new SeriesResource();
        }

        private List<SeriesResource> AllSeries()
        {
            return new List<SeriesResource>();
        }

        private int AddSeries(SeriesResource seriesResource)
        {
            return 0;
        }

        private void UpdateSeries(SeriesResource seriesResource)
        {
            throw new NotImplementedException();
        }

        private void DeleteSeries(int id)
        {
            throw new NotImplementedException();
        }
    }
}
