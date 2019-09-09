using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class MetadataRequestBuilderFixture : CoreTest<MetadataRequestBuilder>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.MetadataSource)
                .Returns("");

            Mocker.GetMock<ILidarrCloudRequestBuilder>()
                .Setup(s => s.Search)
                .Returns(new HttpRequestBuilder("https://api.lidarr.audio/api/v0.4/{route}").CreateFactory());
        }

        private void WithCustomProvider()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.MetadataSource)
                .Returns("http://api.lidarr.audio/api/testing/");
        }

        [TestCase]
        public void should_use_user_definied_if_not_blank()
        {
            WithCustomProvider();

            var details = Subject.GetRequestBuilder().Create();

            details.BaseUrl.ToString().Should().Contain("testing");
        }

        [TestCase]
        public void should_use_default_if_config_blank()
        {
            var details = Subject.GetRequestBuilder().Create();

            details.BaseUrl.ToString().Should().Contain("v0.4");
        }
    }
}
