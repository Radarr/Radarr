using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend.Mappers
{
    public class LoginHtmlMapper : HtmlMapperBase
    {
        private readonly IConfigFileProvider _configFileProvider;

        public LoginHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               IConfigFileProvider configFileProvider,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileProvider = configFileProvider;

            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "login.html");
            UrlBase = configFileProvider.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return HtmlPath;
        }

        protected override Stream GetContentStream(string filePath)
        {
            var text = GetHtmlText();

            var loginText = _configFileProvider.AuthenticationMethod switch
            {
                AuthenticationType.Plex => "Authenticate with Plex",
                AuthenticationType.Oidc => "Authenticate with OpenID Connect",
                _ => "Login"
            };

            var failedText = _configFileProvider.AuthenticationMethod switch
            {
                AuthenticationType.Forms => "Incorrect Username or Password",
                _ => "Access Denied"
            };

            text = text.Replace("LOGIN_PLACEHOLDER", loginText);
            text = text.Replace("FAILED_PLACEHOLDER", failedText);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/login");
        }
    }
}
