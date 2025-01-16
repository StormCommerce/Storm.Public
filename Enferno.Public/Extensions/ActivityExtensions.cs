using System;
using Enferno.Public.Utils;
using System.Diagnostics;

namespace Enferno.Public.Extensions
{
    public static class ActivityExtensions
    {
        public static void SetPropertyOnTrace(this Activity activity, string key, object value)
        {
            if (activity == null) return;
            if (activity.HasRemoteParent || activity.Parent == null)
            {
                activity.SetPropertyOnSpan(key,value);
            }
            else
            {
                activity.Parent.SetPropertyOnTrace(key, value);
            }
        }

        public static void SetPropertyOnSpan(this Activity activity, string key, object value)
        {
            activity?.SetBaggage(key, value?.ToString());
            activity?.SetTag(key, value);
        }

        internal static string GetProperty(this Activity activity, string key)
        {
            return activity?.GetBaggageItem(key);
        } 

        public static void SetClientOnTrace(this Activity activity, int? clientId)
        {
            activity.SetPropertyOnTrace(TagKeyEnum.ClientId, clientId);
        }

        //internal static int? GetClientId(this Activity activity)
        //{
        //    if (Int32.TryParse(activity?.GetBaggageItem(TagKeyEnum.ClientId), out int result))
        //        return result;
        //    return null;
        //}

        public static void SetApplicationOnTrace(this Activity activity, int? applicationId)
        {
            activity.SetPropertyOnTrace(TagKeyEnum.ApplicationId, applicationId);
        }

        //internal static int? GetApplicationId(this Activity activity)
        //{
        //    if (Int32.TryParse(activity?.GetBaggageItem(TagKeyEnum.ApplicationId), out int result))
        //        return result;
        //    return null;
        //}
    }
}