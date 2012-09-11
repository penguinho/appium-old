using System;

namespace Automation.Utility
{
    /// <summary>represents the result of a command </summary>
    public class CommandResult
    {
        /// <summary>constructor</summary>
        /// <param name="returnCode">return code</param>
        /// <param name="commandOutput">text output of the command</param>
        public CommandResult(int returnCode, string commandOutput)
        {
            ReturnCode = returnCode;
            CommandOutput = commandOutput;
        }

        /// <summary>constructor</summary>
        /// <param name="response">response in text format (e.g. 0,resultText)</param>
        public CommandResult(string response)
        {
            try
            {
                string[] parts = response.Split(new char[] { ',' }, 2);
                int returnCode = -42;
                Int32.TryParse(parts[0], out returnCode);
                ReturnCode = returnCode;
                CommandOutput = parts[1];

            }
            catch (Exception e)
            {
                ReturnCode = -42;
                CommandOutput = e.ToString();
            }
        }

        /// <summary>return code of the command</summary>
        public int ReturnCode { get; set; }

        /// <summary>text output of the command</summary>
        public string CommandOutput { get; set; }

        /// <summary>true if the command was successful</summary>
        public bool WasSuccessful { get { return ReturnCode == 0; } }
    }
}
