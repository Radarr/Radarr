using System;
using System.Reflection;
using System.Threading;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Processes;
using NzbDrone.Common.Security;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;

namespace Radarr.Host
{
    public static class Bootstrap
    {
        private static IContainer _container;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Bootstrap));

        public static void Start(StartupContext startupContext, IUserAlert userAlert, Action<IContainer> startCallback = null)
        {
            try
            {
                SecurityProtocolPolicy.Register();
                X509CertificateValidationPolicy.Register();

                Logger.Info("Starting Radarr - {0} - Version {1}", Assembly.GetCallingAssembly().Location, Assembly.GetExecutingAssembly().GetName().Version);

                if (!PlatformValidation.IsValidate(userAlert))
                {
                    throw new TerminateApplicationException("Missing system requirements");
                }

                _container = MainAppContainerBuilder.BuildContainer(startupContext);
                _container.Resolve<IAppFolderFactory>().Register();
                _container.Resolve<IProvidePidFile>().Write();

                var appMode = GetApplicationMode(startupContext);

                Start(appMode, startupContext);
                
                _container.Resolve<ICancelHandler>().Attach();

                if (startCallback != null)
                {
                    startCallback(_container);
                }
                else
                {
                    SpinToExit(appMode);
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

        private static void Start(ApplicationModes applicationModes, StartupContext startupContext)
        {
            _container.Resolve<ReconfigureLogging>().Reconfigure();

            if (!IsInUtilityMode(applicationModes))
            {
                if (startupContext.Flags.Contains(StartupContext.RESTART))
                {
                    Thread.Sleep(2000);
                }

                EnsureSingleInstance(applicationModes == ApplicationModes.Service, startupContext);
            }
            
            _container.Resolve<Router>().Route(applicationModes);
        }

        private static void SpinToExit(ApplicationModes applicationModes)
        {
            if (IsInUtilityMode(applicationModes))
            {
                return;
            }

            _container.Resolve<IWaitForExit>().Spin();
        }

        private static void EnsureSingleInstance(bool isService, IStartupContext startupContext)
        {
            var instancePolicy = _container.Resolve<ISingleInstancePolicy>();

            if (startupContext.Flags.Contains(StartupContext.TERMINATE))
            {
                instancePolicy.KillAllOtherInstance();
            }
            else if (startupContext.Args.ContainsKey(StartupContext.APPDATA))
            {
                instancePolicy.WarnIfAlreadyRunning();
            }
            else if (isService)
            {
                instancePolicy.KillAllOtherInstance();
            }
            else
            {
                instancePolicy.PreventStartIfAlreadyRunning();
            }
        }

        private static ApplicationModes GetApplicationMode(IStartupContext startupContext)
        {
            if (startupContext.Flags.Contains(StartupContext.HELP))
            {
                return ApplicationModes.Help;
            }

            if (OsInfo.IsWindows && startupContext.InstallService)
            {
                return ApplicationModes.InstallService;
            }

            if (OsInfo.IsWindows && startupContext.UninstallService)
            {
                return ApplicationModes.UninstallService;
            }

            if (_container.Resolve<IRuntimeInfo>().IsWindowsService)
            {
                return ApplicationModes.Service;
            }

            return ApplicationModes.Interactive;
        }

        private static bool IsInUtilityMode(ApplicationModes applicationMode)
        {
            switch (applicationMode)
            {
                case ApplicationModes.InstallService:
                case ApplicationModes.UninstallService:
                case ApplicationModes.Help:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
