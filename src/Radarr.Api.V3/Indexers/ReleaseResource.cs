using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.CustomFormats;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Indexers
{
    public class ReleaseResource : RestResource
    {
        public string Guid { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int QualityWeight { get; set; }
        public int Age { get; set; }
        public double AgeHours { get; set; }
        public double AgeMinutes { get; set; }
        public long Size { get; set; }
        public int IndexerId { get; set; }
        public string Indexer { get; set; }
        public string ReleaseGroup { get; set; }
        public string SubGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Title { get; set; }
        public bool SceneSource { get; set; }
        public List<string> MovieTitles { get; set; }
        public List<Language> Languages { get; set; }
        public bool Approved { get; set; }
        public bool TemporarilyRejected { get; set; }
        public bool Rejected { get; set; }
        public int TmdbId { get; set; }
        public int ImdbId { get; set; }
        public IEnumerable<string> Rejections { get; set; }
        public DateTime PublishDate { get; set; }
        public string CommentUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public bool DownloadAllowed { get; set; }
        public int ReleaseWeight { get; set; }
        public IEnumerable<string> IndexerFlags { get; set; }
        public string Edition { get; set; }

        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public DownloadProtocol Protocol { get; set; }

        // Sent when queuing an unknown release
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? MovieId { get; set; }
    }

    public static class ReleaseResourceMapper
    {
        public static ReleaseResource ToResource(this DownloadDecision model)
        {
            var releaseInfo = model.RemoteMovie.Release;
            var parsedMovieInfo = model.RemoteMovie.ParsedMovieInfo;
            var remoteMovie = model.RemoteMovie;
            var torrentInfo = (model.RemoteMovie.Release as TorrentInfo) ?? new TorrentInfo();
            var indexerFlags = torrentInfo.IndexerFlags.ToString().Split(new string[] { ", " }, StringSplitOptions.None).Where(x => x != "0");

            // TODO: Clean this mess up. don't mix data from multiple classes, use sub-resources instead? (Got a huge Deja Vu, didn't we talk about this already once?)
            return new ReleaseResource
            {
                Guid = releaseInfo.Guid,
                Quality = parsedMovieInfo.Quality,
                CustomFormats = remoteMovie.CustomFormats.ToResource(),
                CustomFormatScore = remoteMovie.CustomFormatScore,

                //QualityWeight
                Age = releaseInfo.Age,
                AgeHours = releaseInfo.AgeHours,
                AgeMinutes = releaseInfo.AgeMinutes,
                Size = releaseInfo.Size,
                IndexerId = releaseInfo.IndexerId,
                Indexer = releaseInfo.Indexer,
                ReleaseGroup = parsedMovieInfo.ReleaseGroup,
                ReleaseHash = parsedMovieInfo.ReleaseHash,
                Title = releaseInfo.Title,
                MovieTitles = parsedMovieInfo.MovieTitles,
                Languages = parsedMovieInfo.Languages,
                Approved = model.Approved,
                TemporarilyRejected = model.TemporarilyRejected,
                Rejected = model.Rejected,
                TmdbId = releaseInfo.TmdbId,
                ImdbId = releaseInfo.ImdbId,
                Rejections = model.Rejections.Select(r => r.Reason).ToList(),
                PublishDate = releaseInfo.PublishDate,
                CommentUrl = releaseInfo.CommentUrl,
                DownloadUrl = releaseInfo.DownloadUrl,
                InfoUrl = releaseInfo.InfoUrl,
                DownloadAllowed = remoteMovie.DownloadAllowed,
                Edition = parsedMovieInfo.Edition,

                //ReleaseWeight
                MagnetUrl = torrentInfo.MagnetUrl,
                InfoHash = torrentInfo.InfoHash,
                Seeders = torrentInfo.Seeders,
                Leechers = (torrentInfo.Peers.HasValue && torrentInfo.Seeders.HasValue) ? (torrentInfo.Peers.Value - torrentInfo.Seeders.Value) : (int?)null,
                Protocol = releaseInfo.DownloadProtocol,
                IndexerFlags = indexerFlags
            };
        }

        public static ReleaseInfo ToModel(this ReleaseResource resource)
        {
            ReleaseInfo model;

            if (resource.Protocol == DownloadProtocol.Torrent)
            {
                model = new TorrentInfo
                {
                    MagnetUrl = resource.MagnetUrl,
                    InfoHash = resource.InfoHash,
                    Seeders = resource.Seeders,
                    Peers = (resource.Seeders.HasValue && resource.Leechers.HasValue) ? (resource.Seeders + resource.Leechers) : null
                };
            }
            else
            {
                model = new ReleaseInfo();
            }

            model.Guid = resource.Guid;
            model.Title = resource.Title;
            model.Size = resource.Size;
            model.DownloadUrl = resource.DownloadUrl;
            model.InfoUrl = resource.InfoUrl;
            model.CommentUrl = resource.CommentUrl;
            model.IndexerId = resource.IndexerId;
            model.Indexer = resource.Indexer;
            model.DownloadProtocol = resource.Protocol;
            model.TmdbId = resource.TmdbId;
            model.ImdbId = resource.ImdbId;
            model.PublishDate = resource.PublishDate.ToUniversalTime();

            return model;
        }
    }
}
