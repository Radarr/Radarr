﻿using System;
using System.Collections.Generic;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tags;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;
using System;
using System.Collections.Generic;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tags;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.ImportExclusions;

namespace NzbDrone.Core.Datastore
{
    public static class TableMapping
    {
        private static readonly FluentMappings Mapper = new FluentMappings(true);

        public static void Map()
        {
            RegisterMappers();

            Mapper.Entity<Config>().RegisterModel("Config");
            Mapper.Entity<RootFolder>().RegisterModel("RootFolders").Ignore(r => r.FreeSpace);
            Mapper.Entity<ScheduledTask>().RegisterModel("ScheduledTasks");

            Mapper.Entity<IndexerDefinition>().RegisterDefinition("Indexers")
                  .Ignore(i => i.Enable)
                  .Ignore(i => i.Protocol)
                  .Ignore(i => i.SupportsRss)
                  .Ignore(i => i.SupportsSearch);

            Mapper.Entity<NetImportDefinition>().RegisterDefinition("NetImport")
                .Ignore(i => i.Enable)
                .Relationship()
                .HasOne(n => n.Profile, n => n.ProfileId);

            Mapper.Entity<NotificationDefinition>().RegisterDefinition("Notifications")
                  .Ignore(i => i.SupportsOnGrab)
                  .Ignore(i => i.SupportsOnDownload)
                  .Ignore(i => i.SupportsOnUpgrade)
                  .Ignore(i => i.SupportsOnRename);
            
            Mapper.Entity<MetadataDefinition>().RegisterDefinition("Metadata");

            Mapper.Entity<DownloadClientDefinition>().RegisterDefinition("DownloadClients")
                  .Ignore(d => d.Protocol);

            Mapper.Entity<SceneMapping>().RegisterModel("SceneMappings");

            Mapper.Entity<History.History>().RegisterModel("History")
                  .AutoMapChildModels();

            Mapper.Entity<Series>().RegisterModel("Series")
                  .Ignore(s => s.RootFolderPath)
                  .Relationship()
                  .HasOne(s => s.Profile, s => s.ProfileId);



            Mapper.Entity<EpisodeFile>().RegisterModel("EpisodeFiles")
                  .Ignore(f => f.Path)
                  .Relationships.AutoMapICollectionOrComplexProperties()
                  .For("Episodes")
                  .LazyLoad(condition: parent => parent.Id > 0, 
                            query: (db, parent) => db.Query<Episode>().Where(c => c.EpisodeFileId == parent.Id).ToList())
                  .HasOne(file => file.Series, file => file.SeriesId);

                Mapper.Entity<MovieFile>().RegisterModel("MovieFiles")
                .Ignore(f => f.Path)
                .Relationships.AutoMapICollectionOrComplexProperties()
                .For("Movie")
                .LazyLoad(condition: parent => parent.Id > 0,
                            query: (db, parent) => db.Query<Movie>().Where(c => c.MovieFileId == parent.Id).ToList())
                .HasOne(file => file.Movie, file => file.MovieId);

            Mapper.Entity<Movie>().RegisterModel("Movies")
                .Ignore(s => s.RootFolderPath)
                .Ignore(m => m.Actors)
                .Ignore(m => m.Genres)
//                .Ignore(m => m.Tags)
                .Relationship()
                .HasOne(s => s.Profile, s => s.ProfileId);
                //.HasOne(m => m.MovieFile, m => m.MovieFileId);

            Mapper.Entity<AlternativeTitle>().RegisterModel("AlternativeTitles")
                .For(t => t.Id)
                .SetAltName("AltTitle_Id")
                .Relationship()
                .HasOne(t => t.Movie, t => t.MovieId);
                

            Mapper.Entity<ImportExclusion>().RegisterModel("ImportExclusions");
       

            Mapper.Entity<Episode>().RegisterModel("Episodes")
                  .Ignore(e => e.SeriesTitle)
                  .Ignore(e => e.Series)
                  .Ignore(e => e.HasFile)
                  .Relationship()
                  .HasOne(episode => episode.EpisodeFile, episode => episode.EpisodeFileId);

            Mapper.Entity<QualityDefinition>().RegisterModel("QualityDefinitions")
                  .Ignore(d => d.Weight);

            Mapper.Entity<Profile>().RegisterModel("Profiles");
            Mapper.Entity<Log>().RegisterModel("Logs");
            Mapper.Entity<NamingConfig>().RegisterModel("NamingConfig");
            Mapper.Entity<SeasonStatistics>().MapResultSet();
            Mapper.Entity<Blacklist>().RegisterModel("Blacklist");
            Mapper.Entity<MetadataFile>().RegisterModel("MetadataFiles");
            Mapper.Entity<SubtitleFile>().RegisterModel("SubtitleFiles");
            Mapper.Entity<OtherExtraFile>().RegisterModel("ExtraFiles");

            Mapper.Entity<PendingRelease>().RegisterModel("PendingReleases")
                  .Ignore(e => e.RemoteEpisode);

            Mapper.Entity<RemotePathMapping>().RegisterModel("RemotePathMappings");
            Mapper.Entity<Tag>().RegisterModel("Tags");
            Mapper.Entity<Restriction>().RegisterModel("Restrictions");

            Mapper.Entity<DelayProfile>().RegisterModel("DelayProfiles");
            Mapper.Entity<User>().RegisterModel("Users");
            Mapper.Entity<CommandModel>().RegisterModel("Commands")
                .Ignore(c => c.Message);

            Mapper.Entity<IndexerStatus>().RegisterModel("IndexerStatus");
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
            MapRepository.Instance.RegisterTypeConverter(typeof(List<ProfileQualityItem>), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(QualityModel), new EmbeddedDocumentConverter(new QualityIntConverter()));
            MapRepository.Instance.RegisterTypeConverter(typeof(Dictionary<string, string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<int>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(List<string>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ParsedEpisodeInfo), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(ParsedMovieInfo), new EmbeddedDocumentConverter());
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