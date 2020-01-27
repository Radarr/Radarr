﻿using System;
using System.Windows.Forms;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.SysTray;
using Radarr.Host;

namespace NzbDrone
{
    public static class WindowsApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(WindowsApp));

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupContext(args);

                NzbDroneLogger.Register(startupArgs, false, true);

                Bootstrap.Start(startupArgs, new MessageBoxUserAlert(), container =>
                {
                    container.Register<ISystemTrayApp, SystemTrayApp>();
                    var trayApp = container.Resolve<ISystemTrayApp>();
                    trayApp.Start();
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "EPIC FAIL: " + e.Message);
                var message = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                MessageBox.Show(text: message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }
    }
}
