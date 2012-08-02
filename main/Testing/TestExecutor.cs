using Automation.Testing.Logging;
using Automation.Testing.Logging.Listeners;
using System;
using System.IO;

namespace Automation.Testing
{
    /// <summary>used to execute tests</summary>
    public class TestExecutor
    {
        /// <summary>main log engine for this test executor</summary>
        public LogEngine Log;
        /// <summary>the log file listener object</summary>
        protected FileSystemListener _LogFileListener_;

        /// <summary>delegate for an event handler which fires after the main log has been setup</summary>
        public delegate void OnMainLogSetupCompletedHandler();
        /// <summary>delegate for an event handler which fires after the test log has been setup</summary>
        /// <param name="test">information about the test</param>
        public delegate void OnTestLogSetupCompletedHandler(TestInfo test);
        /// <summary>delegate for an event handler which fires after all tests have been executed</summary>
        /// <param name="tests">list of tests that have been executed</param>
        public delegate void OnAllTestsHaveBeenExecutedHandler(TestInfo[] tests);
        /// <summary>delegate for an event handler which fires after an individual tests has been executed</summary>
        /// <param name="didRun">true if the test executed</param>
        /// <param name="test">information about the test</param>
        public delegate void OnTestExecutionCompletedHandler(bool didRun, TestInfo test);

        /// <summary>fires when the setup for the main log has completed</summary>
        public event OnMainLogSetupCompletedHandler OnMainLogSetupCompleted;
        /// <summary>fires when the setup for the test log has completed</summary>
        public event OnTestLogSetupCompletedHandler OnTestLogSetupCompleted;
        /// <summary>fires when an individual test has completed execution</summary>
        public event OnTestExecutionCompletedHandler OnTestExecutionCompleted;
        /// <summary>fires once every test has been executed</summary>
        public event OnAllTestsHaveBeenExecutedHandler OnAllTestsHaveBeenExecuted;

        /// <summary>contructor</summary>
        public TestExecutor()
        {
        }

        /// <summary>executes tests</summary>
        /// <param name="name">name for the run</param>
        /// <param name="handleTestExceptions">true if exceptions thrown inside tests will be logged as errors instead of thrown</param>
        /// <param name="tests">list of tests to run</param>
        public void Execute(string name="Untitled", bool handleTestExceptions=true, params TestInfo[] tests)
        {
            _SetupMainLog(name);
            _RunTests(handleTestExceptions, tests);
            if (OnAllTestsHaveBeenExecuted != null)
                OnAllTestsHaveBeenExecuted(tests);
            _TearDownMainLog();
        }

        /// <summary>sets up the main automation log</summary>
        /// <param name="name">name of the run</param>
        private void _SetupMainLog(string name)
        {
            // name the log
            string mainLogName = "Automation Run @ \"" + name + "\" " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            // attempt to mount the automation log share and create log files
            string mainLogPath = Path.GetTempPath();

            // add log file listeners so that the log will be written to disk
            _LogFileListener_ = new FileSystemListener(mainLogName, mainLogPath, true, true);
            Log = new LogEngine(mainLogName, _LogFileListener_, new ConsoleLogListener());
            
            // call main log setup completed handler
            if (OnMainLogSetupCompleted != null)
                OnMainLogSetupCompleted();

            // write out the log file locations
            if (_LogFileListener_.TextLogFilePath != null)
                Log.Marker("Main Text Log Path: " + _LogFileListener_.TextLogFilePath);
            if (_LogFileListener_.HTMLLogFilePath != null)
                Log.Marker("Main HTML Log Path: " + _LogFileListener_.HTMLLogFilePath);
        }

