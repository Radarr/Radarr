using Nancy.Routing;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Api.System
{
    public class SystemModule : NzbDroneApiModule
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IPlatformInfo _platformInfo;
        private readonly IOsInfo _osInfo;
        private readonly IRouteCacheProvider _routeCacheProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IMainDatabase _database;
        private readonly ILifecycleService _lifecycleService;

        public SystemModule(IAppFolderInfo appFolderInfo,
                            IRuntimeInfo runtimeInfo,
                            IPlatformInfo platformInfo,
                            IOsInfo osInfo,
                            IRouteCacheProvider routeCacheProvider,
                            IConfigFileProvider configFileProvider,
                            IMainDatabase database,
                            ILifecycleService lifecycleService)
            : base("system")
        {
            _appFolderInfo = appFolderInfo;
            _runtimeInfo = runtimeInfo;
            _platformInfo = platformInfo;
            _osInfo = osInfo;
            _routeCacheProvider = routeCacheProvider;
            _configFileProvider = configFileProvider;
            _database = database;
            _lifecycleService = lifecycleService;
            Get("/status", x => GetStatus());
            Get("/routes", x => GetRoutes());
            Post("/shutdown", x => Shutdown());
            Post("/restart", x => Restart());
        }

        private object GetStatus()
        {
            return new
            {
                Version = BuildInfo.Version.ToString(),
                BuildTime = BuildInfo.BuildDateTime,
                IsDebug = BuildInfo.IsDebug,
                IsProduction = RuntimeInfo.IsProduction,
                IsAdmin = _runtimeInfo.IsAdmin,
                IsUserInteractive = RuntimeInfo.IsUserInteractive,
                StartupPath = _appFolderInfo.StartUpFolder,
                AppData = _appFolderInfo.GetAppDataPath(),
                OsName = _osInfo.Name,
                OsVersion = _osInfo.Version,
                IsNetCore = PlatformInfo.IsNetCore,
                IsMono = PlatformInfo.IsMono,
                IsLinux = OsInfo.IsLinux,
                IsOsx = OsInfo.IsOsx,
                IsWindows = OsInfo.IsWindows,
                Branch = _configFileProvider.Branch,
                Authentication = _configFileProvider.AuthenticationMethod,
                SqliteVersion = _database.Version,
                MigrationVersion = _database.Migration,
                UrlBase = _configFileProvider.UrlBase,
                RuntimeVersion = _platformInfo.Version,
                RuntimeName = PlatformInfo.Platform
            };
        }

        private object GetRoutes()
        {
            return _routeCacheProvider.GetCache().Values;
        }

        private object Shutdown()
        {
            _lifecycleService.Shutdown();
            return "";
        }

        private object Restart()
        {
            _lifecycleService.Restart();
            return "";
        }
    }
}
