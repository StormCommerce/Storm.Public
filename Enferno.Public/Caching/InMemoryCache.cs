using System;
using System.Linq;
using System.Runtime.Caching;

namespace Enferno.Public.Caching
{
    public class InMemoryCache : BaseCache
    {
        protected MemoryCache MyCache;

        public InMemoryCache(string name, int duration) : base(name)
        {
            DurationMinutes = duration;
            MyCache = InMemoryCacheFactory.GetCache(Name);
        }

        public InMemoryCache(string name) : base(name)
        {
            MyCache = InMemoryCacheFactory.GetCache(Name);
        }

        public override void FlushTag(string dependencyName)
        {
            CacheChangedMonitor.Flush(dependencyName);
        }

        protected override object GetItem(string key)
        {
            return MyCache.Get(key);
        }

        protected override void AddItem(string key, object cached, TimeSpan duration)
        {
            AddItem(key, cached, duration, (string)null);
        }

        protected override void AddItem(string key, object cached, TimeSpan duration, string dependencyName)
        {
            AddItem(key, cached, duration, string.IsNullOrWhiteSpace(dependencyName) ? null : new[] { dependencyName });
        }

        protected override void AddItem(string key, object cached, TimeSpan duration, string[] dependencyNames)
        {
            var cip = CreateCacheItemPolicy(duration, dependencyNames);
            MyCache.Set(key, cached, cip);
        }

        private static CacheItemPolicy CreateCacheItemPolicy(TimeSpan duration, string[] dependencyNames)
        {
            var cip = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(duration)
            };

            if (dependencyNames == null || dependencyNames.Length == 0) return cip;

            foreach (var dependencyName in dependencyNames.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                cip.ChangeMonitors.Add(new CacheChangedMonitor(dependencyName));
            }
            return cip;
        }

        public override void Remove(string key)
        {
            MyCache.Remove(key);
        }
    }
}
