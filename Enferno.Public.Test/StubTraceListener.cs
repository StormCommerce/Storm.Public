using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;

namespace Enferno.Public.Test
{
    /// <summary>
    /// This is used in a unit test environment to be able to read log entries back from memory to Asserts with the GetMethods.
    /// </summary>
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class StubTraceListener : Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners.CustomTraceListener
    {
        private readonly static List<LogEntry> LogEntries = new List<LogEntry>();
        private readonly static List<string> LogMessages = new List<string>();

        public override void Write(string message)
        {
            LogMessages.Add(message);
        }

        public override void WriteLine(string message)
        {
            LogMessages.Add(message);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var le = data as LogEntry;
            if (le != null)
            {
                LogEntries.Add(le);
                if (Formatter != null)
                {
                    Write(Formatter.Format(le));
                    return;
                }
            }
            //base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Gets log entries in a text format
        /// </summary>
        /// <returns></returns>
        //public static IList<string> GetLogMessages()
        //{
        //    return new ReadOnlyCollection<string>(StubTraceListener.logMessages_);
        //}

        /// <summary>
        /// Get a filtered list of log entries from memory buffer
        /// </summary>
        /// <param name="severity">Filter by Severity - Critical, Error, Warning, Information or Verbose</param>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Readonly list of Entries</returns>
        public static IList<LogEntry> GetLogEntries(TraceEventType severity, string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            return new ReadOnlyCollection<LogEntry>(LogEntries.FindAll(le => le.Severity == severity
                && (category == null || le.Categories.Contains(category))
                && (messageStartsWith == null || le.Message.StartsWith(messageStartsWith))
                && (containsProperty == null || le.ContainsProperty(containsProperty))
                ));
        }
        /// <summary>
        /// Gets the first entry to match filter criterias from memory buffer
        /// </summary>
        /// <param name="severity">Filter by Severity - Critical, Error, Warning, Information or Verbose</param>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Log Entry or null</returns>
        public static LogEntry GetLogEntry(TraceEventType severity, string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            return LogEntries.FirstOrDefault(le => le.Severity == severity
                && (category == null || le.Categories.Contains(category))
                && (messageStartsWith == null || le.Message.StartsWith(messageStartsWith))
                && (containsProperty == null || le.ContainsProperty(containsProperty))
                );
        }
        /// <summary>
        /// Get log message from entry that match filter criterias.
        /// </summary>
        /// <param name="severity">Filter by Severity - Critical, Error, Warning, Information or Verbose</param>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Log Message or String.Empty</returns>
        public static string GetMessageFromLogEntry(TraceEventType severity, string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            var entry = GetLogEntry(severity, category, messageStartsWith, containsProperty);
            return entry != null ? entry.Message : string.Empty;
        }
        /// <summary>
        /// Get property object from entry that match filter criterias.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyKey">the property Key</param>
        /// <param name="severity">Filter by Severity - Critical, Error, Warning, Information or Verbose</param>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <returns>Property or Null</returns>
        public static T GetPropertyFromLogEntry<T>(string propertyKey, TraceEventType severity, string category = null, string messageStartsWith = null)
        {
            var entry = GetLogEntry(severity, category, messageStartsWith, propertyKey);
            return entry != null ? entry.GetProperty<T>(propertyKey) : default(T);
        }

        /// <summary>
        /// Get a filtered list of log entries from memory buffer
        /// </summary>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Readonly list of Entries</returns>
        public static IList<LogEntry> GetLogEntries(string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            return new ReadOnlyCollection<LogEntry>(LogEntries.FindAll(le => (category == null || le.Categories.Contains(category))
                && (messageStartsWith == null || le.Message.StartsWith(messageStartsWith))
                && (containsProperty == null || le.ContainsProperty(containsProperty))
                ));
        }
        /// <summary>
        /// Gets the first entry to match filter criterias from memory buffer
        /// </summary>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Log Entry or null</returns>
        public static LogEntry GetLogEntry(string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            return LogEntries.FirstOrDefault(le => (category == null || le.Categories.Contains(category))
                && (messageStartsWith == null || le.Message.StartsWith(messageStartsWith))
                && (containsProperty == null || le.ContainsProperty(containsProperty))
                );
        }
        /// <summary>
        /// Get log message from entry that match filter criterias.
        /// </summary>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <param name="containsProperty">Filter by property key</param>
        /// <returns>Log Message or String.Empty</returns>
        public static string GetMessageFromLogEntry(string category = null, string messageStartsWith = null, string containsProperty = null)
        {
            var entry = GetLogEntry(category, messageStartsWith, containsProperty);
            return entry != null ? entry.Message : string.Empty;
        }
        /// <summary>
        /// Get property object from entry that match filter criterias.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyKey">the property Key</param>
        /// <param name="category">Filter by a category</param>
        /// <param name="messageStartsWith">Filter by how logmessage starts</param>
        /// <returns>Property or Null</returns>
        public static T GetPropertyFromLogEntry<T>(string propertyKey, string category = null, string messageStartsWith = null)
        {
            var entry = GetLogEntry(category, messageStartsWith, propertyKey);
            return entry != null ? entry.GetProperty<T>(propertyKey) : default(T);
        }


        /// <summary>
        /// Clear memory buffers (use this before every new test)
        /// </summary>
        public static void Reset()
        {
            LogEntries.Clear();
            LogMessages.Clear();
        }
    }
}
