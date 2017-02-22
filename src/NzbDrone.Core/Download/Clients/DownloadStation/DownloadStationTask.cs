﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTask
    {
        public string Username { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// /// Possible values are: BT, NZB, http, ftp, eMule and https
        /// </summary>
        public string Type { get; set; }

        [JsonProperty(PropertyName = "status_extra")]
        public Dictionary<string, string> StatusExtra { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationTaskStatus Status { get; set; }

        public DownloadStationTaskAdditional Additional { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public enum DownloadStationTaskType
    {
        BT, NZB, http, ftp, eMule, https
    }

    public enum DownloadStationTaskStatus
    {
        Waiting,
        Downloading,
        Paused,
        Finishing,
        Finished,
        HashChecking,
        Seeding,
        FileHostingWaiting,
        Extracting,
        Error
    }

    public enum DownloadStationPriority
    {
        Auto,
        Low,
        Normal,
        High
    }
}
