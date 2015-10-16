using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace ExecProcessRunnerTests
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Get the child processes for a given process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static List<Process> GetChildProcesses(this Process process)
        {
            var results = new List<Process>();

            // query the management system objects for any process that has the current
            // process listed as it's parentprocessid
            string queryText = string.Format("select processid from win32_process where parentprocessid = {0}", process.Id);
            using (var searcher = new ManagementObjectSearcher(queryText))
            {
                foreach (var obj in searcher.Get())
                {
                    object data = obj.Properties["processid"].Value;
                    if (data != null)
                    {
                        // retrieve the process
                        var childId = Convert.ToInt32(data);
                        Process childProcess = null;
                        try
                        {
                            childProcess = Process.GetProcessById(childId);
                        }
                        catch (ArgumentException) { }
     
                        // ensure the current process is still live
                        if (childProcess != null)
                            results.Add(childProcess);
                    }
                }
            }
            return results;
        }
    }
}
