using NUnit.Framework;
using Killfactory.Tools.Exec;
using System.IO;
using System.ComponentModel;
using System;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace ExecProcessRunnerTests
{
    [TestFixture]
    public class ExecProcessRunnerTests
    {
        private const string BatchName = "batchtest.bat";
        private const string PauseArguments = "/c pause";
        private const int Timeout = 1000;
        private const int AdditionalTimeout = 100;

        // Command call test.
        [Test]
        public void SimpleTest()
        {
            // Act.
            var result = ProcessRunner.RunProcess("cmd.exe", "/c echo aaa");
            // Assert.
            Assert.AreEqual("aaa", result.Output);
        }

        // Multistring output test.
        [Test]
        public void BatchTest()
        {
            // Arrange.
            using (var file = File.Create(BatchName))
            {
                using (var writer = new StreamWriter(file))
                {
                    writer.Write("@echo off\r\n" +
                                 "echo aaa\r\n" +
                                 "echo bbb");
                }
            }
            // Act.
            var result = ProcessRunner.RunProcess("cmd.exe", string.Format("/c {0}", BatchName));
            // Assert.
            Assert.AreEqual("aaa\r\nbbb", result.Output);
        }

        // We can call only real executables.
        [Test]
        [ExpectedException(typeof(Win32Exception))]
        public void ExceptionTest()
        {
            // Arrange.
            using (var file = File.Create(BatchName)) { }
            // Act.
            var result = ProcessRunner.RunProcess(BatchName);
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

        // Process is run only one time.
        [Test]
        public void SingleRunTest()
        {
            // Act.
            var result = ProcessRunner.RunProcess("cmd.exe", "/c echo aaa", Timeout);
            // Assert.
            Assert.AreEqual("aaa", result.Output);
        }

        // We should clean up to ensure repeatability.
        [TearDown]
        public void TearDown()
        {
            File.Delete(BatchName);
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
