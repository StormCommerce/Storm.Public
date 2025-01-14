using System.Diagnostics;

namespace Enferno.Public.Extensions
{
    public static class ActivityExtensions
    {
        internal const string ApplicationIdKey = "ApplicationId";
        internal const string ClientIdKey = "ClientId";

        public static void SetClient(this Activity activity, int? clientId)
        {
            activity.SetBaggage(ClientIdKey, clientId?.ToString());
        }

        internal static string  GetClientId(this Activity activity)
        {
            return activity.GetBaggageItem(ClientIdKey);
        }

        public static void SetApplication(this Activity activity, int? applicationId)
        {
            activity.SetBaggage(ApplicationIdKey, applicationId?.ToString());
        }

        internal static string GetApplicationId(this Activity activity)
        {
            return activity.GetBaggageItem(ApplicationIdKey);
        }
    }
}