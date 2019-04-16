using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Unity;

using Enferno.Public.Caching.Configuration;
using Enferno.Public.InversionOfControl;

namespace Enferno.Public.Caching
{
    public class CacheManager : ICacheManager
    {
        private static readonly ICacheKeyLock CacheKeyLock = new CacheKeyLock();
        private static readonly ICacheKeyLock CacheKeyNoLock = new CacheKeyNoLock();

        #region Singleton stuff
        // See http://csharpindepth.com/Articles/General/Singleton.aspx version 6.

        private static readonly Lazy<ICacheManager> LazyInstance = new Lazy<ICacheManager>(() => IoC.Container.Resolve<ICacheManager>());

        public static ICacheManager Instance => LazyInstance.Value;

        #endregion

        private static readonly Regex ItemRule = new Regex(@"^(?<item>[^:]+)", RegexOptions.IgnoreCase);  // sample: GetApplication:

        private Dictionary<string, CacheConfiguration> Configurations { get; }
        private Dictionary<string, ICache> Caches { get; }

        public CacheManager(params ICache[] caches)
        {
            Configurations = new Dictionary<string, CacheConfiguration>();
            Caches = new Dictionary<string, ICache>();

            foreach (var cache in caches)
            {
                if (Caches.ContainsKey(cache.Name)) continue;
                Caches.Add(cache.Name, cache);
                Configurations.Add(cache.Name, CacheConfiguration.Instance(cache.Name));
                if (!cache.DurationMinutes.HasValue) cache.DurationMinutes = CacheConfiguration.Instance(cache.Name).DefaultDuration;
            }
        }

        private bool ShouldCache(string cacheName, string itemName)
        {
            if (!Caches.TryGetValue(cacheName, out _)) return false;
            var cfg = Configurations[cacheName];
            if (!cfg.HasFile) return true;
            var time = cfg.GetCacheTime(itemName);
            return time != 0;
        }

        public string GetKey(string itemName, params object[] keys)
        {
            var buff = new StringBuilder();

            buff.AppendFormat("{0}:", itemName);
            foreach (var key in keys)
            {
                if (key == null) buff.Append("NULL");
                else buff.Append(key);
                buff.Append(":");
            }
            return buff.ToString();
        }

        public string GetArgumentKey(string cacheName, MethodInfo method, params object[] args)
        {
            if (method?.DeclaringType == null || args == null || args.Length == 0) return null;

            var cfg = Configurations[cacheName];
            var redirectFormat = cfg.GetRedirectFormat(method.Name);
            var argumentName = cfg.GetArgumentName(method.Name);

            if (string.IsNullOrWhiteSpace(redirectFormat) || string.IsNullOrWhiteSpace(argumentName)) return null;

            return string.Format("{1}:" + redirectFormat, GetParameterValue(method, argumentName, args), method.Name);
        }

