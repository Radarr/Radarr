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
        public void movie_page()
        {
            page.MovieNavIcon.Click();
            page.WaitForNoSpinner();
            page.Find(By.CssSelector("div[class*='MovieIndex']")).Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            page.CalendarNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.CssSelector("div[class*='CalendarPage']")).Should().NotBeNull();
        }

        [Test]
        public void activity_page()
        {
            page.ActivityNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Queue")).Should().NotBeNull();
            page.Find(By.LinkText("History")).Should().NotBeNull();
            page.Find(By.LinkText("Blacklist")).Should().NotBeNull();
        }

        [Test]
        public void system_page()
        {
            page.SystemNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.CssSelector("div[class*='Health']")).Should().NotBeNull();
        }

        [Test]
        public void add_movie_page()
        {
            page.MovieNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Add New")).Click();

            page.WaitForNoSpinner();

            page.Find(By.CssSelector("input[class*='AddNewMovie/searchInput']")).Should().NotBeNull();
        }
    }
}