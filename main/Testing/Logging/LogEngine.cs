using System;
using System.Collections.Generic;
using Automation.Testing.Logging.Listeners;

namespace Automation.Testing.Logging
{
    /// <summary>used to log information</summary>
    public class LogEngine
	{
        /// <summary>list of log listeners</summary>
        public StatisticsLogListener Statistics;
        /// <summary>name of the log engine</summary>
        public string Name;
        /// <summary>true if the log has not logged any failures or errors</summary>
        public bool HasNoErrorsOrFailures { get { return Statistics.HasNoErrorsOrFailures; } }
        /// <summary>list of all log listeners</summary>
        private List<BaseLogListener> _Listeners;

        /// <summary>constructor</summary>
        /// <param name="name">name of the log</param>
        /// <param name="logListeners">array of log listeners which should be hooked up to the log engine</param>
		public LogEngine (string name="unnamed log", params BaseLogListener[] logListeners)
		{
            // name the log engine
            Name = name;
            Statistics = new StatisticsLogListener();
            _Listeners = new List<BaseLogListener>();
            _Listeners.Add(Statistics);

            if (logListeners.Length < 1)
            {
                // add default listeners
                _Listeners.Add(new ConsoleLogListener());
            }
            else
            {
                // add supplied custom listeners
                foreach (BaseLogListener consumer in logListeners)
                    _Listeners.Add(consumer);
            }
		}
		
        /// <summary>adds an entry to the log</summary>
        /// <param name="entry">the entry to add</param>
		private void _AddEntry(LogEntry entry)
		{
            foreach (BaseLogListener logConsumer in _Listeners)
                logConsumer.AddEntry(entry);
		}

        /// <summary>logs a success</summary>
        /// <param name="remark">remark associated with the log entry</param>
		public void Pass(string remark)
		{
            _AddEntry(new LogEntry(LogEntryType.SUCCESS, remark));
		}

        /// <summary>logs a failure</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public virtual void Fail(string remark)
		{
			_AddEntry(new LogEntry(LogEntryType.FAILURE, remark));
		}

        /// <summary>logs an informational entry</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Info(string remark)
		{
            _AddEntry(new LogEntry(LogEntryType.INFO, remark));
		}

        /// <summary>logs a comment</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Comment(string remark)
		{
            _AddEntry(new LogEntry(LogEntryType.COMMENT, remark));
		}

        /// <summary>logs an error</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public virtual void Error(string remark)
		{
            _AddEntry(new LogEntry(LogEntryType.ERROR, remark));
		}

        /// <summary>logs a warning</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Warning(string remark)
        {
            _AddEntry(new LogEntry(LogEntryType.WARNING, remark));
        }

        /// <summary>logs a verbose informational entry</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Verbose(string remark)
        {
            _AddEntry(new LogEntry(LogEntryType.VERBOSE, remark));
        }

        /// <summary>logs a debug informational entry</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Debug(string remark)
        {
            _AddEntry(new LogEntry(LogEntryType.DEBUG, remark));
        }

        /// <summary>logs a critical error</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public virtual void CriticalError(string remark)
        {
            _AddEntry(new LogEntry(LogEntryType.C_ERROR, remark));
            throw new Exception("Critical Error: " + remark);
        }

        /// <summary>logs a procedural entry</summary>
        /// <param name="remark">remark associated with the log entry</param>
        public void Marker(string remark)
        {
            _AddEntry(new LogEntry(LogEntryType.MARKER, remark));
        }
		
        /// <summary>logs an error if the supplied values are not equal</summary>
        /// <param name="expectedValue">expected value</param>
        /// <param name="actualValue">actual value</param>
        /// <param name="remark">remark associated with the log entry</param>
        /// <returns></returns>
        public bool Assert(IComparable expectedValue, IComparable actualValue, string remark)
        {
            if (expectedValue.CompareTo(actualValue) != 0)
            {
                Fail(remark + " --- EXPECTED: " + expectedValue.ToString() + " ACTUAL: " + actualValue.ToString());
                return false;
            }
            return true;
        }

