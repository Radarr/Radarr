using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NzbDrone.Common.Http
{
    public class SslCertificateLoadException : Exception
    {
        public SslCertificateLoadException(string message)
            : base(message)
        {
        }
    }

    public static class SslCertificateLoader
    {
        public static SslStreamCertificateContext LoadCertificateContext(string certPath, string certPassword)
        {
            var certificateCollection = new X509Certificate2Collection();

            certificateCollection.Import(certPath, certPassword, X509KeyStorageFlags.DefaultKeySet);

            var leafCert = certificateCollection.FirstOrDefault(c => c.HasPrivateKey);
            if (leafCert == null)
            {
                throw new SslCertificateLoadException(
                    $"The SSL certificate file {certPath} does not contain a certificate with an associated private key");
            }

            return SslStreamCertificateContext.Create(leafCert, certificateCollection, offline: true);
        }
    }
}
