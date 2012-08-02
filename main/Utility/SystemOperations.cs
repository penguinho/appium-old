using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Automation.Utility
{
    /// <summary>contains utility functions to run system Level actions</summary>
    public static class SystemOperations
    {
        /// <summary>checks if the operating system is mac os x</summary>
        /// <returns></returns>
        public static bool IsRunningOnMacOSX()
        {
            return System.Environment.OSVersion.Platform == PlatformID.MacOSX || System.Environment.OSVersion.Platform == PlatformID.Unix;
        }

        /// <summary>executes a shell command</summary>
        /// <param name="filename">name of the binary to execute</param>
        /// <param name="arguments">arguments to supply to the binary</param>
        /// <returns>response of the command</returns>
        public static CommandResult RunShellCommand(string filename, string arguments)
        {
            // start the process
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = filename;
            procInfo.Arguments = arguments;
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardOutput = true;
            Process p = Process.Start(procInfo);

            // wait for exit
            p.WaitForExit();
            return new CommandResult(p.ExitCode, p.StandardOutput.ReadToEnd());
        }

          /// <summary>checks if selenium rc server is running</summary>
        /// <param name="SeleniumServerProc">the selenium rc server process</param>
        /// <returns>true if selenium rc server is running</returns>
        public static bool IsSeleniumRunning(Process SeleniumServerProc)
        {
            return SeleniumServerProc != null && !SeleniumServerProc.HasExited;
        }

        /// <summary>quits selenium rc server</summary>
        /// <param name="SeleniumServerProc">the selenium rc server process</param>
        /// <param name="SeleniumServerFileName">the filename pf the jar used to launch selenium rc server</param>
        /// <returns>true if successful</returns>
        public static bool QuitSelenium(Process SeleniumServerProc, string SeleniumServerFileName)
        {
            if (IsSeleniumRunning(SeleniumServerProc))
            {
                // kill the process
                SeleniumServerProc.Kill();
                SeleniumServerProc.WaitForExit();
            }

            // delete the temp file
            File.Delete(SeleniumServerFileName);

            return SeleniumServerProc.HasExited;
        }

        /// <summary>cleans the iphone simulator</summary>
        /// <returns>true if successful</returns>
        public static bool CleaniOSSimulator()
        {
            // check for mac os x
            if (!Utility.SystemOperations.IsRunningOnMacOSX())
                return false;

            // delete all subdirectories which only have digits and periods in their names (data for simulator versions)
            string simulatorDataPath = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/iPhone Simulator");
            foreach (string directory in Directory.GetDirectories(simulatorDataPath))
            {
                string relativeDirectory = Path.GetFileName(directory);
                bool shouldDelete = true;
                for (int i = 0; i < relativeDirectory.Length && shouldDelete; i++)
                {
                    // check for things that aren't digits or numbers
                    if (!char.IsDigit(relativeDirectory[i]) && relativeDirectory[i] != '.')
                        shouldDelete = false;
                }

                if (shouldDelete)
                {
                    // delete the directory
                    DeleteDirectory(new DirectoryInfo(directory));
                }
            }

            return true;
        }

        /// <summary>runs an applescript command</summary>
        /// <param name="scriptText">text of the command</param>
        /// <param name="prependiPhoneBoilerPlate">true if iphone ui boilerplate should be prepended</param>
        /// <returns>response of the command</returns>
        public static CommandResult RunApplescript(string scriptText, bool prependiPhoneBoilerPlate = false)
        {
            // check for mac os x
            if (!Utility.SystemOperations.IsRunningOnMacOSX())
                return new CommandResult(-1, "This can only be run from Mac OS X");

            // write applescript to a temp file
            string tempScriptFileName = Path.GetTempFileName();
            tempScriptFileName = Path.ChangeExtension(tempScriptFileName, ".applescript");
            StreamWriter sw = new StreamWriter(tempScriptFileName);
            sw.Write(scriptText);
            sw.Close();

            // run the command
            CommandResult cr;
            try
            {
                // return the output of the command
                cr = RunShellCommand("/usr/bin/osascript", tempScriptFileName);
            }
            catch (Exception e)
            {
                // return the error
                cr = new CommandResult(-1, e.ToString());
            }
            finally
            {
                // delete the temp file
                File.Delete(tempScriptFileName);
            }

            return cr;
        }

        /// <summary>deletes a directory</summary>
        /// <param name="directoryInfo">directory to delete</param>
        public static void DeleteDirectory(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
                file.Delete();
            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
                DeleteDirectory(subfolder);
            try { directoryInfo.Delete(); }
            catch { }
        }
    }
}
