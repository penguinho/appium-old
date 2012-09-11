using Automation.Testing.Logging;
using Automation.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Automation.Testing.iOS
{
    /// <summary>test client which can execute ios javascript automation command</summary>
    public class iOSClient
    {
        /// <summary>log engine</summary>
        public LogEngine Log { get; set; }

        /// <summary>true if the automation client is batching commands</summary>
        protected bool _IsBatchingCommands_;
        /// <summary>queue of batched commands</summary>
        protected List<string> _BatchCommandQueue_;

        // variables for local ios client
        /// <summary>index of the last command issued via ios automation</summary>
        private int _CommandIndex;
        /// <summary>the instruments process</summary>
        private Process _InstrumentsProc;
        /// <summary>process which monitors for security agent dialogs</summary>
        private Process _SecurityAgentDetector;
        /// <summary>directory where automation files will be stored</summary>
        private string _iOSAutomationDirectory;
        /// <summary>directory of the compiled ios app</summary>
        private string _CompiledAppPath;
        /// <summary>configuration for the ios client</summary>
        private Configuration Config;

        /// <summary>contructor</summary>
        /// <param name="config">configuration for the ios client</param>
        /// <param name="targetLog">log engine to use</param>
        public iOSClient(Configuration config, LogEngine targetLog = null)
        {
            Log = targetLog ?? new LogEngine();
            Config = config;

            _BatchCommandQueue_ = new List<string>();
            _IsBatchingCommands_ = false;
            _CommandIndex = -1;
            _CompiledAppPath = "";
            _StartAutomation(Config.Device);
        }

        /// <summary>represents the configuration for an ios client</summary>
        public class Configuration
        {
            /// <summary>device to boot</summary>
            public iOSDevice Device;
            /// <summary>username of a user with permissions to run instruments</summary>
            public string DevUserName;
            /// <summary>password of a user with permissions to run instruments</summary>
            public string DevPassword;
            /// <summary>name of the xcode project</summary>
            public string XCodeProjectName;
            /// <summary>path to the xcode project</summary>
            public string XCodeProjectPath;
            /// <summary>name of the compiled ios app</summary>
            public string CompiledAppName;
            /// <summary>target for the xcodebuild command line</summary>
            public string BuildTarget;
            /// <summary>sdk for the xcodebuild command line</summary>
            public string BuildSDK;
            /// <summary>scheme for the xcodebuild command line</summary>
            public string BuildScheme;

            /// <summary>contructor</summary>
            /// <param name="device">type of device (iphone or ipad)</param>
            /// <param name="devUserName">username of a user with permissions to run instruments</param>
            /// <param name="devPassword">password of a user with permissions to run instruments</param>
            /// <param name="xcodeProjectName">name of the xcode project</param>
            /// <param name="compiledAppName">name of the compiled app</param>
            /// <param name="buildTarget">target for xcodebuild command line</param>
            /// <param name="buildSDK">sdk for xcodebuild command line</param>
            /// <param name="buildScheme">scheme for xcodebuild command line</param>
            /// <param name="xCodeProjectPath">absolute path to the xcode project (null will search for it using spotlight)</param>
            public Configuration(iOSDevice device, string devUserName, string devPassword,  string xcodeProjectName, string compiledAppName, string buildTarget, string buildSDK = "iphonesimulator5.1", string buildScheme = null, string xCodeProjectPath = null)
            {
                Device = device;
                XCodeProjectPath = xCodeProjectPath;
                XCodeProjectName = xcodeProjectName;
                CompiledAppName = compiledAppName;
                BuildTarget = buildTarget;
                BuildSDK = buildSDK;
                BuildScheme = buildScheme;
                DevUserName = devUserName;
                DevPassword = devPassword;
            }
        }

        /// <summary>issues a command to the automation system</summary>
        /// <param name="cmdText">text of the command to issue</param>
        /// <returns>result of the command</returns>
        public CommandResult[] IssueiOSAutomationCommand(string cmdText)
        {
            if (_IsBatchingCommands_)
            {
                _BatchCommandQueue_.Add(cmdText);
                return new CommandResult[] { new CommandResult(0, "command batched successfully") };
            }

            _CommandIndex++;
            try
            {
                // write the command
                StreamWriter commandFileWriter = new StreamWriter(Path.Combine(_iOSAutomationDirectory, _CommandIndex.ToString() + "-cmd.txt"));
                commandFileWriter.Write(cmdText);
                commandFileWriter.Close();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                _CommandIndex--;
            }

            // wait up to 10 minutes for a response
            DateTime startTime = DateTime.Now;
            while (DateTime.Now.Subtract(startTime).TotalMinutes < 10)
            {
                string responseFile = Path.Combine(_iOSAutomationDirectory, _CommandIndex.ToString() + "-resp.txt");
                StreamReader responseFileReader;
                try
                {
                    if (File.Exists(responseFile))
                    {
                        List<CommandResult> results = new List<CommandResult>();
                        XmlDocument data = new XmlDocument();
                        responseFileReader = new StreamReader(responseFile);
                        data.LoadXml(responseFileReader.ReadToEnd());
                        responseFileReader.Close();
                        XmlNodeList responses = data.SelectNodes("/collection/response");
                        foreach (XmlNode responseNode in responses)
                            results.Add(new CommandResult(responseNode.InnerXml));
                        return results.ToArray();
                    }
                }
                catch { }
            }

            return new CommandResult[] { new CommandResult(-1, "Did Not Get Response.") };
        }

        /// <summary>launches the app in the iphone simulator</summary>
        /// <param name="device">type of ios device</param>
        /// <returns>true if successful</returns>
        private bool _StartAutomation(iOSDevice device)
        {
            if (IsAutomationRunning())
                return true;

            // create a new directory for the automation
            _iOSAutomationDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_iOSAutomationDirectory);

            // write bootstrap.js and file writer
            string bootstrapPath = Path.Combine(_iOSAutomationDirectory, "bootstrap.js");
            string instrumentsTemplatePath = Path.Combine(_iOSAutomationDirectory, "Automation.tracetemplate");
            File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "Automation.tracetemplate"), instrumentsTemplatePath);
            StreamWriter sw = new StreamWriter(bootstrapPath);
            sw.Write(Automation.Testing.Properties.Resources.bootstrap.Replace("$PATH_ROOT", _iOSAutomationDirectory + "/"));
            sw.Close();
            sw = new StreamWriter(Path.Combine(_iOSAutomationDirectory, "writeResponse.py"));
            sw.Write(Automation.Testing.Properties.Resources.writeResponse);
            sw.Close();

            // clean the simulator
            Log.Info("Cleaning the iOS Simulator");
            Utility.SystemOperations.CleaniOSSimulator();

            // delete old builds
            Log.Info("Deleting Old Builds");
            string buildDir = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/iPhone Simulator");
            if (Directory.Exists(buildDir))
            {
                foreach (string subdir in Directory.GetDirectories(buildDir))
                {
                    Log.Verbose("Deleting Old Build : " + Path.Combine(buildDir, subdir));
                    Utility.SystemOperations.DeleteDirectory(new DirectoryInfo(Path.Combine(buildDir, subdir)));
                }
            }

            // delete all copies of the app under test
            CommandResult cr = Utility.SystemOperations.RunShellCommand("/usr/bin/mdfind", "-name \"" + Config.CompiledAppName + "\"");
            if (cr.WasSuccessful)
            {
                foreach (string appPath in cr.CommandOutput.Split('\n'))
                {
                    Log.Verbose("Deleting Old Build : " + appPath);
                    try { Utility.SystemOperations.DeleteDirectory(new DirectoryInfo(appPath)); }
                    catch { }
                }
            }

            // find the xcode project if it was not supplied in settings
            if (null == Config.XCodeProjectPath || !Config.XCodeProjectPath.Contains("/"))
            {
                cr = Utility.SystemOperations.RunShellCommand("/usr/bin/mdfind", "-name \"" + Config.XCodeProjectName + "\"");
                if (!cr.WasSuccessful)
                    Log.CriticalError("Could not find the .xcodeproj file:" + cr.CommandOutput);
                Config.XCodeProjectPath = cr.CommandOutput.Split(new char[] { '\n' })[0];
            }
            Log.Verbose("XCode Project Found : " + Config.XCodeProjectPath);

            // perform any prebuild modifications
            Log.Verbose("Running Pre-Build Steps");
            OnPreBuild(Config.XCodeProjectPath);

            // generate xcodebuild argument string
            StringBuilder xcodebuildArgSB = new StringBuilder();
            xcodebuildArgSB.Append("-sdk ");
            xcodebuildArgSB.Append(Config.BuildSDK);
            xcodebuildArgSB.Append(" -target ");
            xcodebuildArgSB.Append(Config.BuildTarget);
            if (null != Config.BuildScheme)
            {
                xcodebuildArgSB.Append(" -scheme ");
                xcodebuildArgSB.Append(Config.BuildScheme);
            }
            xcodebuildArgSB.Append(" TARGETED_DEVICE_FAMILY=");
            xcodebuildArgSB.Append(((int)device).ToString());

            // compile the xcode project
            Log.Info("Compiling the XCode Project.");
            string xcodeProjDirectory = Path.GetDirectoryName(Config.XCodeProjectPath);
            string oldDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(xcodeProjDirectory);
            ProcessStartInfo xCodeBuildProcInfo = new ProcessStartInfo();
            xCodeBuildProcInfo.FileName = "/usr/bin/xcodebuild";
            xCodeBuildProcInfo.Arguments = xcodebuildArgSB.ToString();
            xCodeBuildProcInfo.UseShellExecute = true;
            Process xCodeBuildProc = Process.Start(xCodeBuildProcInfo);
            xCodeBuildProc.WaitForExit();
            Directory.SetCurrentDirectory(oldDirectory);

            // perform any postbuild modifications
            OnPostBuild(Config.XCodeProjectPath);

            // find the compiled app using spotlight since you cannot read stdout when using shell execute
            int numberOfAttemptsToFindTheCompiledApp = 0;
            while (_CompiledAppPath == "" && numberOfAttemptsToFindTheCompiledApp < 15)
            {
                cr = Utility.SystemOperations.RunShellCommand("/usr/bin/mdfind", "-name \"" + Config.CompiledAppName + "\"");
                if (!cr.WasSuccessful)
                    Log.CriticalError("Could not find the .app file:" + cr.CommandOutput);
                foreach (string projPath in cr.CommandOutput.Split(new char[] { '\n' }))
                {
                    if (projPath.EndsWith(".app"))
                    {
                        _CompiledAppPath = projPath;
                        break;
                    }
                    else if (projPath.EndsWith(".app.dSYM"))
                    {
                        _CompiledAppPath = projPath.Replace(".dSYM", "");
                        break;
                    }
                }
                numberOfAttemptsToFindTheCompiledApp++;
                System.Threading.Thread.Sleep(2000);
            }

            if (_CompiledAppPath == "")
                Log.CriticalError("Could not find the compiled app.");
            Log.Verbose("Compiled AppPath : " + _CompiledAppPath);

            // launch security agent detector
            string beatSecurityAgentPath = Path.Combine(_iOSAutomationDirectory, "BeatSecurityAgent.applescript");
            sw = new StreamWriter(beatSecurityAgentPath);
            sw.Write(Automation.Testing.Properties.Resources.BeatSecurityAgent.Replace("$USERNAME", Config.DevUserName).Replace("$PASSWORD", Config.DevPassword));
            sw.Close();
            ProcessStartInfo securityAgentDetectorStartInfo = new ProcessStartInfo();
            securityAgentDetectorStartInfo.FileName = "/usr/bin/osascript";
            securityAgentDetectorStartInfo.Arguments = beatSecurityAgentPath;
            securityAgentDetectorStartInfo.UseShellExecute = false;
            _SecurityAgentDetector = Process.Start(securityAgentDetectorStartInfo);

            // launch instruments with bootstrap
            Log.Info("Launching Instruments");
            ProcessStartInfo instrumentsProcStartInfo = new ProcessStartInfo();
            instrumentsProcStartInfo.FileName = "/usr/bin/instruments";
            instrumentsProcStartInfo.Arguments = "-t " + instrumentsTemplatePath + " " + _CompiledAppPath +
                " -e UIASCRIPT " + bootstrapPath + " -e UIARESULTSPATH " + _iOSAutomationDirectory;
            instrumentsProcStartInfo.UseShellExecute = false;
            instrumentsProcStartInfo.RedirectStandardInput = true;
            try
            {
                _InstrumentsProc = Process.Start(instrumentsProcStartInfo);
                _CommandIndex = -1;
            }
            catch (Exception e)
            {
                Log.CriticalError(e.ToString());
                _InstrumentsProc = null;
            }

            return true;
        }

        /// <summary>method to perform any pre build modifications</summary>
        /// <param name="xCodeProjectPath">path to the xcode project</param>
        protected virtual void OnPreBuild(string xCodeProjectPath) { }

        /// <summary>method to perform any post build modifications</summary>
        /// <param name="xCodeProjectPath">path to the xcode project</param>
        protected virtual void OnPostBuild(string xCodeProjectPath) { }

        /// <summary>method to perform any postbuild modifications</summary>
        /// <param name="xCodeProjectPath">path to the xcode project</param>
        protected virtual void OnShutDown(string xCodeProjectPath) { }

        /// <summary>quits the iphone simulator</summary>
        /// <returns>true if successful</returns>
        public bool StopAutomation()
        {
            if (IsAutomationRunning())
            {
                // try to exit the process normally
                IssueiOSAutomationCommand("runLoop=false;");
                if (!Automation.Utility.Misc.WaitWithTimeout<bool>(() => _InstrumentsProc.HasExited, true, 15000))
                {
                    // kill the process if it does not exit normally
                    _InstrumentsProc.Kill();
                }
                Utility.SystemOperations.RunApplescript("tell application \"iPhone Simulator\" to quit");
            }

            OnShutDown(Config.XCodeProjectPath);

            try { _SecurityAgentDetector.Kill(); }
            catch { }

            return true;
        }

        /// <summary>checks if ios automation is running on the remote controller server</summary>
        /// <returns>true if ios automation is running of the remote controlled server</returns>
        public bool IsAutomationRunning()
        {
            return (_InstrumentsProc != null) ? !_InstrumentsProc.HasExited : false;
        }

        /// <summary>runs all jobs in the batch queue</summary>
        /// <returns>indexed results of all the batch commands</returns>
        public Dictionary<int, CommandResult[]> RunBatchJob(params Action[] actions)
        {
            // enqueue all of the command
            _IsBatchingCommands_ = true;
            foreach (Action a in actions)
                a();

            // generate one large command string and reset the queue
            StringBuilder cmdBuilder = new StringBuilder();
            for (int i = 0; i < _BatchCommandQueue_.Count; i++)
            {
                cmdBuilder.AppendLine(_BatchCommandQueue_[i]);
                cmdBuilder.Append("\"end batched automation command " + i.ToString() + "\";" + (i < (_BatchCommandQueue_.Count - 1) ? "\n" : ""));
            }
            _BatchCommandQueue_ = new List<string>();

            // runs all of the commands at once
            _IsBatchingCommands_ = false;
            CommandResult[] results = IssueiOSAutomationCommand(cmdBuilder.ToString());

            // separate out the results
            Dictionary<int, CommandResult[]> resultsDict = new Dictionary<int, CommandResult[]>();
            List<CommandResult> lastCommandResults = new List<CommandResult>();
            int lastCommandIndex = 0;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].CommandOutput == "end batched automation command " + lastCommandIndex.ToString())
                {
                    resultsDict[lastCommandIndex] = lastCommandResults.ToArray();
                    lastCommandIndex++;
                    lastCommandResults = new List<CommandResult>();
                }
                else
                {
                    lastCommandResults.Add(results[i]);
                }
            }
            return resultsDict;
        }

        /// <summary>delays within the ios automation server</summary>
        /// <param name="seconds">amount of time to delay in seconds</param>
        public void Delay(double seconds)
        {
            IssueiOSAutomationCommand("delay(" + seconds.ToString() + ");");
        }

        /// <summary>gets the hook</summary>
        /// <param name="hook">the ios hook to get</param>
        /// <returns>the javascript object associated with the hook (as a string)</returns>
        public string Get(iOSHook hook)
        {
            return IssueiOSAutomationCommand(hook.ToString() + ";")[0].CommandOutput;
        }

        /// <summary>flattens the contents of a UIAElementArray to a string</summary>
        /// <param name="hook">hook for the UIAElementArray</param>
        /// <returns>UIAElementArray as a string</returns>
        public string GetElementArrayContents(iOSHook hook)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var items=" + hook.ToString() + ";");
            sb.Append("var output = \"[\";");
            sb.Append("for (var i = 0; i < items.length; i++)");
            sb.Append("{");
            sb.Append("if (i != 0) { output = output + \", \"; }");
            sb.Append("output=output+items[i].name();");
            sb.Append("}");
            sb.Append("output + \"]\";");
            return IssueiOSAutomationCommand(sb.ToString())[0].CommandOutput;
        }

        /// <summary>taps an element</summary>
        /// <param name="hook">element to tap</param>
        /// <returns>true if successful</returns>
        public bool Tap(iOSHook hook)
        {
            return IssueiOSAutomationCommand(hook.ToString() + ".tap();")[0].WasSuccessful;
        }

        /// <summary>scroll to an element</summary>
        /// <param name="hook">element to scroll to</param>
        /// <returns>true if successful</returns>
        public bool ScrollTo(iOSHook hook)
        {
            return IssueiOSAutomationCommand(hook.ToString() + ".scrollToVisible();")[0].WasSuccessful;
        }
        /// <summary>sets the value of an element</summary>
        /// <param name="hook">element whose value will be set</param>
        /// <param name="newValue">new value</param>
        /// <returns>true if successful</returns>
        public bool SetValue(iOSHook hook, string newValue)
        {
            string escapedText = newValue.Replace("\"", "\\\"");
            return IssueiOSAutomationCommand(hook.ToString() + ".setValue(\"" + escapedText + "\");")[0].WasSuccessful;
        }

        /// <summary>gets the value of the supplied element</summary>
        /// <param name="hook">element</param>
        /// <returns>value of the element</returns>
        public string GetValue(iOSHook hook)
        {
            CommandResult cr = IssueiOSAutomationCommand(hook.ToString() + ".value();")[0];
            return (cr.WasSuccessful) ? cr.CommandOutput : null;
        }
		
		/// <summary>gets the name of the supplied element</summary>
        /// <param name="hook">element</param>
        /// <returns>name of the element</returns>
        public string GetName(iOSHook hook)
        {
            CommandResult cr = IssueiOSAutomationCommand(hook.ToString() + ".name();")[0];
            return (cr.WasSuccessful) ? cr.CommandOutput : null;
        }

        /// <summary>waits for an element to be non-null</summary>
        /// <param name="hook">element</param>
        /// <param name="maxTimeInMilliseconds">maximum amount of time to wait in millisecond</param>
        /// <returns>true if the element is not null in the supplied timeframe</returns>
        public bool WaitForNotNull(iOSHook hook, int maxTimeInMilliseconds=30000)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var result = false; var startTime = new Date().getTime(); while(new Date().getTime() - startTime < ");
            sb.Append(maxTimeInMilliseconds.ToString());
            sb.Append(") { try { var e = ");
            sb.Append(hook.ToString());
            sb.Append("; if (e != null && e.toString() != \"[object UIAElementNil]\") { result = true; startTime = 0; } } catch(e) { } } result;");
            CommandResult cr = IssueiOSAutomationCommand(sb.ToString())[0];
            if (cr.WasSuccessful)
                return cr.CommandOutput.ToLower().StartsWith("true");
            else
                throw new Exception(cr.CommandOutput);
        }

        /// <summary>checks if a ui element is null</summary>
        /// <param name="hook">element</param>
        /// <returns>true if the element is null</returns>
        public bool CheckIfNull(iOSHook hook)
        {
            CommandResult cr = IssueiOSAutomationCommand(hook.ToString() + " == null || " + hook.ToString() +".toString() == \"[object UIAElementNil]\"")[0];
            if (cr.WasSuccessful)
                return cr.CommandOutput.ToLower().StartsWith("true");
            else
                throw new Exception(cr.CommandOutput);
        }

        /// <summary>captures a screenshot</summary>
        /// <param name="fileName">filename where the screenshot will be saved</param>
        /// <returns>true if successful</returns>
        public bool CaptureScreenshot(string fileName)
        {
            Log.Verbose("Screenshot saved at: " + fileName);
            return Utility.SystemOperations.RunShellCommand("/usr/sbin/screencapture", "-m " + fileName).WasSuccessful;
        }

        /// <summary>called to terminate use of the client</summary>
        public void Teardown()
        {
            if (IsAutomationRunning())
                StopAutomation();
        }
    }
}