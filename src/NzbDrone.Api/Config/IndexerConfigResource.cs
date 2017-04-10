using NzbDrone.Api.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class IndexerConfigResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int Retention { get; set; }
        public int RssSyncInterval { get; set; }
	public int AvailabilityDelay { get; set; }
	public bool AllowHardcodedSubs { get; set; }
    }

    public static class IndexerConfigResourceMapper
    {
        public static IndexerConfigResource ToResource(IConfigService model)
        {
            return new IndexerConfigResource
            {
                MinimumAge = model.MinimumAge,
                Retention = model.Retention,
                RssSyncInterval = model.RssSyncInterval,
		AvailabilityDelay = model.AvailabilityDelay,
		AllowHardcodedSubs = model.AllowHardcodedSubs,
            };
        }
    }
}
