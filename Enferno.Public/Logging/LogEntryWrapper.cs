using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enferno.Public.Extensions;
using Enferno.Public.Utils;
using static Unity.Storage.RegistrationSet;

namespace Enferno.Public.Logging
{
    /// <summary>
    /// Wrapper around Logging application block LogEntry.
    /// </summary>
    public class LogEntryWrapper
    {
        private static readonly Dictionary<string, object> LogTracker = new Dictionary<string, object>();

        private readonly LogEntry entry;

        public LogEntryWrapper()
        {
            entry = new LogEntry();
        }

        /// <summary>
        /// Adds categories.
        /// </summary>
        /// <param name="categories"></param>
        /// <returns></returns>
        public LogEntryWrapper Categories(CategoryFlags categories)
        {
            AddCategories(GetCategoryList(categories));
            return this;
        }

        public LogEntryWrapper Categories(params string[] categories)
        {
            AddCategories(categories);
            return this;
        }

        private void AddCategories(IEnumerable<string> categories)
        {
            if (categories == null) return;
            foreach (var category in categories)
            {
                entry.Categories.Add(category);
            }
        }

        private string message;
        private object[] messageArgs;

        /// <summary>
        /// Resolved log message
        /// </summary>
        /// <remarks>For debugging purposes, set message with ChangeMessage()</remarks>
        private string FormattedMessage => message == null ? "" :
            (messageArgs != null && messageArgs.Length > 0) ? string.Format(message, messageArgs) :
            message;

        private readonly List<Exception> myExceptions = new List<Exception>();

        /// <summary>
        /// List of exceptions that will be added to log entry in the ErrorMessages output property (together with ErrorMessages)
        /// </summary>
        public LogEntryWrapper Exceptions(params Exception[] exceptions)
        {
            myExceptions.AddRange(exceptions);
            return this;
        }

        private readonly List<string> myErrorMessages = new List<string>();

        /// <summary>
        /// List of error messages that will be added to log entry in the ErrorMessages output property (together with Exceptions)
        /// </summary>
        public LogEntryWrapper ErrorMessages(params string[] errorMessages)
        {
            myErrorMessages.AddRange(errorMessages);
            return this;
        }

        /// <summary>
        /// Title of log entry
        /// </summary>
        public LogEntryWrapper Title(string title)
        {
            entry.Title = title;
            return this;
        }

        public LogEntryWrapper Client(int clientId)
        {
            if (ShouldBeLogged)
            {
                entry.ExtendedProperties[TagKeyEnum.ClientId] = clientId;
            }
            return this;
        }

        public LogEntryWrapper Application(int applicationId)
        {
            if (ShouldBeLogged)
            {
                entry.ExtendedProperties[TagKeyEnum.ApplicationId] = applicationId;
            }
            return this;
        }

        public LogEntryWrapper Severity(TraceEventType severity)
        {
            entry.Severity = severity;
            return this;
        }

        /// <summary>
        /// Sets log message to entry
        /// </summary>
        /// <param name="msg">Log message, supports String.Format() with arguments from separate parameters</param>
        /// <param name="args">arguments</param>
        public LogEntryWrapper Message(string msg, params object[] args)
        {
            message = msg;
            messageArgs = args;

            return this;
        }

        /// <summary>
        /// Adds an extendedproperty to log entry
        /// </summary>
        /// <param name="key">Key name</param>
        /// <param name="obj">Object that should be logged</param>
        /// <remarks>Primarily for Debug purposes, make sure that configuration supports your extended properties</remarks>
        public LogEntryWrapper Property(string key, object obj)
        {
            if (ShouldBeLogged)
            {
                entry.ExtendedProperties[key] = obj?.ToString() ?? "null";
            }
            return this;
        }

        public object Property(string key)
        {
            return entry.ExtendedProperties[key];
        }

        public LogEntryWrapper Properties(params KeyValuePair<string, object>[] entries)
        {
            if (ShouldBeLogged)
            {
                foreach (var keyValuePair in entries)
                {
                    entry.ExtendedProperties[keyValuePair.Key] = keyValuePair.Value;
                }
            }
            return this;
        }

        public void WriteCritical()
        {
            Write(TraceEventType.Critical);
        }

        public void WriteError()
        {
            Write(TraceEventType.Error);
        }

        public void WriteOnce(TraceEventType severity)
        {
            Severity(severity);
            WriteOnce();
        }

        /// <summary>
        /// When writing through this method, severity must be set or the default severity will be used.
        /// </summary>
        public void WriteOnce()
        {
            var key = GetLogKey();
            if (IsLogged(key)) return;
            lock (LogTracker)
            {
                if (IsLogged(key)) return;
                Write();
                SetLogged(key);
            }
        }

        public void WriteInformation()
        {
            Write(TraceEventType.Information);
        }

        public void WriteResume()
        {
            Write(TraceEventType.Resume);
        }

        public void WriteStart()
        {
            Write(TraceEventType.Start);
        }

        public void WriteStop()
        {
            Write(TraceEventType.Stop);
        }

        public void WriteSuspend()
        {
            Write(TraceEventType.Suspend);
        }

        public void WriteTransfer()
        {
            Write(TraceEventType.Transfer);
        }

        public void WriteWarning()
        {
            Write(TraceEventType.Warning);
        }

        public void WriteVerbose()
        {
            Write(TraceEventType.Verbose);
        }

        /// <summary>
        /// ShouldBeLogged checks if logging is enabled, if the categories are enabled for the current severity.
        /// So the check if this instance should be logged severity and categories must be set.
        /// </summary>
        public bool ShouldBeLogged => Log.LoggingEnabled &&
                                      !ContextualLoggingDisabler.IsLoggingDisabledForCurrentLogicalCallContext() &&
                                      Log.ShouldLog(entry);

        /// <summary>
        /// Writes the entry to the log
        /// </summary>
        private void Write(TraceEventType severity)
        {
            entry.Severity = severity;
            Write();
        }

        /// <summary>
        /// When writing through this method, severity must be set or the default severity will be used.
        /// </summary>
        public void Write()
        {
            if (!ShouldBeLogged) return;

            entry.Message = FormattedMessage;

            foreach (var item in myExceptions)
            {
                entry.AddErrorMessage(item.ToString());
            }
            foreach (var item in myErrorMessages)
            {
                entry.AddErrorMessage(item);
            }

            entry.AddTraceIdAndSpanId();
            entry.AddClientId();
            entry.AddApplicationId();

            Log.Write(entry);
        }

        private static bool IsLogged(string key)
        {
            return LogTracker.ContainsKey(key);
        }

        private static void SetLogged(string key)
        {
            LogTracker.Add(key, null);
        }

        private string GetLogKey()
        {
            var parts = entry.ExtendedProperties.Select(property => property.Value.ToString()).ToList();
            parts.Add(FormattedMessage);
            return parts.Any() ? string.Join(".", parts) : "";
        }

        //Needed to be able to debug/test
        public string[] CategoryStrings => entry.CategoriesStrings;

        private static IEnumerable<string> GetCategoryList(CategoryFlags categories)
        {
            //loopa igenom värden på enum
            return (from CategoryFlags categoryFlag in Enum.GetValues(typeof(CategoryFlags)) where categories.HasFlag(categoryFlag) select categoryFlag.ToString()).ToArray();
        }
    }
}