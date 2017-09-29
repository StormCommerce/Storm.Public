using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Enferno.Public.Logging
{
    /// <summary>
    /// Static class that wrapps the log handling implemented with the Logging application block
    /// </summary>
    /// <remarks>Use the static Methods for simple logging - Error, Warn, Verbose etc. Use Params().[Method()] to be able to add several categories.</remarks>
    public static class Log
    {
        private static LogWriter LogWriter { get; } = new LogWriterFactory().Create();

        public static LogEntryWrapper LogEntry => new LogEntryWrapper();
        public static bool LoggingEnabled => LogWriter.IsLoggingEnabled();

        internal static void Write(LogEntry entry)
        {
            LogWriter.Write(entry);
        }

        public static bool ShouldLog(LogEntry entry)
        {
            return LogWriter.ShouldLog(entry);
        }
    }
}
