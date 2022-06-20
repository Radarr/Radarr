using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using NLog;
using NzbDrone.Common.Composition.Extensions;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Host;

namespace Radarr.Host
{
    public static class Bootstrap
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Bootstrap));

        public static readonly List<string> ASSEMBLIES = new List<string>
        {
            "Radarr.Host",
            "Radarr.Core",
            "Radarr.SignalR",
            "Radarr.Api.V3",
            "Radarr.Http"
        };

        public static void Start(string[] args, Action<IHostBuilder> trayCallback = null)
        {
            try
            {
                Logger.Info("Starting Radarr - {0} - Version {1}",
                            Process.GetCurrentProcess().MainModule.FileName,
                            Assembly.GetExecutingAssembly().GetName().Version);

                var startupContext = new StartupContext(args);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var appMode = GetApplicationMode(startupContext);

                switch (appMode)
                {
                    case ApplicationModes.Service:
                    {
                        Logger.Debug("Service selected");

                        CreateConsoleHostBuilder(args, startupContext).UseWindowsService().Build().Run();
                        break;
                    }

                    case ApplicationModes.Interactive:
                    {
                        Logger.Debug(trayCallback != null ? "Tray selected" : "Console selected");
                        var builder = CreateConsoleHostBuilder(args, startupContext);

                        if (trayCallback != null)
                        {
                            trayCallback(builder);
                        }

                        builder.Build().Run();
                        break;
                    }

                    // Utility mode
                    default:
                    {
                        new HostBuilder()
                            .UseServiceProviderFactory(new DryIocServiceProviderFactory(new Container(rules => rules.WithNzbDroneRules())))
                            .ConfigureContainer<IContainer>(c =>
                            {
                                c.AutoAddServices(ASSEMBLIES)
                                    .AddNzbDroneLogger()
                                    .AddDatabase()
                                    .AddStartupContext(startupContext)
                                    .Resolve<UtilityModeRouter>()
                                    .Route(appMode);
                            }).Build();
                        break;
                    }
                }
            }
            catch (InvalidConfigFileException ex)
            {
                throw new RadarrStartupException(ex);
            }
            catch (TerminateApplicationException e)
            {
                Logger.Info(e.Message);
                LogManager.Configuration = null;
            }
        }

        public static IHostBuilder CreateConsoleHostBuilder(string[] args, StartupContext context)
        {
            return new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .AddConfig(context)
                .UseServiceProviderFactory(new DryIocServiceProviderFactory(new Container(rules => rules.WithNzbDroneRules())))
                .ConfigureContainer<IContainer>(c =>
                {
                    c.AutoAddServices(ASSEMBLIES)
                        .AddNzbDroneLogger()
                        .AddDatabase()
                        .AddStartupContext(context);
                })
                .ConfigureWebHost(builder =>
                {
                    builder.UseKestrel();
                    builder.UseStartup<Startup>();
                });
        }

        public static ApplicationModes GetApplicationMode(IStartupContext startupContext)
        {
            if (startupContext.Help)
            {
                return ApplicationModes.Help;
            }

            if (OsInfo.IsWindows && startupContext.RegisterUrl)
            {
                return ApplicationModes.RegisterUrl;
            }

            if (OsInfo.IsWindows && startupContext.InstallService)
            {
                return ApplicationModes.InstallService;
            }

            if (OsInfo.IsWindows && startupContext.UninstallService)
            {
                return ApplicationModes.UninstallService;
            }

            // IsWindowsService can throw sometimes, so wrap it
            var isWindowsService = false;
            try
            {
                isWindowsService = WindowsServiceHelpers.IsWindowsService();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get service status");
            }

            if (OsInfo.IsWindows && isWindowsService)
            {
                return ApplicationModes.Service;
            }

            return ApplicationModes.Interactive;
        }

        private static IHostBuilder AddConfig(this IHostBuilder builder, StartupContext context)
        {
            var appFolder = new AppFolderInfo(context);
            return builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddXmlFile(appFolder.GetConfigPath(), optional: true, reloadOnChange: true)
                .AddInMemoryCollection(new List<KeyValuePair<string, string>> { new ("dataProtectionFolder", appFolder.GetDataProtectionPath()) })
                .AddEnvironmentVariables($"{BuildInfo.AppName}:");
            });
        }
    }
}
