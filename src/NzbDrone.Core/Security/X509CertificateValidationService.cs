using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Security
{
    public class X509CertificateValidationService : ICertificateValidationService
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public X509CertificateValidationService(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sender is not SslStream request)
            {
                return true;
            }

            if (certificate is X509Certificate2 cert2 && cert2.SignatureAlgorithm.FriendlyName == "md5RSA")
            {
                _logger.Error("https://{0} uses the obsolete md5 hash in it's https certificate, if that is your certificate, please (re)create certificate with better algorithm as soon as possible.", request.TargetHostName);
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (request.TargetHostName == "localhost" || request.TargetHostName == "127.0.0.1")
            {
                return true;
            }

            var ipAddresses = GetIPAddresses(request.TargetHostName);
            var certificateValidation = _configService.CertificateValidation;

            if (certificateValidation == CertificateValidationType.Disabled)
            {
                return true;
            }

            if (certificateValidation == CertificateValidationType.DisabledForLocalAddresses &&
                ipAddresses.All(i => i.IsLocalAddress()))
            {
                return true;
            }

            _logger.Error("Certificate validation for {0} failed. {1}", request.TargetHostName, sslPolicyErrors);

            return false;
        }

        private IPAddress[] GetIPAddresses(string host)
        {
            if (IPAddress.TryParse(host, out var ipAddress))
            {
                return new[] { ipAddress };
            }

            return Dns.GetHostEntry(host).AddressList;
        }
    }
}
