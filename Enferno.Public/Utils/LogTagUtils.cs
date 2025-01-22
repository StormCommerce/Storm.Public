using System;
using System.Collections.Generic;
using System.Linq;

namespace Enferno.Public.Utils
{
    public class LogTagUtils
    {
        public static object lockObj = new object();
        private static Dictionary<string, Func<string, object>> KeysToLog { get; } = new Dictionary<string, Func<string, object>>()
        {
            {TagNames.ApplicationId,_parseInt},
            {TagNames.ClientId,_parseInt},
            {TagNames.BasketId,_parseInt},
            {TagNames.OrderId,_parseString},
            {TagNames.JobId,_parseInt},
            {TagNames.JobKey,_parseGuid},
        };

        public static List<KeyValuePair<string, Func<string, object>>> GetKeysToLog => KeysToLog.ToList();


        public static void AddKeyToLog(string key)
        {
            AddKeyToLog(key, _parseString);
        }

        public static void AddKeyToLog(string key, Func<string, object> func)
        {
            if (!KeysToLog.ContainsKey(key))
            {
                lock (lockObj)
                {
                    if (!KeysToLog.ContainsKey(key))
                    {
                        KeysToLog.Add(key, func);
                    }
                }
                
            }
        }

        private static Func<string, object> _parseString
        {
            get
            {
                return value => value;
            }
        }

        private static Func<string, object> _parseInt
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

        private static Func<string, object> _parseGuid
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
}