        /// <summary>runs the supplied tests</summary>
        /// <param name="handleTestExceptions">true if exceptions thrown inside tests will be logged as errors instead of thrown</param>
        /// <param name="tests">test to run</param>
        private void _RunTests(bool handleTestExceptions, TestInfo[] tests)
        {

            // check that tests were found
            if (null == tests || tests.Length == 0)
                Log.CriticalError("No tests were found.");

            // run the tests
            Log.Marker("Beginning Test Execution");

            int indexOfCurrentlyExecutingTest = 1;
            int paddingForTestNumber = Convert.ToInt32(Math.Floor(Math.Log10(tests.Length)) + 1);
            foreach (TestInfo test in tests)
            {
                // attempt to create the test
                if (test.Initialize())
                {
                    // add log file listener for the test
                    string testLogPath = _LogFileListener_.RootDirectory != null ? Path.Combine(_LogFileListener_.RootDirectory, indexOfCurrentlyExecutingTest.ToString().PadLeft(paddingForTestNumber, '0')) : Path.GetTempPath();
                    Directory.CreateDirectory(testLogPath);
                    test.LogFileListener = new FileSystemListener(test.Name, testLogPath, true, true);
                    if (test.LogFileListener.TextLogFilePath != null)
                        Log.Marker("Test Text Log Path: " + test.LogFileListener.TextLogFilePath);
                    if (test.LogFileListener.HTMLLogFilePath != null)
                        Log.Marker("Test HTML Log Path: " + test.LogFileListener.HTMLLogFilePath);
                    test.Log.AddListener(test.LogFileListener);

                    if (OnTestLogSetupCompleted != null)
                        OnTestLogSetupCompleted(test);

                    // launch test
                    if (!handleTestExceptions)
                    {
                        // throw all exceptions to the ide
                        test.Log.Marker("Beginning Test Class Setup");
                        test.ClassSetup();
                        test.Log.Marker("Completed Test Class Setup");
                        test.Log.Marker("Beginning Test Setup");
                        test.Setup();
                        test.Log.Marker("Completed Test Setup");
                        test.Log.Marker("Beginning Test Execution");
                        test.Run();
                        test.Log.Marker("Completed Test Execution");
                        test.Log.Marker("Beginning Test Teardown");
                        test.Teardown();
                        test.Log.Marker("Completed Test Teardown");
                        test.Log.Marker("Beginning Test Class Teardown");
                        test.ClassTeardown();
                        test.Log.Marker("Completed Test Class Teardown");
                    }
                    else
                    {
                        // report all exceptions in the log
                        try
                        {
                            test.Log.Marker("Beginning Test Class Setup");
                            test.ClassSetup();
                            test.Log.Marker("Completed Test Class Setup");
                            test.Log.Marker("Beginning Test Setup");
                            test.Setup();
                            test.Log.Marker("Completed Test Setup");
                            test.Log.Marker("Beginning Test Execution");
                            test.Run();
                            test.Log.Marker("Completed Test Execution");
                        }
                        catch (Exception e) { test.Log.Error(e.ToString()); }
                        try
                        {
                            test.Log.Marker("Beginning Test Teardown");
                            test.Teardown();
                            test.Log.Marker("Completed Test Teardown");
                        }
                        catch (Exception e) { test.Log.Error(e.ToString()); }
                        try
                        {
                            test.Log.Marker("Beginning Test Class Teardown");
                            test.ClassTeardown();
                            test.Log.Marker("Completed Test Class Teardown");
                        }
                        catch (Exception e) { test.Log.Error(e.ToString()); }
                    }

                    // close out the log for the test so nothing else can be written to it
                    test.Log.CloseAllListeners();

                    // add the test results to the logs and to the automation system
                    Log.Verify(true, test.Log.HasNoErrorsOrFailures,
                        indexOfCurrentlyExecutingTest.ToString().PadLeft(paddingForTestNumber, '0') + ".) " + test.Name,
                        false);
                    if (OnTestExecutionCompleted != null)
                        OnTestExecutionCompleted(true, test);
                }
                else
                {
                    // add results to the logs and the automation system
                    Log.Fail(indexOfCurrentlyExecutingTest.ToString().PadLeft(paddingForTestNumber, '0') + ".) " + test.FullPathToTestClass);
                    if (OnTestExecutionCompleted != null)
                        OnTestExecutionCompleted(false, test);
                }

                // increment the test counter
                indexOfCurrentlyExecutingTest++;
            }

            Log.Marker("Completed Test Execution");
        }

        /// <summary>tearsdown the automation log</summary>
        private void _TearDownMainLog()
        {
            // close all log listeners if they were not closed earlier
            Log.CloseAllListeners();
        }
    }
}
