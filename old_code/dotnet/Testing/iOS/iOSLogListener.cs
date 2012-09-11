using Automation.Testing.Logging;
using Automation.Testing.Logging.Listeners;
using System.IO;

namespace Automation.Testing.iOS
{
    /// <summary>used to log information</summary>
    public class iOSLogListener : BaseLogListener
    {
        /// <summary>selenium client object used by the log engine</summary>
        public iOSClient Client;

        public iOSLogListener(iOSClient client)
        {
            Client = client;
        }

        /// <summary>logs an entry</summary>
        /// <param name="entry">entry to log</param>
        public override void AddEntry(LogEntry entry)
        {
            switch (entry.EntryType)
            {
                case LogEntryType.C_ERROR:
                case LogEntryType.FAILURE:
                case LogEntryType.ERROR:
                    SaveScreenshotToTempFile();
                    break;
                default:
                    break;
            }
        }

        /// <summary>adds an attachment</summary>
        /// <param name="filePath">path to the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public override void AddAttachment(string filePath, string suffix) { }

        /// <summary>saves a screenshot to a tempfile</summary>
        public void SaveScreenshotToTempFile()
        {
            try
            {
                string tempFile = Path.GetTempFileName();
                Client.CaptureScreenshot(tempFile);
                Sender.AddAttachment(tempFile, "png");
            }
            catch { }
        }

        /// <summary>closes the listener</summary>
        public override void Close() { }
    }
}