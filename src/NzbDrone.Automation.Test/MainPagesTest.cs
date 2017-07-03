using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using OpenQA.Selenium;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    public class MainPagesTest : AutomationTest
    {
        private PageBase page;

        [SetUp]
        public void Setup()
        {
            page = new PageBase(driver);
        }

        [Test]
        public void artist_page()
        {
            page.ArtistNavIcon.Click();
            page.WaitForNoSpinner();
            page.FindByClass("iv-artist-index-artistindexlayout").Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            page.CalendarNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-calendar-calendarlayout").Should().NotBeNull();
        }

        [Test]
        public void activity_page()
        {
            page.ActivityNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-activity-activitylayout").Should().NotBeNull();
        }

        [Test]
        public void wanted_page()
        {
            page.WantedNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-wanted-missing-missinglayout").Should().NotBeNull();
        }

        [Test]
        public void system_page()
        {
            page.SystemNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-system-systemlayout").Should().NotBeNull();
        }

        [Test]
        public void add_series_page()
        {
            page.ArtistNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Add Artist")).Click();

            page.WaitForNoSpinner();

            page.FindByClass("iv-addartist-addartistlayout").Should().NotBeNull();
        }
    }
}