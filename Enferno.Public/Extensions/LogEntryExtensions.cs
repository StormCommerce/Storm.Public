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
            logEntry.AddKeyIfMissing( TraceIdKey, Activity.Current?.TraceId);
            logEntry.AddKeyIfMissing( SpanIdKey, Activity.Current?.SpanId);
        }

        internal static void AddClientId(this LogEntry logEntry)
        {
            logEntry?.AddKeyIfMissing(ActivityExtensions.ClientIdKey, Activity.Current.GetClientId());
        }

        internal static void AddApplicationId(this LogEntry logEntry)
        {
            logEntry?.AddKeyIfMissing(ActivityExtensions.ApplicationIdKey, Activity.Current.GetApplicationId());
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