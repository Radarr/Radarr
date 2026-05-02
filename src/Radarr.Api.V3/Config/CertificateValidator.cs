using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FluentValidation.Validators;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace Radarr.Api.V3.Config
{
    public static class CertificateValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidCertificate<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CertificateValidator());
        }
    }

    public class CertificateValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid SSL certificate file or password. {message}";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(CertificateValidator));

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            if (context.InstanceToValidate is not HostConfigResource resource)
            {
                return true;
            }

            var certificateCollection = new X509Certificate2Collection();

            try
            {
                certificateCollection.Import(resource.SslCertPath, resource.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (CryptographicException ex)
            {
                Logger.Debug(ex, "Invalid SSL certificate file or password. {0}", ex.Message);

                context.MessageFormatter.AppendArgument("message", ex.Message);

                return false;
            }

            if (certificateCollection.None(c => c.HasPrivateKey))
            {
                var message = $"The SSL certificate file {resource.SslCertPath} does not contain a certificate with an associated private key";

                Logger.Debug($"{message}");

                context.MessageFormatter.AppendArgument("message", message);

                return false;
            }

            return true;
        }
    }
}
