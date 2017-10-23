﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Api.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.DecisionEngine;
using System.Linq;
using NzbDrone.Core.Datastore.Migration;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseResource : RestResource
    {
        public string Guid { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public int Age { get; set; }
        public double AgeHours { get; set; }
        public double AgeMinutes { get; set; }
        public long Size { get; set; }
        public int IndexerId { get; set; }
        public string Indexer { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set; }
        public string Title { get; set; }
        public bool FullSeason { get; set; }
        public int SeasonNumber { get; set; }
        public Language Language { get; set; }
        public int Year { get; set; }
        public string MovieTitle { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public bool Approved { get; set; }
        public bool TemporarilyRejected { get; set; }
        public bool Rejected { get; set; }
        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public IEnumerable<string> Rejections { get; set; }
        public DateTime PublishDate { get; set; }
        public string CommentUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public MappingResultType MappingResult { get; set; }
        public int ReleaseWeight { get; set; }
        public int SuspectedMovieId { get; set; }

	public IEnumerable<string> IndexerFlags { get; set; }

        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public DownloadProtocol Protocol { get; set; }


        // TODO: Remove in v3
        // Used to support the original Release Push implementation
        // JsonIgnore so we don't serialize it, but can still parse it
        [JsonIgnore]
        public DownloadProtocol DownloadProtocol
        {
            get
            {
                return Protocol;
            }
            set
            {
                if (value > 0 && Protocol == 0)
                {
                    Protocol = value;
                }
            }
        }

        public bool IsDaily { get; set; }
        public bool IsAbsoluteNumbering { get; set; }
        public bool IsPossibleSpecialEpisode { get; set; }
        public bool Special { get; set; }
    }

    public static class ReleaseResourceMapper
    {
        public static ReleaseResource ToResource(this DownloadDecision model)
        {
            var releaseInfo = model.RemoteEpisode.Release;
            var parsedEpisodeInfo = model.RemoteEpisode.ParsedEpisodeInfo;
            var remoteEpisode = model.RemoteEpisode;
            var torrentInfo = (model.RemoteEpisode.Release as TorrentInfo) ?? new TorrentInfo();
            var mappingResult = MappingResultType.Success;
            if (model.IsForMovie)
            {
                mappingResult = model.RemoteMovie.MappingResult;
                var parsedMovieInfo = model.RemoteMovie.ParsedMovieInfo;
                var movieId = model.RemoteMovie.Movie?.Id ?? 0;

                return new ReleaseResource
                {
                    Guid = releaseInfo.Guid,
                    Quality = parsedMovieInfo.Quality,
                    QualityWeight = parsedMovieInfo.Quality.Quality.Id, //Id kinda hacky for wheight, but what you gonna do? TODO: Fix this shit!
                    Age = releaseInfo.Age,
                    AgeHours = releaseInfo.AgeHours,
                    AgeMinutes = releaseInfo.AgeMinutes,
                    Size = releaseInfo.Size,
                    IndexerId = releaseInfo.IndexerId,
                    Indexer = releaseInfo.Indexer,
                    ReleaseGroup = parsedMovieInfo.ReleaseGroup,
                    ReleaseHash = parsedMovieInfo.ReleaseHash,
                    Title = releaseInfo.Title,
                    //FullSeason = parsedMovieInfo.FullSeason,
                    //SeasonNumber = parsedMovieInfo.SeasonNumber,
                    Language = parsedMovieInfo.Language,
                    Year = parsedMovieInfo.Year,
                    MovieTitle = parsedMovieInfo.MovieTitle,
                    EpisodeNumbers = new int[0],
                    AbsoluteEpisodeNumbers = new int[0],
                    Approved = model.Approved,
                    TemporarilyRejected = model.TemporarilyRejected,
                    Rejected = model.Rejected,
                    TvdbId = releaseInfo.TvdbId,
                    TvRageId = releaseInfo.TvRageId,
                    Rejections = model.Rejections.Select(r => r.Reason).ToList(),
                    PublishDate = releaseInfo.PublishDate,
                    CommentUrl = releaseInfo.CommentUrl,
                    DownloadUrl = releaseInfo.DownloadUrl,
                    InfoUrl = releaseInfo.InfoUrl,
                    MappingResult = mappingResult,
                    //ReleaseWeight
                    
                    SuspectedMovieId = movieId,

                    MagnetUrl = torrentInfo.MagnetUrl,
                    InfoHash = torrentInfo.InfoHash,
                    Seeders = torrentInfo.Seeders,
                    Leechers = (torrentInfo.Peers.HasValue && torrentInfo.Seeders.HasValue) ? (torrentInfo.Peers.Value - torrentInfo.Seeders.Value) : (int?)null,
                    Protocol = releaseInfo.DownloadProtocol,
		IndexerFlags = torrentInfo.IndexerFlags.ToString().Split(new string[] { ", " }, StringSplitOptions.None),
                    Edition = parsedMovieInfo.Edition,

                    IsDaily = false,
                    IsAbsoluteNumbering = false,
                    IsPossibleSpecialEpisode = false,
                    //Special = parsedMovieInfo.Special,
                };
            }

            // TODO: Clean this mess up. don't mix data from multiple classes, use sub-resources instead? (Got a huge Deja Vu, didn't we talk about this already once?)
            return new ReleaseResource
            {
                Guid = releaseInfo.Guid,
                Quality = parsedEpisodeInfo.Quality,
                //QualityWeight
                Age = releaseInfo.Age,
                AgeHours = releaseInfo.AgeHours,
                AgeMinutes = releaseInfo.AgeMinutes,
                Size = releaseInfo.Size,
                IndexerId = releaseInfo.IndexerId,
                Indexer = releaseInfo.Indexer,
                ReleaseGroup = parsedEpisodeInfo.ReleaseGroup,
                ReleaseHash = parsedEpisodeInfo.ReleaseHash,
                Title = releaseInfo.Title,
                FullSeason = parsedEpisodeInfo.FullSeason,
                SeasonNumber = parsedEpisodeInfo.SeasonNumber,
                Language = parsedEpisodeInfo.Language,
                //AirDate = parsedEpisodeInfo.AirDate,
                //SeriesTitle = parsedEpisodeInfo.SeriesTitle,
                EpisodeNumbers = parsedEpisodeInfo.EpisodeNumbers,
                AbsoluteEpisodeNumbers = parsedEpisodeInfo.AbsoluteEpisodeNumbers,
                Approved = model.Approved,
                TemporarilyRejected = model.TemporarilyRejected,
                Rejected = model.Rejected,
                TvdbId = releaseInfo.TvdbId,
                TvRageId = releaseInfo.TvRageId,
                Rejections = model.Rejections.Select(r => r.Reason).ToList(),
                PublishDate = releaseInfo.PublishDate,
                CommentUrl = releaseInfo.CommentUrl,
                DownloadUrl = releaseInfo.DownloadUrl,
                InfoUrl = releaseInfo.InfoUrl,
                //DownloadAllowed = downloadAllowed,
                //ReleaseWeight

                MagnetUrl = torrentInfo.MagnetUrl,
                InfoHash = torrentInfo.InfoHash,
                Seeders = torrentInfo.Seeders,
                Leechers = (torrentInfo.Peers.HasValue && torrentInfo.Seeders.HasValue) ? (torrentInfo.Peers.Value - torrentInfo.Seeders.Value) : (int?)null,
                Protocol = releaseInfo.DownloadProtocol,

                IsDaily = parsedEpisodeInfo.IsDaily,
                IsAbsoluteNumbering = parsedEpisodeInfo.IsAbsoluteNumbering,
                IsPossibleSpecialEpisode = parsedEpisodeInfo.IsPossibleSpecialEpisode,
                Special = parsedEpisodeInfo.Special,
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
            model.DownloadProtocol = resource.DownloadProtocol;
            model.TvdbId = resource.TvdbId;
            model.TvRageId = resource.TvRageId;
            model.PublishDate = resource.PublishDate;

            return model;
        }
    }
}