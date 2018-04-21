using Moq;
using System;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Common.TPL;
using NzbDrone.Test.Common;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Core.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

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
            Mocker.SetConstant<ManagedHttpDispatcher>(new ManagedHttpDispatcher(Mocker.Resolve<IHttpProxySettingsProvider>(), Mocker.Resolve<ICreateManagedWebProxy>(), Mocker.Resolve<UserAgentBuilder>()));
            Mocker.SetConstant<CurlHttpDispatcher>(new CurlHttpDispatcher(Mocker.Resolve<IHttpProxySettingsProvider>(), Mocker.Resolve<UserAgentBuilder>(), Mocker.Resolve<NLog.Logger>()));
            Mocker.SetConstant<IHttpProvider>(new HttpProvider(TestLogger));
            Mocker.SetConstant<IHttpClient>(new HttpClient(new IHttpRequestInterceptor[0], Mocker.Resolve<CacheManager>(), Mocker.Resolve<RateLimitService>(), Mocker.Resolve<FallbackHttpDispatcher>(), Mocker.Resolve<UserAgentBuilder>(), TestLogger));
            Mocker.SetConstant<ISonarrCloudRequestBuilder>(new SonarrCloudRequestBuilder());
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
                    var result = Parser.Parser.ParseMovieTitle(title, false);
                    if (result != null)
                    {
                        result.Quality = QualityParser.ParseQuality(title);
                    }
                    return result;
                });
        }
    }

    public abstract class CoreTest<TSubject> : CoreTest where TSubject : class
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
