
using System.Diagnostics;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace Enferno.Public.Logging
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class TraceTraceListener : CustomTraceListener
    {
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (data is LogEntry)
            {
                WriteInternal(data as LogEntry);
            }
            else
            {
                this.WriteLine(data.ToString());
            }
        }

        private void WriteInternal(LogEntry entry)
        {
            if (this.Formatter != null) this.WriteLine(this.Formatter.Format(entry));
            else
            {
                switch (entry.Severity)
                {
                    case TraceEventType.Critical:
                    case TraceEventType.Error:
                        Trace.TraceError(entry.ToString());
                        break;
                    case TraceEventType.Warning:
                        Trace.TraceWarning(entry.ToString());
                        break;
                    case TraceEventType.Information:
                        Trace.TraceInformation(entry.ToString());
                        break;
                    default:
                        Write(entry.ToString());
                        break;
                }
            }
        }

        public override void Write(string message)
        {
            Trace.Write(message);
        }

        public override void WriteLine(string message)
        {

            Trace.WriteLine(message);
        }
    }
}
