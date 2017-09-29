
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace Enferno.Public.Caching
{
    internal static class InMemoryCacheFactory
    {
        private static readonly ConcurrentDictionary<string, MemoryCache> Caches = new ConcurrentDictionary<string, MemoryCache>();

        static InMemoryCacheFactory()
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            // See http://csharpindepth.com/Articles/General/Singleton.aspx version 4.
        }

        public static MemoryCache GetCache(string name)
        {
            return Caches.GetOrAdd(name, (n => new MemoryCache(name)));
        }
    }
}
