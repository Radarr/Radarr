using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFilters;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Extras.Lyrics;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tags;
using NzbDrone.Core.ThingiProvider;
using static Dapper.SqlMapper;

namespace NzbDrone.Core.Datastore
{
    public static class TableMapping
    {
        static TableMapping()
        {
            Mapper = new TableMapper();
        }

        public static TableMapper Mapper { get; private set; }

        public static void Map()
        {
            RegisterMappers();

            Mapper.Entity<Config>("Config").RegisterModel();

            Mapper.Entity<RootFolder>("RootFolders").RegisterModel()
                  .Ignore(r => r.Accessible)
                  .Ignore(r => r.FreeSpace)
                  .Ignore(r => r.TotalSpace);

            Mapper.Entity<ScheduledTask>("ScheduledTasks").RegisterModel();

            Mapper.Entity<IndexerDefinition>("Indexers").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.Enable)
                  .Ignore(i => i.Protocol)
                  .Ignore(i => i.SupportsRss)
                  .Ignore(i => i.SupportsSearch)
                  .Ignore(d => d.Tags);

            Mapper.Entity<ImportListDefinition>("ImportLists").RegisterModel()
                .Ignore(x => x.ImplementationName)
                .Ignore(i => i.Enable)
                .Ignore(i => i.ListType);

            Mapper.Entity<NotificationDefinition>("Notifications").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.SupportsOnGrab)
                  .Ignore(i => i.SupportsOnReleaseImport)
                  .Ignore(i => i.SupportsOnUpgrade)
                  .Ignore(i => i.SupportsOnRename)
                  .Ignore(i => i.SupportsOnHealthIssue)
                  .Ignore(i => i.SupportsOnDownloadFailure)
                  .Ignore(i => i.SupportsOnImportFailure)
                  .Ignore(i => i.SupportsOnTrackRetag);

