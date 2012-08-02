using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Automation.Testing.Logging.Listeners
{
    /// <summary>log listener which which write a log to a file</summary>
    public class FileSystemListener : BaseLogListener
    {
        /// <summary>true if the listener is listening</summary>
        private bool _Listening;
        /// <summary>directory being logged to</summary>
        public string RootDirectory;
        /// <summary>path to the text version of the log</summary>
        public string TextLogFilePath { get { return _TextLogFilePath; } }
        private string _TextLogFilePath;
        /// <summary>stream writer for the text log</summary>
        private StreamWriter TextLogWriter;
        /// <summary>path to the html version of the log</summary>
        public string HTMLLogFilePath { get { return _HTMLLogFilePath; } }
        private string _HTMLLogFilePath;
        /// <summary>stream writer for the html log</summary>
        private XmlTextWriter HTMLLogWriter;
        /// <summary>true if an html log is being written</summary>
        public bool IsLoggingAsHTML { get { return HTMLLogWriter != null; } }
        /// <summary>true if an text log is being written</summary>
        public bool IsLoggingAsText { get { return TextLogWriter != null; } }
        /// <summary>true if attachments are being logged</summary>
        public bool IsLoggingAttachments { get; set; }
        /// <summary>list of attachment paths associated with this log</summary>
        public List<string> AttachmentPaths;

        /// <summary>true if the log has not logged any failures or errors</summary>
        public bool Success { get { return _NumErrors + _NumFailures == 0; } }
        /// <summary>number of passes logged</summary>
        public int Passes { get { return _NumPasses; } }
        private int _NumPasses = 0;
        /// <summary>number of failures logged</summary>
        public int Failures { get { return _NumFailures; } }
        private int _NumFailures = 0;
        /// <summary>number of warnings logged</summary>
        public int Warnings { get { return _NumWarnings; } }
        private int _NumWarnings = 0;
        /// <summary>number of errors logged</summary>
        public int Errors { get { return _NumErrors; } }
        private int _NumErrors = 0;

        /// <summary>constructor</summary>
        /// <param name="logName">name of the log</param>
        /// <param name="directory">directory where the log contents are being written</param>
        /// <param name="createTextLog">true if a text log should be created</param>
        /// <param name="createHTMLLog">true if an html log should be created</param>
        /// <param name="saveAttachments">true if attachments should be saved</param>
        public FileSystemListener(string logName, string directory = null, bool createTextLog = true, bool createHTMLLog = false, bool saveAttachments = false)
        {
            _Listening = true;

            try
            {
                if (null == directory)
                {
                    RootDirectory = Path.GetTempPath();
                    _TextLogFilePath = createTextLog ? Path.GetTempFileName() : null;
                    _HTMLLogFilePath = createHTMLLog ? Path.GetTempFileName() : null;
                }
                else
                {
                    RootDirectory = directory;
                    _TextLogFilePath = createTextLog ? Path.Combine(RootDirectory, "automation_log.txt") : null;
                    _HTMLLogFilePath = createHTMLLog ? Path.Combine(RootDirectory, "automation_log.html") : null;
                }

                if (!Directory.Exists(RootDirectory))
                    Directory.CreateDirectory(RootDirectory);

                if (createTextLog)
                {
                    TextLogWriter = new StreamWriter(TextLogFilePath);
                }
                if (createHTMLLog)
                {
                    HTMLLogWriter = new XmlTextWriter(HTMLLogFilePath, Encoding.ASCII);
                    HTMLLogWriter.WriteDocType("html", null, null, null);
                    HTMLLogWriter.WriteStartElement("html");
                    HTMLLogWriter.WriteStartElement("head");
                    HTMLLogWriter.WriteStartElement("title");
                    HTMLLogWriter.WriteString(logName);
                    HTMLLogWriter.WriteEndElement(); //title
                    HTMLLogWriter.WriteEndElement(); //head
                    HTMLLogWriter.WriteStartElement("body");
                    HTMLLogWriter.WriteStartElement("h1");
                    HTMLLogWriter.WriteAttributeString("style", "font:Courier; font-family:Courier");
                    HTMLLogWriter.WriteString(logName);
                    HTMLLogWriter.WriteEndElement(); //h1
                    HTMLLogWriter.WriteStartElement("p");
                    HTMLLogWriter.Flush();
                }

                IsLoggingAttachments = saveAttachments;
                AttachmentPaths = new List<string>();
            }
            catch { Close(); }
        }

        /// <summary>adds an entry to the log</summary>
        /// <param name="entry">entry to add</param>
        public override void AddEntry(LogEntry entry)
        {
            if (!_Listening)
                return;
            try
            {
                if (IsLoggingAsText)
                {
                    TextLogWriter.WriteLine(entry.ToString());
                    TextLogWriter.Flush();
                }

                if (IsLoggingAsHTML)
                {
                    string color;
                    switch (entry.EntryType)
                    {
                        case LogEntryType.VERBOSE: color = "gray"; break;
                        case LogEntryType.DEBUG: color = "dark-gray"; break;
                        case LogEntryType.MARKER: color = "purple"; break;
                        case LogEntryType.SUCCESS: color = "green"; _NumPasses++; break;
                        case LogEntryType.FAILURE: color = "red"; _NumFailures++; break;
                        case LogEntryType.ERROR:
                        case LogEntryType.C_ERROR: color = "red"; _NumErrors++; break;
                        case LogEntryType.WARNING: color = "orange"; _NumWarnings++; break;
                        case LogEntryType.COMMENT: color = "blue"; break;
                        default: color = "black"; break;
                    }

                    // write the entry's remark
                    HTMLLogWriter.WriteStartElement("span");
                    HTMLLogWriter.WriteAttributeString("style", "white-space:pre; color:" + color);
                    HTMLLogWriter.WriteString(entry.ToString());
                    HTMLLogWriter.WriteEndElement(); // span
                    HTMLLogWriter.WriteStartElement("br");
                    HTMLLogWriter.WriteEndElement(); // br
                    HTMLLogWriter.Flush();
                }
            }
            catch { Close(); }
        }

        /// <summary>adds an attachment to the log</summary>
        /// <param name="filePath">path to the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public override void AddAttachment(string filePath, string suffix)
        {
            if (!_Listening)
                return;
            try { AttachmentPaths.Add(filePath); }
            catch { Close(); }
        }

        /// <summary>closes the listener</summary>
        public override void Close()
        {
            try
            {
                if (IsLoggingAsText)
                {
                    TextLogWriter.Flush();
                    TextLogWriter.Close();
                    TextLogWriter = null;
                }

                if (IsLoggingAsHTML)
                {
                    HTMLLogWriter.WriteStartElement("br");
                    HTMLLogWriter.WriteEndElement();
                    HTMLLogWriter.WriteString("Passes: " + _NumPasses.ToString());
                    HTMLLogWriter.WriteStartElement("br");
                    HTMLLogWriter.WriteEndElement();
                    HTMLLogWriter.WriteString("Failures: " + _NumFailures.ToString());
                    HTMLLogWriter.WriteStartElement("br");
                    HTMLLogWriter.WriteEndElement();
                    HTMLLogWriter.WriteString("Warnings: " + _NumWarnings.ToString());
                    HTMLLogWriter.WriteStartElement("br");
                    HTMLLogWriter.WriteEndElement();
                    HTMLLogWriter.WriteString("Errors: " + _NumErrors.ToString());
                    HTMLLogWriter.WriteEndElement(); // p
                    HTMLLogWriter.WriteEndElement(); // body
                    HTMLLogWriter.WriteEndElement(); //html
                    HTMLLogWriter.Flush();
                    HTMLLogWriter.Close();
                    HTMLLogWriter = null;
                }
            }
            catch { }
            _Listening = false;
        }
    }
}
