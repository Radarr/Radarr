using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public interface IFingerprintingService
    {
        bool IsSetup();
        Version FpcalcVersion();
        void Lookup(List<LocalBook> tracks, double threshold);
    }

    public class AcoustId
    {
        public double Duration { get; set; }
        public string Fingerprint { get; set; }
    }

    public class FingerprintingService : IFingerprintingService
    {
        private const string _acoustIdUrl = "https://api.acoustid.org/v2/lookup";
        private const string _acoustIdApiKey = "QANd68ji1L";
        private const int _fingerprintingTimeout = 10000;

        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _customerRequestBuilder;
        private readonly ICached<AcoustId> _cache;

        private readonly string _fpcalcPath;
        private readonly Version _fpcalcVersion;
        private readonly string _fpcalcArgs;

        public FingerprintingService(Logger logger,
                                     IHttpClient httpClient,
                                     ICacheManager cacheManager)
        {
            _logger = logger;
            _httpClient = httpClient;
            _cache = cacheManager.GetCache<AcoustId>(GetType());

            _customerRequestBuilder = new HttpRequestBuilder(_acoustIdUrl).CreateFactory();

            // An exception here will cause Readarr to fail to start, so catch any errors
            try
            {
                _fpcalcPath = GetFpcalcPath();

                if (_fpcalcPath.IsNotNullOrWhiteSpace())
                {
                    _fpcalcVersion = GetFpcalcVersion();
                    _fpcalcArgs = GetFpcalcArgs();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Somthing went wrong detecting fpcalc");
            }
        }

        public bool IsSetup() => _fpcalcPath.IsNotNullOrWhiteSpace();
        public Version FpcalcVersion() => _fpcalcVersion;

        private string GetFpcalcPath()
        {
            string path = null;

            // Take the fpcalc from the install directory if it exists
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fpcalc");
            if (OsInfo.IsWindows)
            {
                path += ".exe";
            }

            if (File.Exists(path))
            {
                return path;
            }

            // Otherwise search path for a candidate and check it works
            if (OsInfo.IsLinux)
            {
                // must be on users path on Linux
                path = "fpcalc";

                // check that the command exists
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "-version";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;

                try
                {
                    p.Start();

                    // To avoid deadlocks, always read the output stream first and then wait.
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(1000);
                }
                catch
                {
                    _logger.Debug("fpcalc not found");
                    return null;
                }

                return path;
            }
            else
            {
                // on OSX / Windows, we have put fpcalc in the application folder
                _logger.Warn("fpcalc missing from application directory");
                return null;
            }
        }

        private Version GetFpcalcVersion()
        {
            if (_fpcalcPath == null)
            {
                return null;
            }

            Process p = new Process();
            p.StartInfo.FileName = _fpcalcPath;
            p.StartInfo.Arguments = $"-version";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(1000);

            if (p.ExitCode != 0)
            {
                _logger.Warn("Could not get fpcalc version (may be known issue with fpcalc v1.4)");
                return null;
            }

            var versionstring = Regex.Match(output, @"\d\.\d\.\d").Value;
            if (versionstring.IsNullOrWhiteSpace())
            {
                return null;
            }

            var version = new Version(versionstring);
            _logger.Debug($"fpcalc version: {version}");

            return version;
        }

        private string GetFpcalcArgs()
        {
            var args = "";

            if (_fpcalcVersion == null)
            {
                return args;
            }

            if (_fpcalcVersion >= new Version("1.4.0"))
            {
                args = "-json";
            }

            if (_fpcalcVersion >= new Version("1.4.3"))
            {
                args += " -ignore-errors";
            }

            return args;
        }

        public AcoustId ParseFpcalcJsonOutput(string output)
        {
            return Json.Deserialize<AcoustId>(output);
        }

        public AcoustId ParseFpcalcTextOutput(string output)
        {
            var durationstring = Regex.Match(output, @"(?<=DURATION=)[\d\.]+(?=\s)").Value;
            double duration;
            if (durationstring.IsNullOrWhiteSpace() || !double.TryParse(durationstring, out duration))
            {
                return null;
            }

            var fingerprint = Regex.Match(output, @"(?<=FINGERPRINT=)[^\s]+").Value;
            if (fingerprint.IsNullOrWhiteSpace())
            {
                return null;
            }

            return new AcoustId
            {
                Duration = duration,
                Fingerprint = fingerprint
            };
        }

        public AcoustId ParseFpcalcOutput(string output)
        {
            if (output.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (_fpcalcArgs.Contains("-json"))
            {
                return ParseFpcalcJsonOutput(output);
            }
            else
            {
                return ParseFpcalcTextOutput(output);
            }
        }

        public AcoustId GetFingerprint(string file)
        {
            return _cache.Get(file, () => GetFingerprintUncached(file), TimeSpan.FromMinutes(30));
        }

        private AcoustId GetFingerprintUncached(string file)
        {
            if (IsSetup() && File.Exists(file))
            {
                Process p = new Process();
                p.StartInfo.FileName = _fpcalcPath;
                p.StartInfo.Arguments = $"{_fpcalcArgs} \"{file}\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                _logger.Trace("Executing {0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                // see https://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why?lq=1
                // this is most likely overkill...
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                {
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        DataReceivedEventHandler outputHandler = (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };

                        DataReceivedEventHandler errorHandler =  (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        p.OutputDataReceived += outputHandler;
                        p.ErrorDataReceived += errorHandler;

                        p.Start();

                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();

                        if (p.WaitForExit(_fingerprintingTimeout) &&
                            outputWaitHandle.WaitOne(_fingerprintingTimeout) &&
                            errorWaitHandle.WaitOne(_fingerprintingTimeout))
                        {
                            // Process completed.
                            if (p.ExitCode != 0)
                            {
                                _logger.Warn($"fpcalc error: {error}");
                                return null;
                            }
                            else
                            {
                                return ParseFpcalcOutput(output.ToString());
                            }
                        }
                        else
                        {
                            // Timed out.  Remove handlers to avoid object disposed error
                            p.OutputDataReceived -= outputHandler;
                            p.ErrorDataReceived -= errorHandler;

                            _logger.Warn($"fpcalc timed out. {error}");
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        public void Lookup(List<LocalBook> tracks, double threshold)
        {
            if (!IsSetup())
            {
                return;
            }

            Lookup(tracks.Select(x => Tuple.Create(x, GetFingerprint(x.Path))).ToList(), threshold);
        }

        public void Lookup(List<Tuple<LocalBook, AcoustId>> files, double threshold)
        {
            var toLookup = files.Where(x => x.Item2 != null).ToList();
            if (!toLookup.Any())
            {
                return;
            }

            var request = GenerateRequest(toLookup);
            var response = GetResponse(request);
            ParseResponse(response, toLookup, threshold);
        }

        public HttpRequest GenerateRequest(List<Tuple<LocalBook, AcoustId>> toLookup)
        {
            var httpRequest = _customerRequestBuilder.Create()
                .WithRateLimit(0.334)
                .Build();

            var sb = new StringBuilder($"client={_acoustIdApiKey}&format=json&meta=recordingids&batch=1", 2000);
            for (int i = 0; i < toLookup.Count; i++)
            {
                sb.Append($"&duration.{i}={toLookup[i].Item2.Duration:F0}&fingerprint.{i}={toLookup[i].Item2.Fingerprint}");
            }

            // they prefer a gzipped body
            httpRequest.SetContent(Encoding.UTF8.GetBytes(sb.ToString()).Compress());
            httpRequest.Headers.Add("Content-Encoding", "gzip");
            httpRequest.Headers.ContentType = "application/x-www-form-urlencoded";
            httpRequest.SuppressHttpError = true;
            httpRequest.RequestTimeout = TimeSpan.FromSeconds(5);

            return httpRequest;
        }

        public LookupResponse GetResponse(HttpRequest request, int retry = 3)
        {
            HttpResponse<LookupResponse> httpResponse;

            try
            {
                httpResponse = _httpClient.Post<LookupResponse>(request);
            }
            catch (UnexpectedHtmlContentException e)
            {
                _logger.Warn(e, "AcoustId API gave invalid response");
                return retry > 0 ? GetResponse(request, retry - 1) : null;
            }
            catch (Exception e)
            {
                _logger.Warn(e, "AcoustId API lookup failed");
                return null;
            }

            var response = httpResponse.Resource;

            if (httpResponse.HasHttpError || (response != null && response.Status != "ok"))
            {
                if (response?.Error != null)
                {
                    if (response.Error.Code == AcoustIdErrorCode.TooManyRequests && retry > 0)
                    {
                        _logger.Trace($"Too many requests, retrying in 1s");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        return GetResponse(request, retry - 1);
                    }

                    _logger.Debug($"Webservice error {response.Error.Code}: {response.Error.Message}");
                }
                else
                {
                    _logger.Warn("HTTP Error - {0}", httpResponse);
                }

                return null;
            }

            return response;
        }

        private void ParseResponse(LookupResponse response, List<Tuple<LocalBook, AcoustId>> toLookup, double threshold)
        {
            if (response == null)
            {
                return;
            }

            // The API will give errors if fingerprint isn't found or is invalid.
            // We don't want to stop the entire import because the fingerprinting failed
            // so just log and return.
            foreach (var fileResponse in response.Fingerprints)
            {
                if (fileResponse.Results.Count == 0)
                {
                    _logger.Debug("No results for given fingerprint.");
                    continue;
                }

                foreach (var result in fileResponse.Results.Where(x => x.Recordings != null))
                {
                    _logger.Trace("Found: {0}, {1}, {2}", result.Id, result.Score, string.Join(", ", result.Recordings.Select(x => x.Id)));
                }

                var ids = fileResponse.Results.Where(x => x.Score > threshold && x.Recordings != null).SelectMany(y => y.Recordings.Select(z => z.Id)).Distinct().ToList();
                _logger.Trace("All recordings: {0}", string.Join("\n", ids));

                toLookup[fileResponse.index].Item1.AcoustIdResults = ids;
            }

            _logger.Debug("Fingerprinting complete.");

            var serializerSettings = Json.GetSerializerSettings();
            serializerSettings.Formatting = Formatting.None;
            var output = new { Fingerprints = toLookup.Select(x => new { Path = x.Item1.Path, AcoustIdResults = x.Item1.AcoustIdResults }) };
            _logger.Debug($"*** FingerprintingService TestCaseGenerator ***\n{JsonConvert.SerializeObject(output, serializerSettings)}");
        }

        public class LookupResponse
        {
            public string Status { get; set; }
            public LookupError Error { get; set; }
            public List<LookupResultListItem> Fingerprints { get; set; }
        }

        public enum AcoustIdErrorCode
        {
            // https://github.com/acoustid/acoustid-server/blob/f671339ad9ab049c4d4361d3eadb6660a8fe4dda/acoustid/api/errors.py#L10
            UnknownFormat = 1,
            MissingParameter = 2,
            InvalidFingerprint = 3,
            InvalidApikey = 4,
            Internal = 5,
            InvalidUserApikey = 6,
            InvalidUuid = 7,
            InvalidDuration = 8,
            InvalidBitrate = 9,
            InvalidForeignid = 10,
            InvalidMaxDurationDiff = 11,
            NotAllowed = 12,
            ServiceUnavailable = 13,
            TooManyRequests = 14,
            InvalidMusicbrainzAccessToken = 15,
            InsecureRequest = 16,
            UnknownApplication = 17,
            FingerprintNotFound = 18
        }

        public class LookupError
        {
            public string Message { get; set; }
            public AcoustIdErrorCode Code { get; set; }
        }

        public class LookupResultListItem
        {
            public int index { get; set; }
            public List<LookupResult> Results { get; set; }
        }

        public class LookupResult
        {
            public string Id { get; set; }
            public double Score { get; set; }
            public List<RecordingResult> Recordings { get; set; }
        }

        public class RecordingResult
        {
            public string Id { get; set; }
        }
    }
}
