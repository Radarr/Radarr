using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using RestSharp;

namespace NzbDrone.Test.Common
{
    public class NzbDroneRunner
    {
        private readonly IProcessProvider _processProvider;
        private readonly IRestClient _restClient;
        private Process _nzbDroneProcess;
        private List<string> _startupLog;

        public string AppData { get; private set; }
        public string ApiKey { get; private set; }
        public int Port { get; private set; }

        public NzbDroneRunner(Logger logger, int port = 8787)
        {
            _processProvider = new ProcessProvider(logger);
            _restClient = new RestClient($"http://localhost:{port}/api/v1");

            Port = port;
        }

        public void Start()
        {
            AppData = Path.Combine(TestContext.CurrentContext.TestDirectory, "_intg_" + TestBase.GetUID());
            Directory.CreateDirectory(AppData);

            GenerateConfigFile();

            string readarrConsoleExe;
            if (OsInfo.IsWindows)
            {
                readarrConsoleExe = "Readarr.Console.exe";
            }
            else if (PlatformInfo.IsMono)
            {
                readarrConsoleExe = "Readarr.exe";
            }
            else
            {
                readarrConsoleExe = "Readarr";
            }

            _startupLog = new List<string>();
            if (BuildInfo.IsDebug)
            {
                var frameworkFolder = PlatformInfo.IsNetCore ? "netcoreapp3.1" : "net462";
                Start(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "_output", frameworkFolder, readarrConsoleExe));
            }
            else
            {
                Start(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "bin", readarrConsoleExe));
            }

            while (true)
            {
                _nzbDroneProcess.Refresh();

                if (_nzbDroneProcess.HasExited)
                {
                    TestContext.Progress.WriteLine("Readarr has exited unexpectedly");
                    Thread.Sleep(2000);
                    var output = _startupLog.Join(Environment.NewLine);
                    Assert.Fail("Process has exited: ExitCode={0} Output={1}", _nzbDroneProcess.ExitCode, output);
                }

                var request = new RestRequest("system/status");
                request.AddHeader("Authorization", ApiKey);
                request.AddHeader("X-Api-Key", ApiKey);

                var statusCall = _restClient.Get(request);

                if (statusCall.ResponseStatus == ResponseStatus.Completed)
                {
                    _startupLog = null;
                    TestContext.Progress.WriteLine($"Readarr {Port} is started. Running Tests");
                    return;
                }

                TestContext.Progress.WriteLine("Waiting for Readarr to start. Response Status : {0}  [{1}] {2}", statusCall.ResponseStatus, statusCall.StatusDescription, statusCall.ErrorException.Message);

                Thread.Sleep(500);
            }
        }

        public void Kill()
        {
            try
            {
                if (_nzbDroneProcess != null)
                {
                    _nzbDroneProcess.Refresh();
                    if (_nzbDroneProcess.HasExited)
                    {
                        var log = File.ReadAllLines(Path.Combine(AppData, "logs", "Readarr.trace.txt"));
                        var output = log.Join(Environment.NewLine);
                        TestContext.Progress.WriteLine("Process has exited prematurely: ExitCode={0} Output:\n{1}", _nzbDroneProcess.ExitCode, output);
                    }

                    _processProvider.Kill(_nzbDroneProcess.Id);
                }
            }
            catch (InvalidOperationException)
            {
                // May happen if the process closes while being closed
            }

            TestBase.DeleteTempFolder(AppData);
        }

        public void KillAll()
        {
            try
            {
                if (_nzbDroneProcess != null)
                {
                    _processProvider.Kill(_nzbDroneProcess.Id);
                }

                _processProvider.KillAll(ProcessProvider.READARR_CONSOLE_PROCESS_NAME);
                _processProvider.KillAll(ProcessProvider.READARR_PROCESS_NAME);
            }
            catch (InvalidOperationException)
            {
                // May happen if the process closes while being closed
            }

            TestBase.DeleteTempFolder(AppData);
        }

        private void Start(string outputNzbdroneConsoleExe)
        {
            TestContext.Progress.WriteLine("Starting instance from {0} on port {1}", outputNzbdroneConsoleExe, Port);

            var args = "-nobrowser -nosingleinstancecheck -data=\"" + AppData + "\"";
            _nzbDroneProcess = _processProvider.Start(outputNzbdroneConsoleExe, args, null, OnOutputDataReceived, OnOutputDataReceived);
        }

        private void OnOutputDataReceived(string data)
        {
            TestContext.Progress.WriteLine($" [{Port}] > " + data);

            if (_startupLog != null)
            {
                _startupLog.Add(data);
            }

            if (data.Contains("Press enter to exit"))
            {
                _nzbDroneProcess.StandardInput.WriteLine(" ");
            }
        }

        private void GenerateConfigFile()
        {
            var configFile = Path.Combine(AppData, "config.xml");

            // Generate and set the api key so we don't have to poll the config file
            var apiKey = Guid.NewGuid().ToString().Replace("-", "");

            var xDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ConfigFileProvider.CONFIG_ELEMENT_NAME,
                             new XElement(nameof(ConfigFileProvider.ApiKey), apiKey),
                             new XElement(nameof(ConfigFileProvider.LogLevel), "trace"),
                             new XElement(nameof(ConfigFileProvider.AnalyticsEnabled), false),
                             new XElement(nameof(ConfigFileProvider.Port), Port)));

            var data = xDoc.ToString();

            File.WriteAllText(configFile, data);

            ApiKey = apiKey;
        }
    }
}
