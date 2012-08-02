using Automation.Testing.Logging;

namespace Automation.Testing
{
    /// <summary>base class for tests that can be run</summary>
    public abstract class Test
    {
        /// <summary>stores log information</summary>
        public LogEngine Log { get; set; }
        /// <summary>name of the test</summary>
        abstract public string Name { get; }
        
        /// <summary>constructor</summary>
        public Test()
        {
            Log = new LogEngine();
        }

        /// <summary>initializes the test client</summary>
        /// <returns>true if initialization completes</returns>
        public virtual bool ClassSetup() { return true; }

        /// <summary>sets up the test</summary>
        /// <returns>true if successful</returns>
        public virtual bool Setup() { return true; }

        /// <summary>executes the test</summary>
        /// <returns>true if text execution completes</returns>
        public abstract bool Run();

        /// <summary>tears down the test</summary>
        /// <returns>true if successful</returns>
        public virtual bool Teardown() { return true; }

        /// <summary>finalizes the test client</summary>
        /// <returns>true if tear down completes</returns>
        public virtual bool ClassTeardown() { return true; }
    }
}
