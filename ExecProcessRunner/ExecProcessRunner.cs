using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Killfactory.Tools.Exec
{
    public static class ProcessRunner
    {
        public static ProcessExecutionResult RunProcess(string fileName, string arguments = null, int timeout = default(int))
        {
            var outputBuilder = new List<string>();
            var errorOutputBuilder = new List<string>();
            var startInfo = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            using (var process = new Process
            {
                StartInfo = startInfo
            })
            {
                process.ErrorDataReceived += (sendingProcess, outLine) =>
                {
                    if (!string.IsNullOrEmpty(outLine.Data))
                    {
                        errorOutputBuilder.Add(outLine.Data);
                    }
                };
                process.OutputDataReceived += (sendingProcess, outLine) =>
                {
                    if (!string.IsNullOrEmpty(outLine.Data))
                    {
                        outputBuilder.Add(outLine.Data);
                    }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                if (timeout != default(int))
                {
                    if (!process.WaitForExit(timeout))
                    {
                        process.Kill();
                        throw new TimeoutException();
                    }
                }
                else
                {
                    process.WaitForExit();
                }
                var processExecutionResult = new ProcessExecutionResult
                {
                    ExitCode = process.ExitCode,
                    ErrorOutput = string.Join("\r\n", errorOutputBuilder),
                    Output = string.Join("\r\n", outputBuilder),
                };
                return processExecutionResult;
            }
        }
    }
}
