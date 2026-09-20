using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class TriblerSettingsResponse
    {
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        public Api Api { get; set; }
        public bool Statistics { get; set; }

        [JsonProperty("content_discovery_community")]
        public ContentDiscoveryCommunity ContentDiscoveryCommunity { get; set; }
        public Database Database { get; set; }

        [JsonProperty("dht_discovery")]
        public DHTDiscovery DHTDiscovery { get; set; }

        [JsonProperty("knowledge_community")]
        public KnowledgeCommunity KnowledgeCommunity { get; set; }
        public LibTorrent LibTorrent { get; set; }
        public Recommender Recommender { get; set; }
        public Rendezvous RecoRendezvousmmender { get; set; }

        [JsonProperty("torrent_checker")]
        public TorrentChecker TorrentChecker { get; set; }

        [JsonProperty("tunnel_community")]
        public TunnelCommunity TunnelCommunity { get; set; }

        public Versioning Versioning { get; set; }

        [JsonProperty("watch_folder")]
        public WatchFolder WatchFolder { get; set; }

        [JsonProperty("state_dir")]
        public string StateDir { get; set; }

        [JsonProperty("memory_db")]
        public bool? MemoryDB { get; set; }
    }

    public class Api
    {
        [JsonProperty("http_enabled")]
        public bool HttpEnabled { get; set; }

        [JsonProperty("http_port")]
        public int HttpPort { get; set; }

        [JsonProperty("http_host")]
        public string HttpHost { get; set; }

        [JsonProperty("https_enabled")]
        public bool HttpsEnabled { get; set; }

        [JsonProperty("https_port")]
        public int HttpsPort { get; set; }

        [JsonProperty("https_host")]
        public string HttpsHost { get; set; }

        [JsonProperty("https_certfile")]
        public string HttpsCertFile { get; set; }

        [JsonProperty("http_port_running")]
        public int HttpPortRunning { get; set; }

        [JsonProperty("https_port_running")]
        public int HttpsPortRunning { get; set; }
    }

    public class ContentDiscoveryCommunity
    {
        public bool? Enabled { get; set; }
    }

    public class Database
    {
        public bool? Enabled { get; set; }
    }

    public class DHTDiscovery
    {
        public bool? Enabled { get; set; }
    }

    public class KnowledgeCommunity
    {
        public bool? Enabled { get; set; }
    }

    public class LibTorrent
    {
        [JsonProperty("download_defaults")]
        public LibTorrentDownloadDefaults DownloadDefaults { get; set; }

        // contains a lot more data, but it's not needed currently
    }

    public class Recommender
    {
        public bool? Enabled { get; set; }
    }

    public class Rendezvous
    {
        public bool? Enabled { get; set; }
    }

    public class TorrentChecker
    {
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
    }

    public class TunnelCommunity
    {
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }

        [JsonProperty("min_circuits")]
        public int? MinCircuits { get; set; }

        [JsonProperty("max_circuits")]
        public int? MaxCircuits { get; set; }
    }

    public class Versioning
    {
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
    }

    public class WatchFolder
    {
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
        [JsonProperty("directory")]
        public string Directory { get; set; }
        [JsonProperty("check_interval")]
        public int? CheckInterval { get; set; }
    }

    public class LibTorrentDownloadDefaults
    {
        [JsonProperty("anonymity_enabled")]
        public bool? AnonymityEnabled { get; set; }

        [JsonProperty("number_hops")]
        public int? NumberHops { get; set; }

        [JsonProperty("safeseeding_enabled")]
        public bool? SafeSeedingEnabled { get; set; }

        [JsonProperty("saveas")]
        public string SaveAS { get; set; }

        [JsonProperty("seeding_mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadDefaultsSeedingMode? SeedingMode { get; set; }

        [JsonProperty("seeding_ratio")]
        public double? SeedingRatio { get; set; }

        [JsonProperty("seeding_time")]
        public double? SeedingTime { get; set; }
    }

    public enum DownloadDefaultsSeedingMode
    {
        [EnumMember(Value = @"ratio")]
        Ratio = 0,

        [EnumMember(Value = @"forever")]
        Forever = 1,

        [EnumMember(Value = @"time")]
        Time = 2,

        [EnumMember(Value = @"never")]
        Never = 3,
    }
}
