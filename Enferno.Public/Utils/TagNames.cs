namespace Enferno.Public.Utils
{
    //Add these to stage.labels in config.alloy in kube-application-state
    public static class TagNames
    {
        public const string ApplicationId = "norce.application.id";
        public const string ClientId = "norce.client.id";
        public const string BasketId = "norce.basket.id";
        public const string OrderId = "norce.order.id";
        public const string JobId = "norce.job.id";
        public const string JobKey = "norce.job.key";

        public static string LogNameTransformer(string key)
        {
            return key.Contains(".") ? key.Replace('.', '_') : key;
        }
    }
}