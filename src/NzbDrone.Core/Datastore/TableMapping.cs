using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.ArtistStats;
using NzbDrone.Core.Tags;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.CustomFilters;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Lyrics;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music;
using Marr.Data.QGen;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.Datastore
{
    public static class TableMapping
    {
        private static readonly FluentMappings Mapper = new FluentMappings(true);

        public static void Map()
        {
            RegisterMappers();

            Mapper.Entity<Config>().RegisterModel("Config");

            Mapper.Entity<RootFolder>().RegisterModel("RootFolders")
                  .Ignore(r => r.Accessible)
                  .Ignore(r => r.FreeSpace)
                  .Ignore(r => r.TotalSpace);

            Mapper.Entity<ScheduledTask>().RegisterModel("ScheduledTasks");

            Mapper.Entity<IndexerDefinition>().RegisterDefinition("Indexers")
                  .Ignore(i => i.Enable)
                  .Ignore(i => i.Protocol)
                  .Ignore(i => i.SupportsRss)
                  .Ignore(i => i.SupportsSearch)
                  .Ignore(d => d.Tags);

            Mapper.Entity<ImportListDefinition>().RegisterDefinition("ImportLists")
                .Ignore(i => i.Enable)
                .Ignore(i => i.ListType);

            Mapper.Entity<NotificationDefinition>().RegisterDefinition("Notifications")
                  .Ignore(i => i.SupportsOnGrab)
                  .Ignore(i => i.SupportsOnReleaseImport)
                  .Ignore(i => i.SupportsOnUpgrade)
                  .Ignore(i => i.SupportsOnRename)
                  .Ignore(i => i.SupportsOnHealthIssue)
                  .Ignore(i => i.SupportsOnDownloadFailure)
                  .Ignore(i => i.SupportsOnImportFailure)
                  .Ignore(i => i.SupportsOnTrackRetag);
            
            Mapper.Entity<MetadataDefinition>().RegisterDefinition("Metadata")
                  .Ignore(d => d.Tags);

            Mapper.Entity<DownloadClientDefinition>().RegisterDefinition("DownloadClients")
                  .Ignore(d => d.Protocol)
                  .Ignore(d => d.Tags);

            Mapper.Entity<History.History>().RegisterModel("History")
                .AutoMapChildModels();

            Mapper.Entity<Artist>().RegisterModel("Artists")
                .Ignore(s => s.RootFolderPath)
                .Ignore(s => s.Name)
                .Ignore(s => s.ForeignArtistId)
                .Relationship()
                .HasOne(a => a.Metadata, a => a.ArtistMetadataId)
                .HasOne(a => a.QualityProfile, a => a.QualityProfileId)
                .HasOne(s => s.MetadataProfile, s => s.MetadataProfileId)
                .For(a => a.Albums)
                .LazyLoad(condition: a => a.Id > 0, query: (db, a) => db.Query<Album>().Where(rg => rg.ArtistMetadataId == a.Id).ToList());

            Mapper.Entity<ArtistMetadata>().RegisterModel("ArtistMetadata");

            Mapper.Entity<Album>().RegisterModel("Albums")
                .Ignore(r => r.ArtistId)
                .Relationship()
                .HasOne(r => r.ArtistMetadata, r => r.ArtistMetadataId)
                .For(rg => rg.AlbumReleases)
                .LazyLoad(condition: rg => rg.Id > 0, query: (db, rg) => db.Query<AlbumRelease>().Where(r => r.AlbumId == rg.Id).ToList())
                .For(rg => rg.Artist)
                .LazyLoad(condition: rg => rg.ArtistMetadataId > 0,
                          query: (db, rg) => db.Query<Artist>()
                          .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                          .Where(a => a.ArtistMetadataId == rg.ArtistMetadataId).SingleOrDefault());

            Mapper.Entity<AlbumRelease>().RegisterModel("AlbumReleases")
                .Relationship()
                .HasOne(r => r.Album, r => r.AlbumId)
                .For(r => r.Tracks)
                .LazyLoad(condition: r => r.Id > 0, query: (db, r) => db.Query<Track>().Where(t => t.AlbumReleaseId == r.Id).ToList());

            Mapper.Entity<Track>().RegisterModel("Tracks")
                .Ignore(t => t.HasFile)
                .Ignore(t => t.AlbumId)
                .Ignore(t => t.Album)
                .Relationship()
                .HasOne(track => track.AlbumRelease, track => track.AlbumReleaseId)
                .HasOne(track => track.ArtistMetadata, track => track.ArtistMetadataId)
                .For(track => track.TrackFile)
                .LazyLoad(condition: track => track.TrackFileId > 0,
                          query: (db, track) => db.Query<TrackFile>()
                          .Join<TrackFile, Track>(JoinType.Inner, t => t.Tracks, (t, x) => t.Id == x.TrackFileId)
                          .Join<TrackFile, Album>(JoinType.Inner, t => t.Album, (t, a) => t.AlbumId == a.Id)
                          .Join<TrackFile, Artist>(JoinType.Inner, t => t.Artist, (t, a) => t.Album.Value.ArtistMetadataId == a.ArtistMetadataId)
                          .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                          .Where(t => t.Id == track.TrackFileId)
                          .SingleOrDefault())
                .For(t => t.Artist)
                .LazyLoad(condition: t => t.AlbumReleaseId > 0, query: (db, t) => db.Query<Artist>()
                          .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                          .Join<Artist, Album>(JoinType.Inner, a => a.Albums, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                          .Join<Album, AlbumRelease>(JoinType.Inner, a => a.AlbumReleases, (l, r) => l.Id == r.AlbumId)
                          .Where<AlbumRelease>(r => r.Id == t.AlbumReleaseId)
                          .SingleOrDefault());

            Mapper.Entity<TrackFile>().RegisterModel("TrackFiles")
                .Relationship()
                .HasOne(f => f.Album, f => f.AlbumId)
                .For(f => f.Tracks)
                .LazyLoad(condition: f => f.Id > 0, query: (db, f) => db.Query<Track>()
                          .Where(x => x.TrackFileId == f.Id)
                          .ToList())
                .For(t => t.Artist)
                .LazyLoad(condition: f => f.Id > 0, query: (db, f) => db.Query<Artist>()
                        .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                        .Join<Artist, Album>(JoinType.Inner, a => a.Albums, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                        .Where<Album>(r => r.Id == f.AlbumId)
                        .SingleOrDefault());

            Mapper.Entity<QualityDefinition>().RegisterModel("QualityDefinitions")
                  .Ignore(d => d.GroupName)
                  .Ignore(d => d.GroupWeight)
                  .Ignore(d => d.Weight);

            Mapper.Entity<QualityProfile>().RegisterModel("QualityProfiles");
            Mapper.Entity<MetadataProfile>().RegisterModel("MetadataProfiles");
            Mapper.Entity<Log>().RegisterModel("Logs");
            Mapper.Entity<NamingConfig>().RegisterModel("NamingConfig");
            Mapper.Entity<AlbumStatistics>().MapResultSet();
            Mapper.Entity<Blacklist>().RegisterModel("Blacklist");
            Mapper.Entity<MetadataFile>().RegisterModel("MetadataFiles");
            Mapper.Entity<LyricFile>().RegisterModel("LyricFiles");
            Mapper.Entity<OtherExtraFile>().RegisterModel("ExtraFiles");

            Mapper.Entity<PendingRelease>().RegisterModel("PendingReleases")
                  .Ignore(e => e.RemoteAlbum);

            Mapper.Entity<RemotePathMapping>().RegisterModel("RemotePathMappings");
            Mapper.Entity<Tag>().RegisterModel("Tags");
            Mapper.Entity<ReleaseProfile>().RegisterModel("ReleaseProfiles");

            Mapper.Entity<DelayProfile>().RegisterModel("DelayProfiles");
            Mapper.Entity<User>().RegisterModel("Users");
            Mapper.Entity<CommandModel>().RegisterModel("Commands")
                .Ignore(c => c.Message);

            Mapper.Entity<IndexerStatus>().RegisterModel("IndexerStatus");
            Mapper.Entity<DownloadClientStatus>().RegisterModel("DownloadClientStatus");
            Mapper.Entity<ImportListStatus>().RegisterModel("ImportListStatus");

            Mapper.Entity<CustomFilter>().RegisterModel("CustomFilters");
            Mapper.Entity<ImportListExclusion>().RegisterModel("ImportListExclusions");
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();
            RegisterProviderSettingConverter();

            MapRepository.Instance.RegisterTypeConverter(typeof(int), new Int32Converter());
            MapRepository.Instance.RegisterTypeConverter(typeof(double), new DoubleConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(DateTime), new UtcConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(bool), new BooleanIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Enum), new EnumIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Quality), new QualityIntConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<QualityProfileQualityItem>), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(QualityModel), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(Dictionary<string, string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<int>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<KeyValuePair<string, int>>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<ProfilePrimaryAlbumTypeItem>), new EmbeddedDocumentConverter(new PrimaryAlbumTypeIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(List<ProfileSecondaryAlbumTypeItem>), new EmbeddedDocumentConverter(new SecondaryAlbumTypeIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(List<ProfileReleaseStatusItem>), new EmbeddedDocumentConverter(new ReleaseStatusIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(ParsedAlbumInfo), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ParsedTrackInfo), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ReleaseInfo), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(HashSet<int>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(OsPath), new OsPathConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Guid), new GuidConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(Command), new CommandConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(TimeSpan), new TimeSpanConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(TimeSpan?), new TimeSpanConverter());
        }

        private static void RegisterProviderSettingConverter()
        {
            var settingTypes = typeof(IProviderConfig).Assembly.ImplementationsOf<IProviderConfig>();

            var providerSettingConverter = new ProviderSettingConverter();
            foreach (var embeddedType in settingTypes)
            {
                MapRepository.Instance.RegisterTypeConverter(embeddedType, providerSettingConverter);
            }
        }

        private static void RegisterEmbeddedConverter()
        {
            var embeddedTypes = typeof(IEmbeddedDocument).Assembly.ImplementationsOf<IEmbeddedDocument>();

            var embeddedConvertor = new EmbeddedDocumentConverter();
            var genericListDefinition = typeof(List<>).GetGenericTypeDefinition();

            foreach (var embeddedType in embeddedTypes)
            {
                var embeddedListType = genericListDefinition.MakeGenericType(embeddedType);

                MapRepository.Instance.RegisterTypeConverter(embeddedType, embeddedConvertor);
                MapRepository.Instance.RegisterTypeConverter(embeddedListType, embeddedConvertor);
            }
        }
    }
}
