using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFilters;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Restrictions;
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

            Mapper.Entity<NetImportDefinition>("NetImport").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.ListType)
                  .Ignore(i => i.Enable);

            Mapper.Entity<NotificationDefinition>("Notifications").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.SupportsOnGrab)
                  .Ignore(i => i.SupportsOnDownload)
                  .Ignore(i => i.SupportsOnUpgrade)
                  .Ignore(i => i.SupportsOnRename)
                  .Ignore(i => i.SupportsOnHealthIssue);

            Mapper.Entity<MetadataDefinition>("Metadata").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Tags);

            Mapper.Entity<DownloadClientDefinition>("DownloadClients").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Protocol)
                  .Ignore(d => d.Tags);

            Mapper.Entity<History.History>("History").RegisterModel();

            Mapper.Entity<MovieFile>("MovieFiles").RegisterModel()
                  .Ignore(f => f.Path);

            Mapper.Entity<Movie>("Movies").RegisterModel()
                  .Ignore(s => s.RootFolderPath);

            Mapper.Entity<AlternativeTitle>("AlternativeTitles").RegisterModel();

            Mapper.Entity<Credit>("Credits").RegisterModel();

            Mapper.Entity<ImportExclusion>("ImportExclusions").RegisterModel();

            Mapper.Entity<QualityDefinition>("QualityDefinitions").RegisterModel()
                  .Ignore(d => d.GroupName)
                  .Ignore(d => d.Weight);

            Mapper.Entity<CustomFormat>("CustomFormats").RegisterModel();

            Mapper.Entity<Profile>("Profiles").RegisterModel();
            Mapper.Entity<Log>("Logs").RegisterModel();
            Mapper.Entity<NamingConfig>("NamingConfig").RegisterModel();
            Mapper.Entity<Blacklist>("Blacklist").RegisterModel();
            Mapper.Entity<MetadataFile>("MetadataFiles").RegisterModel();
            Mapper.Entity<SubtitleFile>("SubtitleFiles").RegisterModel();
            Mapper.Entity<OtherExtraFile>("ExtraFiles").RegisterModel();

            Mapper.Entity<PendingRelease>("PendingReleases").RegisterModel()
                  .Ignore(e => e.RemoteMovie);

            Mapper.Entity<RemotePathMapping>("RemotePathMappings").RegisterModel();
            Mapper.Entity<Tag>("Tags").RegisterModel();
            Mapper.Entity<Restriction>("Restrictions").RegisterModel();

            Mapper.Entity<DelayProfile>("DelayProfiles").RegisterModel();
            Mapper.Entity<User>("Users").RegisterModel();
            Mapper.Entity<CommandModel>("Commands").RegisterModel()
                  .Ignore(c => c.Message);

            Mapper.Entity<IndexerStatus>("IndexerStatus").RegisterModel();
            Mapper.Entity<DownloadClientStatus>("DownloadClientStatus").RegisterModel();

            Mapper.Entity<CustomFilter>("CustomFilters").RegisterModel();
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();
            RegisterProviderSettingConverter();

            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(new DapperUtcConverter());
            SqlMapper.AddTypeHandler(new DapperQualityIntConverter());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileQualityItem>>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileFormatItem>>(new CustomFormatIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ICustomFormatSpecification>>(new CustomFormatSpecificationListConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<QualityModel>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<Dictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<IDictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<int>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<KeyValuePair<string, int>>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<KeyValuePair<string, int>>());
            SqlMapper.AddTypeHandler(new DapperLanguageIntConverter());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<Language>>(new LanguageIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedMovieInfo>(new QualityIntConverter(), new LanguageIntConverter()));
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
