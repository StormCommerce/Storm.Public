using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Enferno.Public.Test
{
    public static class Extensions
    {
        public static T GetProperty<T>(this LogEntry entry, string name)
        {
            var prop = entry.ExtendedProperties[name];
            if (prop != null)
            {
                return (T)prop;
            }
            return default(T);
        }

        public static bool ContainsProperty(this LogEntry entry, string name)
        {
            var prop = entry.ExtendedProperties[name];
            return prop != null;
        }
    }
}
