using Automation.Testing;
using System;

namespace TestProgram
{
    /// <summary>main program</summary>
    class Program
    {
        /// <summary>main method</summary>
        /// <param name="args">command line arguments</param>
        static void Main(string[] args)
        {
            // run the test
            TestExecutor executor = new TestExecutor();
            executor.Execute("Run of Sample Test", false, new TestInfo(typeof(TestProgram.SampleTest)));

            // wait for the user to press return before exiting
            Console.WriteLine();
            Console.WriteLine("Press Return To Exit...");
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
