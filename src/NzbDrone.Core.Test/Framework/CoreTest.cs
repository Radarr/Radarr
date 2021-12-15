using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Http;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Security;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Framework
{
    public abstract class CoreTest : TestBase
    {
        protected void UseRealHttp()
        {
            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("3.0.0"));
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Version).Returns("1.0.0");
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Name).Returns("TestOS");

            Mocker.SetConstant<IHttpProxySettingsProvider>(new HttpProxySettingsProvider(Mocker.Resolve<ConfigService>()));
            Mocker.SetConstant<ICreateManagedWebProxy>(new ManagedWebProxyFactory(Mocker.Resolve<CacheManager>()));
            Mocker.SetConstant<ICertificateValidationService>(new X509CertificateValidationService(Mocker.Resolve<ConfigService>(), TestLogger));
            Mocker.SetConstant<IHttpDispatcher>(new ManagedHttpDispatcher(Mocker.Resolve<IHttpProxySettingsProvider>(), Mocker.Resolve<ICreateManagedWebProxy>(), Mocker.Resolve<ICertificateValidationService>(), Mocker.Resolve<UserAgentBuilder>(), Mocker.Resolve<CacheManager>()));
            Mocker.SetConstant<IHttpClient>(new HttpClient(Array.Empty<IHttpRequestInterceptor>(), Mocker.Resolve<CacheManager>(), Mocker.Resolve<RateLimitService>(), Mocker.Resolve<IHttpDispatcher>(), TestLogger));
            Mocker.SetConstant<IRadarrCloudRequestBuilder>(new RadarrCloudRequestBuilder());
        }

        //Used for tests that rely on parsing working correctly.
        protected void UseRealParsingService()
        {
            //Mocker.SetConstant<IParsingService>(new ParsingService(Mocker.Resolve<MovieService>(), Mocker.Resolve<ConfigService>(), Mocker.Resolve<QualityDefinitionService>(), TestLogger));
        }

        //Used for tests that rely on parsing working correctly. Does some minimal parsing using the old static methods.
        protected void ParseMovieTitle()
        {
            Mocker.GetMock<IParsingService>().Setup(c => c.ParseMovieInfo(It.IsAny<string>(), It.IsAny<System.Collections.Generic.List<object>>()))
                .Returns<string, System.Collections.Generic.List<object>>((title, helpers) =>
                {
                    var result = Parser.Parser.ParseMovieTitle(title);
                    if (result != null)
                    {
                        result.Quality = QualityParser.ParseQuality(title);
                    }

                    return result;
                });
        }
    }

    public abstract class CoreTest<TSubject> : CoreTest
        where TSubject : class
    {
        private TSubject _subject;

        [SetUp]
        public void CoreTestSetup()
        {
            _subject = null;
        }

        protected TSubject Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = Mocker.Resolve<TSubject>();
                }

                return _subject;
            }
        }
    }
}
