using NUnit.Framework;
using Killfactory.Tools.Exec;
using System;
using System.Diagnostics;
using System.Linq;


namespace ExecProcessRunnerTests
{
    [TestFixture]
    public class TimeoutTests
    {
        private const string PauseArguments = "/c pause";
        private const int Timeout = 1000;
        private const int AdditionalTimeout = 100;

        // Process is run only one time.
        [Test]
        public void SingleRunTest()
        {
            // Act.
            var result = ProcessRunner.RunProcess("cmd.exe", "/c echo aaa", Timeout);
            // Assert.
            Assert.AreEqual("aaa", result.Output);
        }

        // We can set timeout and process execution will stop.
        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void TimeoutTest()
        {
            // Arrange.
            var func = new Func<ProcessExecutionResult>(() => ProcessRunner.RunProcess("cmd.exe", PauseArguments, Timeout));
            // Act.
            var result = func.BeginInvoke((a) => { }, null);
            result.AsyncWaitHandle.WaitOne(Timeout + AdditionalTimeout);
            // Assert.
            if (result.IsCompleted)
            {
                func.EndInvoke(result);
            }
        }

        // Process is killed when TimeoutException is called. No additional wait happens.
        [Test]
        public void TimeoutKillTest()
        {
            // Arrange.
            var func = new Func<ProcessExecutionResult>(() => ProcessRunner.RunProcess("cmd.exe", PauseArguments, Timeout));
            // Act.
            var result = func.BeginInvoke((a) => { }, null);
            result.AsyncWaitHandle.WaitOne();
            // Assert.
            if (Process.GetCurrentProcess().GetChildProcesses().Any())
            {
                Assert.Fail();
            }
            Assert.Pass();
        }

        // We should clean up to ensure repeatability.
        [TearDown]
        public void TearDown()
        {
            var processes = Process.GetCurrentProcess().GetChildProcesses();
            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception)
                    {
                        Debug.Print("Can't kill PID:{0}", process.Id);
                    }
                }
            }
        }
    }
}