            Mapper.Entity<MetadataDefinition>("Metadata").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Tags);

            Mapper.Entity<DownloadClientDefinition>("DownloadClients").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Protocol)
                  .Ignore(d => d.Tags);

            Mapper.Entity<History.History>("History").RegisterModel();

            Mapper.Entity<Artist>("Artists")
                .Ignore(s => s.RootFolderPath)
                .Ignore(s => s.Name)
                .Ignore(s => s.ForeignArtistId)
                .HasOne(a => a.Metadata, a => a.ArtistMetadataId)
                .HasOne(a => a.QualityProfile, a => a.QualityProfileId)
                .HasOne(s => s.MetadataProfile, s => s.MetadataProfileId)
                .LazyLoad(a => a.Albums, (db, a) => db.Query<Album>(new SqlBuilder().Where<Album>(rg => rg.ArtistMetadataId == a.Id)).ToList(), a => a.Id > 0);

            Mapper.Entity<ArtistMetadata>("ArtistMetadata").RegisterModel();

            Mapper.Entity<Album>("Albums").RegisterModel()
                .Ignore(x => x.ArtistId)
                .HasOne(r => r.ArtistMetadata, r => r.ArtistMetadataId)
                .LazyLoad(a => a.AlbumReleases, (db, album) => db.Query<AlbumRelease>(new SqlBuilder().Where<AlbumRelease>(r => r.AlbumId == album.Id)).ToList(), a => a.Id > 0)
                .LazyLoad(a => a.Artist,
                          (db, album) => ArtistRepository.Query(db,
                                                                new SqlBuilder()
                                                                .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id)
                                                                .Where<Artist>(a => a.ArtistMetadataId == album.ArtistMetadataId)).SingleOrDefault(),
                          a => a.ArtistMetadataId > 0);

            Mapper.Entity<AlbumRelease>("AlbumReleases").RegisterModel()
                .HasOne(r => r.Album, r => r.AlbumId)
                .LazyLoad(x => x.Tracks, (db, release) => db.Query<Track>(new SqlBuilder().Where<Track>(t => t.AlbumReleaseId == release.Id)).ToList(), r => r.Id > 0);

            Mapper.Entity<Track>("Tracks").RegisterModel()
                .Ignore(t => t.HasFile)
                .Ignore(t => t.AlbumId)
                .HasOne(track => track.AlbumRelease, track => track.AlbumReleaseId)
                .HasOne(track => track.ArtistMetadata, track => track.ArtistMetadataId)
                .LazyLoad(t => t.TrackFile,
                          (db, track) => MediaFileRepository.Query(db,
                                                                   new SqlBuilder()
                                                                   .Join<TrackFile, Track>((l, r) => l.Id == r.TrackFileId)
                                                                   .Join<TrackFile, Album>((l, r) => l.AlbumId == r.Id)
                                                                   .Join<Album, Artist>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                                                                   .Join<Artist, ArtistMetadata>((l, r) => l.ArtistMetadataId == r.Id)
                                                                   .Where<TrackFile>(t => t.Id == track.TrackFileId)).SingleOrDefault(),
                          t => t.TrackFileId > 0)
                .LazyLoad(x => x.Artist,
                          (db, t) => ArtistRepository.Query(db,
                                                            new SqlBuilder()
                                                            .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id)
                                                            .Join<Artist, Album>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                                                            .Join<Album, AlbumRelease>((l, r) => l.Id == r.AlbumId)
                                                            .Where<AlbumRelease>(r => r.Id == t.AlbumReleaseId)).SingleOrDefault(),
                          t => t.Id > 0);

            Mapper.Entity<TrackFile>("TrackFiles").RegisterModel()
                .HasOne(f => f.Album, f => f.AlbumId)
                .LazyLoad(x => x.Tracks, (db, file) => db.Query<Track>(new SqlBuilder().Where<Track>(t => t.TrackFileId == file.Id)).ToList(), x => x.Id > 0)
                .LazyLoad(x => x.Artist,
                          (db, f) => ArtistRepository.Query(db,
                                                            new SqlBuilder()
                                                            .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id)
                                                            .Join<Artist, Album>((l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                                                            .Where<Album>(a => a.Id == f.AlbumId)).SingleOrDefault(),
                          t => t.Id > 0);

            Mapper.Entity<QualityDefinition>("QualityDefinitions").RegisterModel()
                  .Ignore(d => d.GroupName)
                  .Ignore(d => d.GroupWeight)
                  .Ignore(d => d.Weight);

            Mapper.Entity<QualityProfile>("QualityProfiles").RegisterModel();
            Mapper.Entity<MetadataProfile>("MetadataProfiles").RegisterModel();
            Mapper.Entity<Log>("Logs").RegisterModel();
            Mapper.Entity<NamingConfig>("NamingConfig").RegisterModel();

            Mapper.Entity<Blacklist>("Blacklist").RegisterModel();
            Mapper.Entity<MetadataFile>("MetadataFiles").RegisterModel();
            Mapper.Entity<LyricFile>("LyricFiles").RegisterModel();
            Mapper.Entity<OtherExtraFile>("ExtraFiles").RegisterModel();

            Mapper.Entity<PendingRelease>("PendingReleases").RegisterModel()
                  .Ignore(e => e.RemoteAlbum);

            Mapper.Entity<RemotePathMapping>("RemotePathMappings").RegisterModel();
            Mapper.Entity<Tag>("Tags").RegisterModel();
            Mapper.Entity<ReleaseProfile>("ReleaseProfiles").RegisterModel();

            Mapper.Entity<DelayProfile>("DelayProfiles").RegisterModel();
            Mapper.Entity<User>("Users").RegisterModel();
            Mapper.Entity<CommandModel>("Commands").RegisterModel()
                  .Ignore(c => c.Message);

            Mapper.Entity<IndexerStatus>("IndexerStatus").RegisterModel();
            Mapper.Entity<DownloadClientStatus>("DownloadClientStatus").RegisterModel();
            Mapper.Entity<ImportListStatus>("ImportListStatus").RegisterModel();

            Mapper.Entity<CustomFilter>("CustomFilters").RegisterModel();
            Mapper.Entity<ImportListExclusion>("ImportListExclusions").RegisterModel();
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();
            RegisterProviderSettingConverter();

            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(new DapperUtcConverter());
            SqlMapper.AddTypeHandler(new DapperQualityIntConverter());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<QualityProfileQualityItem>>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<QualityModel>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<Dictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<IDictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<int>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<KeyValuePair<string, int>>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<KeyValuePair<string, int>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfilePrimaryAlbumTypeItem>>(new PrimaryAlbumTypeIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileSecondaryAlbumTypeItem>>(new SecondaryAlbumTypeIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileReleaseStatusItem>>(new ReleaseStatusIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedAlbumInfo>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedTrackInfo>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ReleaseInfo>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<HashSet<int>>());
            SqlMapper.AddTypeHandler(new OsPathConverter());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            SqlMapper.AddTypeHandler(new GuidConverter());
            SqlMapper.AddTypeHandler(new CommandConverter());
        }

        private static void RegisterProviderSettingConverter()
        {
            var settingTypes = typeof(IProviderConfig).Assembly.ImplementationsOf<IProviderConfig>()
                .Where(x => !x.ContainsGenericParameters);

            var providerSettingConverter = new ProviderSettingConverter();
            foreach (var embeddedType in settingTypes)
            {
                SqlMapper.AddTypeHandler(embeddedType, providerSettingConverter);
            }
        }

        private static void RegisterEmbeddedConverter()
        {
            var embeddedTypes = typeof(IEmbeddedDocument).Assembly.ImplementationsOf<IEmbeddedDocument>();

            var embeddedConverterDefinition = typeof(EmbeddedDocumentConverter<>).GetGenericTypeDefinition();
            var genericListDefinition = typeof(List<>).GetGenericTypeDefinition();

            foreach (var embeddedType in embeddedTypes)
            {
                var embeddedListType = genericListDefinition.MakeGenericType(embeddedType);

                RegisterEmbeddedConverter(embeddedType, embeddedConverterDefinition);
                RegisterEmbeddedConverter(embeddedListType, embeddedConverterDefinition);
            }
        }

        private static void RegisterEmbeddedConverter(Type embeddedType, Type embeddedConverterDefinition)
        {
            var embeddedConverterType = embeddedConverterDefinition.MakeGenericType(embeddedType);
            var converter = (ITypeHandler)Activator.CreateInstance(embeddedConverterType);

            SqlMapper.AddTypeHandler(embeddedType, converter);
        }
    }
}