        private static string GetParameterValue(MethodInfo method, string argumentName, params object[] args)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var parameter = i < parameters.Length ? parameters[i] : null;
                if (parameter == null) return null;
                if (parameter.Name.Equals(argumentName, StringComparison.CurrentCultureIgnoreCase)) return arg?.ToString();
            }
            return null;
        }

        public T ExecuteFunction<T>(string cacheName, string key, string dependencyName, Func<T> getDataFunc)
        {
            return ExecuteFunction(cacheName, key, string.IsNullOrWhiteSpace(dependencyName) ? null : new [] {dependencyName}, getDataFunc);
        }

        public T ExecuteFunction<T>(string cacheName, string key, string[] dependencyNames, Func<T> getDataFunc)
        {
            T data;
            if (TryGet(cacheName, key, out data)) return data;

            data = getDataFunc();

            Add(cacheName, key, dependencyNames, data);
            return data;
        }

        public T ExecuteFunction<T>(string cacheName, string key, Func<T> getDataFunc)
        {
            T data;
            if (TryGet(cacheName, key, out data)) return data;

            data = getDataFunc();

            Add(cacheName, key, data);
            return data;
        }

        [Obsolete("Use TryGet without dependencyName.")]
        public bool TryGet<T>(string cacheName, string key, string dependencyName, out T cached)
        {
            cached = default(T);
            var itemName = GetItemName(key);
            return ShouldCache(cacheName, itemName) && Caches[cacheName].TryGet(key, out cached);
        }

        public bool TryGet<T>(string cacheName, string key, out T cached)
        {
            cached = default(T);
            var itemName = GetItemName(key);
            if (!ShouldCache(cacheName, itemName)) return false;

            var cfg = Configurations[cacheName];
            var redirectFormat = cfg.GetRedirectFormat(itemName);

            if (string.IsNullOrWhiteSpace(redirectFormat)) return Caches[cacheName].TryGet(key, out cached);

            object entityKey;
            if (!Caches[cacheName].TryGet(key, out entityKey)) return false;

            if (entityKey == null)
            {
                return true;
            }
            if (!string.IsNullOrWhiteSpace(cfg.GetArgumentName(itemName)))
            {
                var argumentKey = entityKey.ToString();
                return Caches[cacheName].TryGet(argumentKey, out cached);
            }

            var newKey = string.Format(redirectFormat, entityKey);
            return Caches[cacheName].TryGet(newKey, out cached);
        }

        public bool Add<T>(string cacheName, string key, string dependencyName, T cached)
        {
            return Add(cacheName, key, string.IsNullOrWhiteSpace(dependencyName) ? null : new [] { dependencyName }, cached);
        }

        public bool Add<T>(string cacheName, string key, string[] dependencyNames, T cached)
        {
            var itemName = GetItemName(key);
            if (!ShouldCache(cacheName, itemName))
            {
                return false;
            }

            var cfg = Configurations[cacheName];

            var duration = cfg.GetCacheTime(itemName);
            Caches[cacheName].Add(key, cached, dependencyNames, duration);
            return true;
        }

        public bool Add<T>(string cacheName, string key, T cached)
        {
            var itemName = GetItemName(key);

            var cfg = Configurations[cacheName];
            var redirectFormat = cfg.GetRedirectFormat(itemName);

            string newKey = null;
            object entityKey = null;            
            if (!string.IsNullOrWhiteSpace(redirectFormat))
            {
// ReSharper disable once CompareNonConstrainedGenericWithNull
                if (cached == null) return false; // Don't cache nulls with redirects

                var propertyPath = cfg.GetPropertyPath(itemName);
                if (!string.IsNullOrWhiteSpace(propertyPath))
                {
                    if (TryGetEntityKey(cached, propertyPath, out entityKey))
                    {
                        newKey = string.Format(redirectFormat, entityKey);
                    }
                }
                else
                {
                    newKey = RemoveItemName(key);
                    entityKey = newKey;
                }
            }

            var duration = cfg.GetCacheTime(itemName);
            if (!ShouldCache(cacheName, itemName))
            {
                if (string.IsNullOrWhiteSpace(newKey)) return false;
                RemoveRedirected(cacheName, newKey);

                var refreshFormat = cfg.GetRefreshFormat(itemName);
                return TryRefresh(cacheName, refreshFormat, entityKey, cached, duration);
            }
            if (!string.IsNullOrWhiteSpace(newKey))
            {
                Caches[cacheName].Add(key, entityKey, duration);
                Caches[cacheName].Add(newKey, cached, duration);
            }
            else Caches[cacheName].Add(key, cached, duration);
            return true;
        }

        private bool TryRefresh(string cacheName, string refreshFormat, object entityKey, object cached, int duration)
        {
            if (string.IsNullOrWhiteSpace(refreshFormat)) return false;
            var newKey = string.Format(refreshFormat, entityKey);
            Caches[cacheName].Add(newKey, cached, duration);
            return true;
        }

        public void ClearRedirected<T>(string cacheName, string key, T cached)
        {
            var itemName = GetItemName(key);

            var cfg = Configurations[cacheName];
            var redirectFormat = cfg.GetRedirectFormat(itemName);
            
            if (string.IsNullOrWhiteSpace(redirectFormat)) return;

            string newKey = null;
            var propertyPath = cfg.GetPropertyPath(itemName);
            if (!string.IsNullOrWhiteSpace(propertyPath))
            {
                object entityKey;
                if (TryGetEntityKey(cached, propertyPath, out entityKey))
                {
                    newKey = string.Format(redirectFormat, entityKey);
                }
            }
            else newKey = RemoveItemName(key); // argument key.

            if (string.IsNullOrWhiteSpace(newKey) || ShouldCache(cacheName, itemName)) return;
            RemoveRedirected(cacheName, newKey);
        }

        private void RemoveRedirected(string cacheName, string newKey)
        {
            var keys = newKey.Split(';'); // redirectFormat can have multiple values to clear, ';'-separeted.
            foreach (var k in keys)
            {
                Caches[cacheName].Remove(k);
            }
        }

        public IDisposable AcquireLock(string cacheName, string key)
        {
            return !ShouldCache(cacheName, key) ? 
                CacheKeyNoLock.AcquireLock(key) : 
                CacheKeyLock.AcquireLock(cacheName + key);
        }

        public void Flush(string cacheName, string dependencyName)
        {
            Caches[cacheName].FlushTag(dependencyName);
        }

        public void Remove(string cacheName, string key)
        {
            Caches[cacheName].Remove(key);
        }

        public bool HasConfiguration(string cacheName)
        {
            var cfg = Configurations[cacheName];
            return cfg.HasFile;
        }

        private static bool TryGetEntityKey<T>(T entity, string propertyPath, out object key)
        {
            object o = entity;
            key = default(object);
            if (o == null || string.IsNullOrWhiteSpace(propertyPath)) return false;
            foreach (var propertyName in propertyPath.Split('.'))
            {
                var type = o.GetType();
                var pi = type.GetProperty(propertyName);
                if (pi == null) { throw new MissingMemberException(typeof(T).ToString(), propertyName); }
                o = pi.GetValue(o, null);
                if (o == null) return false;
            }
            key = o;
            return true;
        }

        private static string GetItemName(string key)
        {
            var match = ItemRule.Match(key);
            return !match.Success ? null : match.Groups["item"].Value;
        }

        private static string RemoveItemName(string key)
        {
            var start = GetItemName(key);
            var length = start?.Length ?? 0;
            return length == 0 ? key : key.Substring(length + 1);
        }
    }
}
