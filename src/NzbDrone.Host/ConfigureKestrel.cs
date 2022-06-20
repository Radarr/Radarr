using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public class ConfigureKestrel : IConfigureOptions<KestrelServerOptions>
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _config;

        public ConfigureKestrel(IOptionsMonitor<ConfigFileOptions> config)
        {
            _config = config;
        }

        public void Configure(KestrelServerOptions options)
        {
            options.AllowSynchronousIO = true;
            options.Limits.MaxRequestBodySize = null;

            Listen(options, _config.CurrentValue.BindAddress, _config.CurrentValue.Port);

            if (_config.CurrentValue.EnableSsl && _config.CurrentValue.SslCertPath.IsNotNullOrWhiteSpace())
            {
                options.ConfigureHttpsDefaults(opts => opts.ServerCertificate = ValidateSslCertificate(_config.CurrentValue.SslCertPath, _config.CurrentValue.SslCertPassword));
                Listen(options, _config.CurrentValue.BindAddress, _config.CurrentValue.SslPort, opts => opts.UseHttps());
            }
        }

        private static void Listen(KestrelServerOptions options, string address, int port)
        {
            Listen(options, address, port, _ => { });
        }

        private static void Listen(KestrelServerOptions options, string address, int port, Action<ListenOptions> configureListenOptions)
        {
            // following https://github.com/dotnet/aspnetcore/blob/d96a100bddc72606f7417b665428411388b8ac54/src/Servers/Kestrel/Core/src/Internal/AddressBinder.cs#L123
            if (string.Equals(address, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                options.ListenLocalhost(port, configureListenOptions);
            }
            else if (IPAddress.TryParse(address, out var endpoint))
            {
                options.Listen(endpoint, port, configureListenOptions);
            }
            else
            {
                options.ListenAnyIP(port, configureListenOptions);
            }
        }

        private static X509Certificate2 ValidateSslCertificate(string cert, string password)
        {
            X509Certificate2 certificate;

            try
            {
                certificate = new X509Certificate2(cert, password, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (CryptographicException ex)
            {
                if (ex.HResult == 0x2 || ex.HResult == 0x2006D080)
                {
                    throw new RadarrStartupException(ex,
                        $"The SSL certificate file {cert} does not exist");
                }

                throw new RadarrStartupException(ex);
            }

            return certificate;
        }
    }
}
