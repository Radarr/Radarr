using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Test.Dummy;

namespace NzbDrone.Common.Test
{
    // We don't want one tests setup killing processes used in another
    [NonParallelizable]
    [TestFixture]
    [Platform(Exclude = "MacOsX")]
    public class ProcessProviderFixture : TestBase<ProcessProvider>
    {

        [SetUp]
        public void Setup()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c =>
                {
                    c.Kill();
                    c.WaitForExit();
                });

            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).Should().BeEmpty();
        }

        [TearDown]
        public void TearDown()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c =>
            {
                try
                {
                    c.Kill();
                }
                catch (Win32Exception ex)
                {
                    TestLogger.Warn(ex, "{0} when killing process", ex.Message);
                }
                
            });
        }

        [Test]
        public void GetById_should_return_null_if_process_doesnt_exist()
        {
            Subject.GetProcessById(1234567).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(9999)]
        public void GetProcessById_should_return_null_for_invalid_process(int processId)
        {
            Subject.GetProcessById(processId).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_be_able_to_start_process()
        {
            var process = StartDummyProcess();

            Thread.Sleep(500);

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should()
                   .BeTrue("one running dummy process");

            process.Kill();
            process.WaitForExit();

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should().BeFalse();
        }

        [Test]
        public void exists_should_find_running_process()
        {
            var process = StartDummyProcess();

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should()
                   .BeTrue("expected one dummy process to be already running");

            process.Kill();
            process.WaitForExit();

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should().BeFalse();
        }


        [Test]
        public void kill_all_should_kill_all_process_with_name()
        {
            var dummy1 = StartDummyProcess();
            var dummy2 = StartDummyProcess();

            Thread.Sleep(500);

            Subject.KillAll(DummyApp.DUMMY_PROCCESS_NAME);

            dummy1.HasExited.Should().BeTrue();
            dummy2.HasExited.Should().BeTrue();
        }

        private Process StartDummyProcess()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, DummyApp.DUMMY_PROCCESS_NAME + ".exe");
            return Subject.Start(path);
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
            ExceptionVerification.MarkInconclusive(typeof(Win32Exception));
        }
    }
}
