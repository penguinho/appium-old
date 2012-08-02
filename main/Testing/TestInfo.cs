using Automation.Testing.Logging;
using Automation.Testing.Logging.Listeners;
using System;

namespace Automation.Testing
{
    /// <summary>stores information about a test</summary>
    public class TestInfo
    {
        /// <summary>test object</summary>
        private Test _TestObject = null;
        /// <summary>full path to the test class</summary>
        public string FullPathToTestClass;

        /// <summary>name of the test</summary>
        public string Name { get { return _TestObject.Name; } }
        /// <summary>log used by the test</summary>
        public LogEngine Log { get { return _TestObject.Log; } }
        /// <summary>place to attach a log file listener</summary>
        public FileSystemListener LogFileListener { get; set; }
        /// <summary>test class for the test info object</summary>
        public Type TestClass;

        /// <summary>constructor</summary>
        /// <param name="testClass">class for the test</param>
        /// <param name="fullPathToTestClass">(for logging purposes only) full path to the test class (will autodetect from the type if null)</param>
        public TestInfo(Type testClass, string fullPathToTestClass = null)
        {
            TestClass = testClass;
            FullPathToTestClass = fullPathToTestClass ?? testClass.FullName;
        }

        /// <summary>constructor</summary>
        /// <param name="fullPathToTestClass">full path to the test class</param>
        public TestInfo(string fullPathToTestClass)
        {
            FullPathToTestClass = fullPathToTestClass;
            TestClass = Type.GetType(fullPathToTestClass);
        }

        /// <summary>initializes the test so that it can be run</summary>
        /// <returns>true if successful</returns>
        public virtual bool Initialize()
        {
            return Initialize<Test>();
        }

        /// <summary>initialize the test so that it can be run</summary>
        /// <typeparam name="T">type of the test</typeparam>
        /// <param name="testClass">the test class to be created</param>
        /// <param name="args">object array of arguments to pass to the contructor when creating the test</param>
        /// <returns>true if successful</returns>
        public virtual bool Initialize<T>(params object[] args) where T : Test
        {
            if (IsInitialized)
                return true;

            try
            {
                // create the test class using reflection
                
                _TestObject = (args != null && args.Length > 0) ? (T)Activator.CreateInstance(TestClass, args) : (T)Activator.CreateInstance(TestClass);
                return true;
            }
            catch (Exception e) { return false; }
        }

        /// <summary>true if the test has been initialized</summary>
        public bool IsInitialized { get { return _TestObject != null; } }

        /// <summary>sets up the test class</summary>
        /// <returns>true if successful</returns>
        public bool ClassSetup() { return _TestObject.ClassSetup(); }

        /// <summary>runs the test</summary>
        /// <returns>true if test set up completes</returns>
        public bool Setup() { return _TestObject.Setup(); }

        /// <summary>runs the test</summary>
        /// <returns>true if test execution completes</returns>
        public bool Run() { return _TestObject.Run(); }

        /// <summary>tears down the test</summary>
        /// <returns>true if test tear down completes</returns>
        public bool Teardown() { return _TestObject.Teardown(); }

        /// <summary>tears down the test class</summary>
        /// <returns>true if test class tear down completes</returns>
        public bool ClassTeardown() { return _TestObject.ClassTeardown(); }
    }
}
