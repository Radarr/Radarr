using System;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.RadarrList2.IMDbList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class IMDbListRequestGeneratorFixture : CoreTest<IMDbListRequestGenerator>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Logger = LogManager.GetCurrentClassLogger();
        }

        [Test]
        public void should_throw_not_supported_exception_for_ls_format_list()
        {
            Subject.Settings = new IMDbListSettings { ListId = "ls123456789" };

            Action act = () => Subject.GetMovies();

            act.Should().Throw<NotSupportedException>()
               .WithMessage("*ls12345678*no longer supported*");
        }

        [Test]
        public void should_throw_not_supported_exception_case_insensitive()
        {
            Subject.Settings = new IMDbListSettings { ListId = "LS999999999" };

            Action act = () => Subject.GetMovies();

            act.Should().Throw<NotSupportedException>();
        }
    }
}
