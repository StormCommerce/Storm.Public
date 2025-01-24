using Enferno.Public.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using System.Linq;

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
            if (!(Activity.Current?.Baggage?.Any() ?? false)) return;
            foreach (var baggage in Activity.Current.Baggage)
            {
                if (LogTagUtils.TryGetValue(baggage.Key, out var func))
                {
                    logEntry?.AddKeyIfMissing(Transform(baggage.Key), func(baggage.Value));
                }
            }
        }

        private static void AddKeyIfMissing(this LogEntry logEntry, string key, object value)
        {
            if (value != null && !(logEntry?.ExtendedProperties.ContainsKey(key) ?? false))
            {
                logEntry?.ExtendedProperties.Add(key, value);
            }
        }

        private static string Transform(string key)
        {
            return key.Contains(".") ? key.Replace('.', '_') : key;
        }
    }
}