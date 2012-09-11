namespace Automation.Testing.Logging
{
    /// <summary>denotes the type of log entry</summary>
    public enum LogEntryType
    {
        /// <summary>log entry related to a success</summary>
        SUCCESS,
        /// <summary>log entry related to a failure</summary>
        FAILURE,
        /// <summary>log entry related to an informational entry</summary>
        INFO,
        /// <summary>log entry related to a comment</summary>
        COMMENT,
        /// <summary>log entry related to an error</summary>
        ERROR,
        /// <summary>log entry related to a warning</summary>
        WARNING,
        /// <summary>log entry related to a verbose information entry</summary>
        VERBOSE,
        /// <summary>log entry related to debugging information</summary>
        DEBUG,
        /// <summary>log entry related to a critical error</summary>
        C_ERROR,
        /// <summary>log entry related to a marker ntry</summary>
        MARKER
    }
}
