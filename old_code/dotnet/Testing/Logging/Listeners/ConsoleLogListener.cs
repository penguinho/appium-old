using System;

namespace Automation.Testing.Logging.Listeners
{
    /// <summary>logs info to the console</summary>
    public class ConsoleLogListener : BaseLogListener
    {
        /// <summary>true if the listener is listening</summary>
        private bool _Listening;

        /// <summary>constructor</summary>
        public ConsoleLogListener()
        {
            _Listening = true;
        }

        /// <summary>logs the entry to the console</summary>
        /// <param name="entry">entry to log</param>
        public override void AddEntry(LogEntry entry)
        {
            if (!_Listening)
                return;

            // note original console colors
            ConsoleColor initialForegroundColor = Console.ForegroundColor;
            ConsoleColor initialBackgroundColor = Console.BackgroundColor;

            // set console colors based on the type of information that is being written
            switch (entry.EntryType)
            {
                case LogEntryType.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.FAILURE:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.COMMENT:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.C_ERROR:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
                case LogEntryType.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.VERBOSE:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogEntryType.DEBUG:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    break;
                case LogEntryType.MARKER:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    break;
                case LogEntryType.INFO:
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
            }

            // write the entry to the console
            Console.Write(entry.ToString());

            // reset the console colors and write the end of line
            Console.ForegroundColor = initialForegroundColor;
            Console.BackgroundColor = initialBackgroundColor;
            Console.WriteLine();
        }

        /// <summary>adds an attachment to the log</summary>
        /// <param name="filePath">path to the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public override void AddAttachment(string filePath, string suffix) { return; }

        /// <summary>closes the listener</summary>
        public override void Close() { _Listening = false; ; }
    }
}
