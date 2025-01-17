using Enferno.Public.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;

namespace Enferno.Public.Extensions
{
    public static class LogEntryExtensions
    {
        private const string SpanIdKey = "span_id";
        private const string TraceIdKey = "trace_id";

        internal static void AddTraceIdAndSpanId(this LogEntry logEntry)
        {
            logEntry?.AddKeyIfMissing(TraceIdKey, Activity.Current?.TraceId);
            logEntry?.AddKeyIfMissing(SpanIdKey, Activity.Current?.SpanId);
        }

        internal static void AddActivityKeysToLog(this LogEntry logEntry)
        {
            foreach (var key in LogTagUtils.KeysToLog)
            {
                logEntry?.AddKeyIfMissing(key.Key, key.Value(Activity.Current?.GetProperty(key.Key)));
            }
        }

        private static void AddKeyIfMissing(this LogEntry logEntry, string key, object value)
        {
            if (value != null && !(logEntry?.ExtendedProperties.ContainsKey(key) ?? false))
            {
                logEntry?.ExtendedProperties.Add(key, value);
            }
        }
    }
}