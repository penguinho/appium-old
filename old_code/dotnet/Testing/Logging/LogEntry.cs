using System;
using System.Text;
using System.Xml.Serialization;

namespace Automation.Testing.Logging
{
    /// <summary>represents an entry in the log</summary>
    public class LogEntry
    {
        /// <summary>type of entry</summary>
        [XmlAttribute("type")]
        public LogEntryType EntryType { get; set; }

        /// <summary>time of entry</summary>
        [XmlAttribute("time")]
        public DateTime EntryTime { get; set; }

        /// <summary>remark associated with the log entry</summary>
        [XmlElement("remark")]
        public string EntryRemark { get; set; }

        /// <summary>parameterless contructor (required for serialization)</summary>
        private LogEntry()
        {
        }

        /// <summary>constrcutor</summary>
        /// <param name="type">type of the entry</param>
        /// <param name="remark">remark associated with the log entry</param>
        public LogEntry(LogEntryType type, string remark)
        {
            EntryTime = DateTime.Now;
            EntryType = type;
            EntryRemark = remark;
        }

        /// <summary>string representation of the log entry</summary>
        /// <returns></returns>
        public override string ToString()
        {
            string enumName = Enum.GetName(typeof(LogEntryType), EntryType);
            StringBuilder sb = new StringBuilder();
            sb.Append(EntryTime.ToLongTimeString());
            sb.Append(" - ");
            sb.Append(enumName);
            // Add trailing whitepsace (if necessary) to line up with 7 letter log types
            for (int i = enumName.Length; i < 7; i++)
                sb.Append(" ");
            sb.Append(" - ");
            sb.Append(EntryRemark);
            return sb.ToString();
        }
    }
}