        /// <summary>logs an error if the supplied values are not equal</summary>
        /// <param name="expectedValue">expected value</param>
        /// <param name="actualValue">actual value</param>
        /// <param name="remark">remark associated with the log entry</param>
        /// <returns></returns>
        public bool AssertCritical(IComparable expectedValue, IComparable actualValue, string remark)
        {
            if (expectedValue.CompareTo(actualValue) != 0)
            {
                CriticalError(remark + " --- EXPECTED: " + expectedValue.ToString() + " ACTUAL: " + actualValue.ToString());
                return false;
            }
            return true;
        }
		
        /// <summary>logs a failure if supplied values are not equal, a pass otherwise</summary>
        /// <param name="expectedValue">expected value</param>
        /// <param name="actualValue">actual value</param>
        /// <param name="remark">remark associated with the log entry</param>
        /// <param name="includeValuesInOutput">if true, the expected and actual values will be appended to the remark if they differ</param>
        /// <returns>true if successful</returns>
        public bool Verify(IComparable expectedValue, IComparable actualValue, string remark, bool includeValuesInOutput = true)
		{
            if (expectedValue.CompareTo(actualValue) == 0)
            {
                Pass(remark);
                return true;
            }
            else
            {
                if (includeValuesInOutput)
                    remark += " --- EXPECTED: " + expectedValue.ToString() + " ACTUAL: " + actualValue.ToString();
                Fail(remark);
                return false;
            }
		}

        /// <summary>logs a critical error if supplied values are not equal, a pass otherwise</summary>
        /// <param name="expectedValue">expected value</param>
        /// <param name="actualValue">actual value</param>
        /// <param name="remark">remark associated with the log entry</param>
        /// <param name="includeValuesInOutput">if true, the expected and actual values will be appended to the remark if they differ</param>
        /// <returns>true if successful</returns>
        public bool VerifyCritical(IComparable expectedValue, IComparable actualValue, string remark, bool includeValuesInOutput = true)
        {
            if (expectedValue.CompareTo(actualValue) == 0)
            {
                Pass(remark);
                return true;
            }
            else
            {
                if (includeValuesInOutput)
                    remark += " --- EXPECTED: " + expectedValue.ToString() + " ACTUAL: " + actualValue.ToString();
                CriticalError(remark);
                return false;
            }
        }

        /// <summary>validates the expected value is equal to the actual value</summary>
        /// <param name="expectedValue">expected value</param>
        /// <param name="actualValue">actual value</param>
        /// <param name="remark">remark associated with the log entry</param>
        /// <param name="logPasses">true if passes should be logged</param>
        /// <param name="isCritical">true if failures are critical</param>
        ///<returns>true if the expected value is equal to the actual value</returns>
        public bool Validate(IComparable expectedValue, IComparable actualValue, string remark, bool logPasses, bool isCritical)
        {
            if (logPasses)
                return (isCritical) ? VerifyCritical(expectedValue, actualValue, remark) : Verify(expectedValue, actualValue, remark);
            else
                return (isCritical) ? AssertCritical(expectedValue, actualValue, remark) : Assert(expectedValue, actualValue, remark);
        }

        /// <summary>adds an attachment to the log</summary>
        /// <param name="filePath">path of the attachment</param>
        /// <param name="suffix">suffix of the attachment</param>
        public void AddAttachment(string filePath, string suffix)
        {
            foreach (BaseLogListener listener in _Listeners)
                listener.AddAttachment(filePath, suffix);
        }

        /// <summary>adds a listener to the log engine</summary>
        /// <param name="listener">listener to add</param>
        public void AddListener(BaseLogListener listener)
        {
            listener.Sender = this;
            _Listeners.Add(listener);
        }

        /// <summary>removes a listener from the log engine</summary>
        /// <param name="listener">listener to remove</param>
        public void RemoveListnener(BaseLogListener listener)
        {
            _Listeners.Remove(listener);
        }

        /// <summary>closes all log listeners</summary>
        public void CloseAllListeners()
        {
            foreach (BaseLogListener listener in _Listeners)
                listener.Close();
        }
	}
}