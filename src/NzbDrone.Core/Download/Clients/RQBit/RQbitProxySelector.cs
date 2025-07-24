namespace NzbDrone.Core.Download.Clients.RQBit
{
    using System;
    using NLog;
    using NzbDrone.Common.Cache;

    public interface IRQbitProxySelector
    {
        IRQbitProxy GetProxy(RQbitSettings settings, bool force = false);
        Version GetApiVersion(RQbitSettings settings, bool force = false);
    }

    public class RQbitProxySelector : IRQbitProxySelector
    {
        private readonly ICached<Tuple<IRQbitProxy, Version>> _proxyCache;
        private readonly Logger _logger;
        private readonly IRQbitProxy _proxyV1;

        public RQbitProxySelector(RQbitProxy proxyV1, ICacheManager cacheManager, Logger logger)
        {
            _proxyCache = cacheManager.GetCache<Tuple<IRQbitProxy, Version>>(GetType());
            _logger = logger;
            _proxyV1 = proxyV1;
        }

        public IRQbitProxy GetProxy(RQbitSettings settings, bool force)
        {
            return GetProxyCache(settings, force).Item1;
        }

        public Version GetApiVersion(RQbitSettings settings, bool force)
        {
            return GetProxyCache(settings, force).Item2;
        }

        private Tuple<IRQbitProxy, Version> GetProxyCache(RQbitSettings settings, bool force)
        {
            var proxyKey = $"{settings.Host}_{settings.Port}";

            if (force)
            {
                _proxyCache.Remove(proxyKey);
            }

            return _proxyCache.Get(proxyKey, () => FetchProxy(settings), TimeSpan.FromMinutes(10.0));
        }

        private Tuple<IRQbitProxy, Version> FetchProxy(RQbitSettings settings)
        {
            // For now, we only have one API version, but this pattern allows for future extensions
            if (_proxyV1.IsApiSupported(settings))
            {
                var version = ParseVersion(_proxyV1.GetVersion(settings));
                _logger.Trace("Using RQBit API v1, detected version: {0}", version);
                return Tuple.Create(_proxyV1, version);
            }

            throw new DownloadClientException("Unable to determine RQBit API version or RQBit is not responding");
        }

        private Version ParseVersion(string versionString)
        {
            // RQBit version might be in different formats, try to parse it safely
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return new Version(1, 0, 0);
            }

            try
            {
                // Remove any non-numeric prefix/suffix and try to parse as version
                var cleanVersion = versionString.Trim().TrimStart('v');

                // Handle semantic versioning (e.g., "8.1.0", "8.0.0-rc1")
                // Split on '-' to remove pre-release identifiers
                var versionPart = cleanVersion.Split('-')[0];

                // If it's just numbers with dots, parse as version
                if (Version.TryParse(versionPart, out var version))
                {
                    return version;
                }

                // Try to extract just the major.minor.patch numbers using regex
                var match = System.Text.RegularExpressions.Regex.Match(cleanVersion, @"(\d+)\.(\d+)\.(\d+)");
                if (match.Success)
                {
                    var major = int.Parse(match.Groups[1].Value);
                    var minor = int.Parse(match.Groups[2].Value);
                    var patch = int.Parse(match.Groups[3].Value);
                    return new Version(major, minor, patch);
                }

                // Try major.minor format
                var simpleMatch = System.Text.RegularExpressions.Regex.Match(cleanVersion, @"(\d+)\.(\d+)");
                if (simpleMatch.Success)
                {
                    var major = int.Parse(simpleMatch.Groups[1].Value);
                    var minor = int.Parse(simpleMatch.Groups[2].Value);
                    return new Version(major, minor, 0);
                }

                // If parsing fails, default to 1.0.0
                return new Version(1, 0, 0);
            }
            catch
            {
                return new Version(1, 0, 0);
            }
        }
    }
}
