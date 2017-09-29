using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Enferno.Public.Logging
{
    public class ContextualLoggingDisabler : IDisposable
    {

        private const string CallContextName = "ContextualLoggingDisablerSetting";

        public ContextualLoggingDisabler()
        {
            CallContext.SetData(CallContextName, true);
        }

        public void Dispose()
        {
            CallContext.SetData(CallContextName, false);
        }

        public static bool IsLoggingDisabledForCurrentLogicalCallContext()
        {
            var loggingDisableSetting = CallContext.GetData(CallContextName);
            return loggingDisableSetting != null && (bool) loggingDisableSetting;
        }
    }
}
