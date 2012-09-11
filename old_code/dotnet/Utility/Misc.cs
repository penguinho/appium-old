using System;

namespace Automation.Utility
{
    /// <summary>contains utility methods used widely across the code base</summary>
    public static class Misc
    {
        /// <summary>default amount of time (in milliseconds) to wait for wait statements</summary>
        public const int DefaultWaitTime = 30000;
        /// <summary>default interval (in milliseconds) to wait between polling</summary>
        public const int DefaultPollingInterval = 100;

        /// <summary>waits for a function to return a desired value</summary>
        /// <typeparam name="T">the return type</typeparam>
        /// <param name="F">the function to execute</param>
        /// <param name="desiredValue">the desired value that will terminate the wait</param>
        /// <param name="maxWaitInMilliseconds">maximum amount of time to wait in milliseconds</param>
        /// <param name="pollingIntervalInMilliseconds">time to wait between polling actions in milliseconds</param>
        /// <returns>true if the function evaluates the desired value within the time limit</returns>
        public static bool WaitWithTimeout<T>(Func<T> F, T desiredValue, int maxWaitInMilliseconds = DefaultWaitTime, int pollingIntervalInMilliseconds = DefaultPollingInterval)
        {
            DateTime startTime = DateTime.Now;
            while (DateTime.Now.Subtract(startTime).TotalMilliseconds < maxWaitInMilliseconds)
            {
                // return true if the function evaluates tp the desired value
                if (F().Equals(desiredValue))
                    return true;
                // sleep before checking again
                System.Threading.Thread.Sleep(pollingIntervalInMilliseconds);
            }
            // give it one last chance
            return F().Equals(desiredValue);
        }
    }
}