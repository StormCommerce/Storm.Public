using Enferno.Public.Utils;
using System.Diagnostics;

namespace Enferno.Public.Extensions
{
    public static class ActivityExtensions
    {
        public static void SetPropertyOnSpan(this Activity activity, string key, object value)
        {
            activity?.SetBaggage(key, value?.ToString());
            activity?.SetTag(key, value);
        }

        internal static string GetProperty(this Activity activity, string key)
        {
            return activity?.GetBaggageItem(key);
        }

        public static void SetClientId(this Activity activity, int? clientId)
        {
            activity?.SetPropertyOnSpan(TagNames.ClientId, clientId);
        }

        public static void SetApplicationId(this Activity activity, int? applicationId)
        {
            activity?.SetPropertyOnSpan(TagNames.ApplicationId, applicationId);
        }
    }
}