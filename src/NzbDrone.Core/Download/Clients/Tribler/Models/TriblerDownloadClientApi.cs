using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public enum DownloadStatus
    {
        [EnumMember(Value = @"WAITING4HASHCHECK")]
        Waiting4HashCheck = 0,

        [EnumMember(Value = @"HASHCHECKING")]
        Hashchecking = 1,

        [EnumMember(Value = @"METADATA")]
        Metadata = 2,

        [EnumMember(Value = @"DOWNLOADING")]
        Downloading = 3,

        [EnumMember(Value = @"SEEDING")]
        Seeding = 4,

        [EnumMember(Value = @"STOPPED")]
        Stopped = 5,

        [EnumMember(Value = @"ALLOCATING_DISKSPACE")]
        AllocatingDiskspace = 6,

        [EnumMember(Value = @"EXIT_NODES")]
        Exitnodes = 7,

        [EnumMember(Value = @"CIRCUITS")]
        Circuits = 8,

        [EnumMember(Value = @"STOPPED_ON_ERROR")]
        StoppedOnError = 9,

        [EnumMember(Value = @"LOADING")]
        Loading = 10,
    }

    public class Trackers
    {
        public string Url { get; set; }
        [JsonProperty("peers")]
        public object Peers { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Download
    {
        public string Name { get; set; }
        public float? Progress { get; set; }
        public string Infohash { get; set; }
        public bool? AnonDownload { get; set; }
        public float? Availability { get; set; }
        public double? Eta { get; set; }
        public long? TotalPieces { get; set; }
        public long? NumSeeds { get; set; }
        public long? AllTimeUpload { get; set; }
        public long? AllTimeDownload { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStatus? Status { get; set; }

        public int? StatusCode { get; set; }
        public float? AllTimeRatio { get; set; }
        public long? TimeAdded { get; set; }
        public long? MaxUploadSpeed { get; set; }
        public long? MaxDownloadSpeed { get; set; }
        public long? Hops { get; set; }
        public bool? SafeSeeding { get; set; }
        public string Error { get; set; }
        public long? TotalDown { get; set; }
        public long? Size { get; set; }
        public string Destination { get; set; }
        public float? SpeedDown { get; set; }
        public float? SpeedUp { get; set; }
        public long? NumPeers { get; set; }
        public List<Trackers> Trackers { get; set; }
    }

    public class DownloadsResponse
    {
        public List<Download> Downloads { get; set; }
    }

    public class AddDownloadRequest
    {
        [JsonProperty("anon_hops")]
        public long? AnonymityHops { get; set; }

        [JsonProperty("safe_seeding")]
        public bool? SafeSeeding { get; set; }
        public string Destination { get; set; }

        [JsonProperty("uri", Required = Newtonsoft.Json.Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Uri { get; set; }
    }

    public class AddDownloadResponse
    {
        public string Infohash { get; set; }
        public bool? Started { get; set; }
    }

    public class RemoveDownloadRequest
    {
        [JsonProperty("remove_data")]
        public bool? RemoveData { get; set; }
    }

    public class DeleteDownloadResponse
    {
        public bool? Removed { get; set; }
        public string Infohash { get; set; }
    }

    public class UpdateDownloadRequest
    {
        [JsonProperty("anon_hops")]
        public long? AnonHops { get; set; }

        [JsonProperty("selected_files")]
        public List<int> Selected_files { get; set; }

        public string State { get; set; }
    }

    public class UpdateDownloadResponse
    {
        public bool? Modified { get; set; }
        public string Infohash { get; set; }
    }

    public class File
    {
        public long? Size { get; set; }
        public long? Index { get; set; }
        public string Name { get; set; }
        public float? Progress { get; set; }
        public bool? Included { get; set; }
    }

    public class GetFilesResponse
    {
        public List<File> Files { get; set; }
    }
}
