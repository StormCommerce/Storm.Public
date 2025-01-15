using System.Diagnostics;
using Enferno.Public.Utils;

namespace Enferno.Public.Extensions
{
    public static class ActivityExtensions
    {

        public static void SetPropertyOnTrace(this Activity activity, string key, string value)
        {
            if (activity == null) return;
            if (activity.HasRemoteParent || activity.Parent == null)
            {
                activity?.SetBaggage(key, value);
            }
            else
            {
                activity.Parent.SetPropertyOnTrace(key, value);
            }
        }

        public static void SetPropertyOnSpanAndSubSpan(this Activity activity, string key, string value)
        {
            activity?.SetBaggage(key, value);
        }

        public static void SetClientOnTrace(this Activity activity, int? clientId)
        {
            activity.SetPropertyOnTrace(TagKeyEnum.ClientId, clientId?.ToString());
        }

        internal static string GetClientId(this Activity activity)
        {
            return activity?.GetBaggageItem(TagKeyEnum.ClientId);
        }

        public static void SetApplicationOnTrace(this Activity activity, int? applicationId)
        {
            activity.SetPropertyOnTrace(TagKeyEnum.ApplicationId, applicationId?.ToString());
        }

        internal static string GetApplicationId(this Activity activity)
        {
            return activity?.GetBaggageItem(TagKeyEnum.ApplicationId);
        }
    }
}