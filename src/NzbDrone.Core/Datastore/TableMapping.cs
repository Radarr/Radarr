using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFilters;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
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

            Mapper.Entity<Author>("Authors")
                .Ignore(s => s.RootFolderPath)
                .Ignore(s => s.Name)
                .Ignore(s => s.ForeignAuthorId)
                .HasOne(a => a.Metadata, a => a.AuthorMetadataId)
                .HasOne(a => a.QualityProfile, a => a.QualityProfileId)
                .HasOne(s => s.MetadataProfile, s => s.MetadataProfileId)
                .LazyLoad(a => a.Books, (db, a) => db.Query<Book>(new SqlBuilder().Where<Book>(rg => rg.AuthorMetadataId == a.Id)).ToList(), a => a.Id > 0);

            Mapper.Entity<Series>("Series").RegisterModel()
                .LazyLoad(s => s.LinkItems,
                          (db, series) => db.Query<SeriesBookLink>(new SqlBuilder().Where<SeriesBookLink>(s => s.SeriesId == series.Id)).ToList(),
                          s => s.Id > 0)
                .LazyLoad(s => s.Books,
                          (db, series) => db.Query<Book>(new SqlBuilder()
                                                         .Join<Book, SeriesBookLink>((l, r) => l.Id == r.BookId)
                                                         .Join<SeriesBookLink, Series>((l, r) => l.SeriesId == r.Id)
                                                         .Where<Series>(s => s.Id == series.Id)).ToList(),
                          s => s.Id > 0);

            Mapper.Entity<SeriesBookLink>("SeriesBookLink").RegisterModel();

            Mapper.Entity<AuthorMetadata>("AuthorMetadata").RegisterModel();

            Mapper.Entity<Book>("Books").RegisterModel()
                .Ignore(x => x.AuthorId)
                .HasOne(r => r.AuthorMetadata, r => r.AuthorMetadataId)
                .LazyLoad(x => x.BookFiles,
                          (db, book) => db.Query<BookFile>(new SqlBuilder()
                                                           .Join<BookFile, Book>((l, r) => l.EditionId == r.Id)
                                                           .Where<Book>(b => b.Id == book.Id)).ToList(),
                          b => b.Id > 0)
                .LazyLoad(x => x.Editions,
                          (db, book) => db.Query<Edition>(new SqlBuilder().Where<Edition>(e => e.BookId == book.Id)).ToList(),
                          b => b.Id > 0)
                .LazyLoad(a => a.Author,
                          (db, book) => AuthorRepository.Query(db,
                                                                new SqlBuilder()
                                                                .Join<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id)
                                                                .Where<Author>(a => a.AuthorMetadataId == book.AuthorMetadataId)).SingleOrDefault(),
                          a => a.AuthorMetadataId > 0);

            Mapper.Entity<Edition>("Editions").RegisterModel()
                .HasOne(r => r.Book, r => r.BookId)
                .LazyLoad(x => x.BookFiles,
                          (db, book) => db.Query<BookFile>(new SqlBuilder()
                                                           .Join<BookFile, Book>((l, r) => l.EditionId == r.Id)
                                                           .Where<Book>(b => b.Id == book.Id)).ToList(),
                          b => b.Id > 0);

            Mapper.Entity<BookFile>("BookFiles").RegisterModel()
                .HasOne(f => f.Edition, f => f.EditionId)
                .LazyLoad(x => x.Author,
                          (db, f) => AuthorRepository.Query(db,
                                                            new SqlBuilder()
                                                            .Join<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id)
                                                            .Join<Author, Book>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
                                                            .Where<Book>(a => a.Id == f.EditionId)).SingleOrDefault(),
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
            Mapper.Entity<OtherExtraFile>("ExtraFiles").RegisterModel();

            Mapper.Entity<PendingRelease>("PendingReleases").RegisterModel()
                  .Ignore(e => e.RemoteBook);

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

            Mapper.Entity<CachedHttpResponse>("HttpResponse").RegisterModel();
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
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedBookInfo>());
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
