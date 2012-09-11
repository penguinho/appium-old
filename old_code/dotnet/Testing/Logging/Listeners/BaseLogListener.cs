namespace Automation.Testing.Logging.Listeners
{
    /// <summary>interface to be able to store log data</summary>
    public abstract class BaseLogListener
    {
        /// <summary>log engine which will be communicating with this listener</summary>
        public LogEngine Sender { get; set; }

        /// <summary>logs an entry</summary>
        /// <param name="entry">entry to log</param>
        public abstract void AddEntry(LogEntry entry);

        /// <summary>logs an attachment</summary>
        /// <param name="filePath">path to the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public abstract void AddAttachment(string filePath, string suffix);

        /// <summary>closes the listener</summary>
        public abstract void Close();
    }
}
