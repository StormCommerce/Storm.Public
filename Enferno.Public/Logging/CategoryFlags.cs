using System;

namespace Enferno.Public.Logging
{
    /// <summary>
    /// Common Logging categories
    /// </summary>
    [Flags]
    public enum CategoryFlags
    {
        /// <summary>
        /// Used for logging that should be alerted in realtime
        /// </summary>
        Alert = 1,
        /// <summary>
        /// Important log items for tracking processes
        /// </summary>
        TrackingEvent = 2,
        /// <summary>
        /// Logging for error checking
        /// </summary>
        Debug = 4,
        /// <summary>
        /// Logging monitores by profiling tools
        /// </summary>
        Profiling = 8,
           /// <summary>
        /// Logging adds ClientId extended property and Category for ClientNotification enabled Tracelisteners
        /// </summary>
        ClientNotification = 16
    }
}
