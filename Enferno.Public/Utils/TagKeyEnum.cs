using System;
using System.Collections.Generic;

namespace Enferno.Public.Utils
{
    public static class TagKeyEnum
    {
        public const string ApplicationId = "ApplicationId";
        public const string ClientId = "ClientId";
        public const string BasketId = "BasketId";
        public const string JobId = "JobId";
        public const string JobKey = "JobKey";

        internal static Dictionary<string, LoggProperty> KeysToLog { get; } = new Dictionary<string, LoggProperty>()
        {
            {TagKeyEnum.ApplicationId,new LoggProperty(TagKeyEnum.ApplicationId,parseInt)},
            {TagKeyEnum.ClientId,new LoggProperty(TagKeyEnum.ClientId,parseInt)},
            {TagKeyEnum.BasketId,new LoggProperty(TagKeyEnum.BasketId,parseInt)},
            {TagKeyEnum.JobId,new LoggProperty(TagKeyEnum.JobId,parseInt)},
            {TagKeyEnum.JobKey,new LoggProperty(TagKeyEnum.JobKey,parseGuid)},
        };

        public static void AddKeyToLog(string key)
        {
            AddKeyToLog(key, parseString);
        }

        public static void AddKeyToLog(string key, Func<string, object> func)
        {
            if (!KeysToLog.ContainsKey(key))
            {
                KeysToLog.Add(key, new LoggProperty(key, func));
            }
        }

        private static Func<string, object> parseString
        {
            get
            {
                return value =>
                {
                    if (int.TryParse(value, out int result))
                        return result;
                    return null;
                };
            }
        }

        private static Func<string, object> parseInt
        {
            get
            {
                return value =>
                {
                    if (int.TryParse(value, out int result))
                        return result;
                    return null;
                };
            }
        }

        private static Func<string, object> parseGuid
        {
            get
            {
                return value =>
                {
                    if (Guid.TryParse(value, out Guid result))
                        return result;
                    return null;
                };
            }
        }
    }

    public class LoggProperty
    {
        public string Key { get; private set; }
        private Func<string, object> Func { get; set; }

        public LoggProperty(string key, Func<string, object> func)
        {
            this.Key = key;
            this.Func = func;
        }

        public object GetValue(string value)
        {
            return Func(value);
        }
    }
}