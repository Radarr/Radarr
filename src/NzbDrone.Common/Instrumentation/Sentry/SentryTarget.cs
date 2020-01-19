using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using SharpRaven;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    [Target("Sentry")]
    public class SentryTarget : TargetWithLayout
    {
        private readonly RavenClient _client;

        // don't report uninformative SQLite exceptions
        // busy/locked are benign https://forums.sonarr.tv/t/owin-sqlite-error-5-database-is-locked/5423/11
        // The others will be user configuration problems and silt up Sentry
        private static readonly HashSet<SQLiteErrorCode> FilteredSQLiteErrors = new HashSet<SQLiteErrorCode>
        {
            SQLiteErrorCode.Busy,
            SQLiteErrorCode.Locked,
            SQLiteErrorCode.Perm,
            SQLiteErrorCode.ReadOnly,
            SQLiteErrorCode.IoErr,
            SQLiteErrorCode.Corrupt,
            SQLiteErrorCode.Full,
            SQLiteErrorCode.CantOpen,
            SQLiteErrorCode.Auth
        };

        // use string and not Type so we don't need a reference to the project
        // where these are defined
        private static readonly HashSet<string> FilteredExceptionTypeNames = new HashSet<string>
        {
            // UnauthorizedAccessExceptions will just be user configuration issues
            "UnauthorizedAccessException",

            // Filter out people stuck in boot loops
            "CorruptDatabaseException",

            // This also filters some people in boot loops
            "TinyIoCResolutionException"
        };

        public static readonly List<string> FilteredExceptionMessages = new List<string>
        {
            // Swallow the many, many exceptions flowing through from Jackett
            "Jackett.Common.IndexerException",

            // Fix openflixr being stupid with permissions
            "openflixr"
        };

        // exception types in this list will additionally have the exception message added to the
        // sentry fingerprint.  Make sure that this message doesn't vary by exception
        // (e.g. containing a path or a url) so that the sentry grouping is sensible
        private static readonly HashSet<string> IncludeExceptionMessageTypes = new HashSet<string>
        {
            "SQLiteException"
        };

        private static readonly IDictionary<LogLevel, ErrorLevel> LoggingLevelMap = new Dictionary<LogLevel, ErrorLevel>
        {
            {LogLevel.Debug, ErrorLevel.Debug},
            {LogLevel.Error, ErrorLevel.Error},
            {LogLevel.Fatal, ErrorLevel.Fatal},
            {LogLevel.Info, ErrorLevel.Info},
            {LogLevel.Trace, ErrorLevel.Debug},
            {LogLevel.Warn, ErrorLevel.Warning},
        };

        private static readonly IDictionary<LogLevel, BreadcrumbLevel> BreadcrumbLevelMap = new Dictionary<LogLevel, BreadcrumbLevel>
        {
            { LogLevel.Debug, BreadcrumbLevel.Debug },
            { LogLevel.Error, BreadcrumbLevel.Error },
            { LogLevel.Fatal, BreadcrumbLevel.Critical },
            { LogLevel.Info, BreadcrumbLevel.Info },
            { LogLevel.Trace, BreadcrumbLevel.Debug },
            { LogLevel.Warn, BreadcrumbLevel.Warning },
        };

        private readonly SentryDebounce _debounce;
        private bool _unauthorized;

        public bool FilterEvents { get; set; }


        public SentryTarget(string dsn)
        {
            _client = new RavenClient(new Dsn(dsn), new RadarrJsonPacketFactory(), new SentryRequestFactory(), new MachineNameUserFactory())
            {
                Compression = true,
                Environment = RuntimeInfo.IsProduction ? "production" : "development",
                Release = BuildInfo.Release,
                ErrorOnCapture = OnError
            };


            _client.Tags.Add("osfamily", OsInfo.Os.ToString());
            _client.Tags.Add("runtime", PlatformInfo.PlatformName);
            _client.Tags.Add("culture", Thread.CurrentThread.CurrentCulture.Name);
            _client.Tags.Add("branch", BuildInfo.Branch);
            _client.Tags.Add("version", BuildInfo.Version.ToString());

            _debounce = new SentryDebounce();

            FilterEvents = true;
        }

        private void OnError(Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null)
            {
                var response = webException.Response as HttpWebResponse;
                var statusCode = response?.StatusCode;
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    _unauthorized = true;
                    _debounce.Clear();
                }
            }

            InternalLogger.Error(ex, "Unable to send error to Sentry");
        }

        private static List<string> GetFingerPrint(LogEventInfo logEvent)
        {
            if (logEvent.Properties.ContainsKey("Sentry"))
            {
                return ((string[])logEvent.Properties["Sentry"]).ToList();
            }

            var fingerPrint = new List<string>
            {
                logEvent.Level.Ordinal.ToString(),
                logEvent.LoggerName,
                logEvent.Message
            };

            var ex = logEvent.Exception;

            if (ex != null)
            {
                fingerPrint.Add(ex.GetType().FullName);
                fingerPrint.Add(ex.TargetSite.ToString());
                if (ex.InnerException != null)
                {
                    fingerPrint.Add(ex.InnerException.GetType().FullName);
                }
                else if (IncludeExceptionMessageTypes.Contains(ex.GetType().Name))
                {
                    fingerPrint.Add(ex?.Message);
                }
            }

            return fingerPrint;
        }

        private bool IsSentryMessage(LogEventInfo logEvent)
        {
            if (logEvent.Properties.ContainsKey("Sentry"))
            {
                return logEvent.Properties["Sentry"] != null;
            }

            if (logEvent.Level >= LogLevel.Error && logEvent.Exception != null)
            {
                if (FilterEvents)
                {
                    var sqlEx = logEvent.Exception as SQLiteException;
                    if (sqlEx != null && FilteredSQLiteErrors.Contains(sqlEx.ResultCode))
                    {
                        return false;
                    }

                    if (FilteredExceptionTypeNames.Contains(logEvent.Exception.GetType().Name))
                    {
                        return false;
                    }

                    if (FilteredExceptionMessages.Any(x => logEvent.Exception.Message.Contains(x)))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


        protected override void Write(LogEventInfo logEvent)
        {
            if (_unauthorized)
            {
                return;
            }

            try
            {
                _client.AddTrail(new Breadcrumb(logEvent.LoggerName) { Level = BreadcrumbLevelMap[logEvent.Level], Message = logEvent.FormattedMessage });

                // don't report non-critical events without exceptions
                if (!IsSentryMessage(logEvent))
                {
                    return;
                }

                var fingerPrint = GetFingerPrint(logEvent);
                if (!_debounce.Allowed(fingerPrint))
                {
                    return;
                }

                var extras = logEvent.Properties.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
                extras.Remove("Sentry");
                _client.Logger = logEvent.LoggerName;

                if (logEvent.Exception != null)
                {
                    foreach (DictionaryEntry data in logEvent.Exception.Data)
                    {
                        extras.Add(data.Key.ToString(), data.Value.ToString());
                    }
                }

                var sentryMessage = new SentryMessage(logEvent.Message, logEvent.Parameters);

                var sentryEvent = new SentryEvent(logEvent.Exception)
                {
                    Level = LoggingLevelMap[logEvent.Level],
                    Message = sentryMessage,
                    Extra = extras,
                    Fingerprint =
                    {
                        logEvent.Level.ToString(),
                        logEvent.LoggerName,
                        logEvent.Message
                    }
                };

                if (logEvent.Exception != null)
                {
                    sentryEvent.Fingerprint.Add(logEvent.Exception.GetType().FullName);
                }

                if (logEvent.Properties.ContainsKey("Sentry"))
                {
                    sentryEvent.Fingerprint.Clear();
                    Array.ForEach((string[])logEvent.Properties["Sentry"], sentryEvent.Fingerprint.Add);
                }

                var runTimeVersion = Environment.GetEnvironmentVariable("RUNTIME_VERSION");

                sentryEvent.Tags.Add("runtime_version", $"{PlatformInfo.PlatformName} {runTimeVersion}");

                _client.Capture(sentryEvent);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
