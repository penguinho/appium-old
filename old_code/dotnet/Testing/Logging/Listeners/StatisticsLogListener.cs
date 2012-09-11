namespace Automation.Testing.Logging.Listeners
{
    /// <summary>logs statistics</summary>
    public class StatisticsLogListener : BaseLogListener
    {
        /// <summary>true if the listener is listening</summary>
        private bool _Listening;
        /// <summary>true if the log has not logged any failures or errors</summary>
        public bool HasNoErrorsOrFailures { get { return _NumErrors + _NumFailures == 0; } }
        /// <summary>number of passes logged</summary>
        public int Passes { get { return _NumPasses; } }
        private int _NumPasses;
        /// <summary>number of failures logged</summary>
        public int Failures { get { return _NumFailures; } }
        private int _NumFailures;
        /// <summary>number of warnings logged</summary>
        public int Warnings { get { return _NumWarnings; } }
        private int _NumWarnings;
        /// <summary>number of errors logged</summary>
        public int Errors { get { return _NumErrors; } }
        private int _NumErrors;

        /// <summary>constructor</summary>
        public StatisticsLogListener()
        {
            _NumPasses = 0;
            _NumFailures = 0;
            _NumErrors = 0;
            _NumWarnings = 0;
            _Listening = true;
        }

        /// <summary>logs an entry</summary>
        /// <param name="entry">entry to log</param>
        public override void AddEntry(LogEntry entry)
        {
            if (!_Listening)
                return;

            switch (entry.EntryType)
            {
                case LogEntryType.SUCCESS: _NumPasses++; break;
                case LogEntryType.FAILURE: _NumFailures++; break;
                case LogEntryType.WARNING: _NumWarnings++; break;
                case LogEntryType.ERROR:
                case LogEntryType.C_ERROR: _NumErrors++; break;
            }
        }

        /// <summary>adds an attachment to the log</summary>
        /// <param name="content">content of the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public override void AddAttachment(string content, string suffix) { return; }

        /// <summary>closes the listener</summary>
        public override void Close() { _Listening = false; }
    }
}
