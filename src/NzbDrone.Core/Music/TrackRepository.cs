using NzbDrone.Core.Datastore;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore.Extensions;
using System;

namespace NzbDrone.Core.Music
{
    public interface ITrackRepository : IBasicRepository<Track>
    {
        Track Find(int artistId, int albumId, int mediumNumber, int trackNumber);
        List<Track> GetTracks(int artistId);
        List<Track> GetTracksByAlbum(int albumId);
        List<Track> GetTracksByMedium(int albumId, int mediumNumber);
        List<Track> GetTracksByFileId(int fileId);
        List<Track> TracksWithFiles(int artistId);
        PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec);
        PagingSpec<Track> TracksWhereCutoffUnmet(PagingSpec<Track> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        void SetMonitoredFlat(Track episode, bool monitored);
        void SetMonitoredByAlbum(int artistId, int albumId, bool monitored);
        void SetFileId(int trackId, int fileId);
    }

    public class TrackRepository : BasicRepository<Track>, ITrackRepository
    {
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public TrackRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _database = database;
            _logger = logger;
        }

        public Track Find(int artistId, int albumId, int mediumNumber, int trackNumber)
        {
            return Query.Where(s => s.ArtistId == artistId)
                               .AndWhere(s => s.AlbumId == albumId)
                               .AndWhere(s => s.MediumNumber == mediumNumber)
                               .AndWhere(s => s.AbsoluteTrackNumber == trackNumber)
                               .SingleOrDefault();
        }


        public List<Track> GetTracks(int artistId)
        {
            return Query.Where(s => s.ArtistId == artistId).ToList();
        }

        public List<Track> GetTracksByAlbum(int albumId)
        {
            return Query.Where(s => s.AlbumId == albumId)
                        .ToList();
        }

        public List<Track> GetTracksByMedium(int albumId, int mediumNumber)
        {
            return Query.Where(s => s.AlbumId == albumId)
                        .AndWhere(s => s.MediumNumber == mediumNumber)
                        .ToList();
        }

        public List<Track> GetTracksByFileId(int fileId)
        {
            return Query.Where(e => e.TrackFileId == fileId).ToList();
        }

        public List<Track> TracksWithFiles(int artistId)
        {
            return Query.Join<Track, TrackFile>(JoinType.Inner, e => e.TrackFile, (e, ef) => e.TrackFileId == ef.Id)
                        .Where(e => e.ArtistId == artistId);
        }

        public PagingSpec<Track> TracksWhereCutoffUnmet(PagingSpec<Track> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.TotalRecords = TracksWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).GetRowCount();
            pagingSpec.Records = TracksWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).ToList();

            return pagingSpec;
        }

        public void SetMonitoredFlat(Track track, bool monitored)
        {
            track.Monitored = monitored;
            SetFields(track, p => p.Monitored);
        }

        public void SetMonitoredByAlbum(int artistId, int albumId, bool monitored)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("artistId", artistId);
            mapper.AddParameter("albumId", albumId);
            mapper.AddParameter("monitored", monitored);

            const string sql = "UPDATE Tracks " +
                               "SET Monitored = @monitored " +
                               "WHERE ArtistId = @artistId " +
                               "AND AlbumId = @albumId";

            mapper.ExecuteNonQuery(sql);
        }

        public void SetFileId(int trackId, int fileId)
        {
            SetFields(new Track { Id = trackId, TrackFileId = fileId }, track => track.TrackFileId);
        }

        public PagingSpec<Track> TracksWithoutFiles(PagingSpec<Track> pagingSpec)
        {
            var currentTime = DateTime.UtcNow;

            pagingSpec.TotalRecords = GetMissingTracksQuery(pagingSpec, currentTime).GetRowCount();
            pagingSpec.Records = GetMissingTracksQuery(pagingSpec, currentTime).ToList();

            return pagingSpec;
        }

        private SortBuilder<Track> GetMissingTracksQuery(PagingSpec<Track> pagingSpec, DateTime currentTime)
        {
            return Query.Join<Track, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
                            .Where(pagingSpec.FilterExpression)
                            .AndWhere(e => e.TrackFileId == 0)
                            .AndWhere(BuildAirDateUtcCutoffWhereClause(currentTime))
                            .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                            .Skip(pagingSpec.PagingOffset())
                            .Take(pagingSpec.PageSize);
        }


        private SortBuilder<Track> TracksWhereCutoffUnmetQuery(PagingSpec<Track> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            return Query.Join<Track, Artist>(JoinType.Inner, e => e.Artist, (e, s) => e.ArtistId == s.Id)
                             .Join<Track, TrackFile>(JoinType.Left, e => e.TrackFile, (e, s) => e.TrackFileId == s.Id)
                             .Where(pagingSpec.FilterExpression)
                             .AndWhere(e => e.TrackFileId != 0)
                             .AndWhere(BuildQualityCutoffWhereClause(qualitiesBelowCutoff))
                             .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                             .Skip(pagingSpec.PagingOffset())
                             .Take(pagingSpec.PageSize);
        }

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            return string.Format("WHERE datetime(strftime('%s', [t0].[AirDateUtc]) + [t1].[RunTime] * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }


        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("([t1].[ProfileId] = {0} AND [t2].[Quality] LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }
    }
}
