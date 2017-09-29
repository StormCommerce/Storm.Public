
using System.Runtime.Caching;
using Enferno.Public.Caching;

namespace Enferno.Public.Test
{
    internal class InMemoryTestCache : InMemoryCache
    {
         public InMemoryTestCache(string name, int duration) : base(name, duration)
        {
            MyCache = new MemoryCache(name);
        }

        public InMemoryTestCache(string name) : base(name)
        {
            MyCache = new MemoryCache(name);
        }
    }
}
