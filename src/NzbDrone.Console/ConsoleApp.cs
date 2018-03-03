using System;
using System.Net.Sockets;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using Radarr.Host;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ConsoleApp));

        private enum ExitCodes : int
        {
            Normal = 0,
            UnknownFailure = 1,
            RecoverableFailure = 2
        }

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupContext(args);
                try
                {
                    NzbDroneLogger.Register(startupArgs, false, true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("NLog Exception: " + ex.ToString());
                    throw;
                }
                Bootstrap.Start(startupArgs, new ConsoleAlerts());
            }
            catch (SocketException e)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(e.Message + ". This can happen if another instance of Radarr is already running another application is using the same port (default: 7878) or the user has insufficient permissions");
                Exit(ExitCodes.RecoverableFailure);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.Fatal(e, "EPIC FAIL!");
                Exit(ExitCodes.UnknownFailure);
            }

            Logger.Info("Exiting main.");

            Exit(ExitCodes.Normal);
        }

        private static void Exit(ExitCodes exitCode)
        {
            LogManager.Shutdown();

            if (exitCode != ExitCodes.Normal)
            {
                System.Console.WriteLine("Press enter to exit...");

                System.Threading.Thread.Sleep(1000);

                // Please note that ReadLine silently succeeds if there is no console, KeyAvailable does not.
                System.Console.ReadLine();
            }

            Environment.Exit((int)exitCode);
        }
    }
}
