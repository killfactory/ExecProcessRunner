using NUnit.Framework;
using Killfactory.Tools.Exec;
using System.IO;
using System.ComponentModel;

namespace ExecProcessRunnerTests
{
    [TestFixture]
    public class ExecProcessRunnerTests
    {
        private const string BatchName = "batchtest.bat";

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

        [TearDown]
        public void TearDown()
        {
            File.Delete(BatchName);
        }
    }
}
