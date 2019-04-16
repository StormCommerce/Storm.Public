
using System;
using System.Reflection;

namespace Enferno.Public.Caching
{
    public interface ICacheManager
    {
        string GetKey(string tag, params object[] keys);
        string GetArgumentKey(string cacheName, MethodInfo method, params object[] args);

        T ExecuteFunction<T>(string cacheName, string key, Func<T> getDataFunc);
        T ExecuteFunction<T>(string cacheName, string key, string dependencyName, Func<T> getDataFunc);
        T ExecuteFunction<T>(string cacheName, string key, string[] dependencyNames, Func<T> getDataFunc);

        bool TryGet<T>(string cacheName, string key, out T cached);
        [Obsolete("Use TryGet without dependencyName.")]
        bool TryGet<T>(string cacheName, string key, string dependencyName, out T cached);
        
        bool Add<T>(string cacheName, string key, T cached);
        bool Add<T>(string cacheName, string key, string dependencyName,  T cached);
        bool Add<T>(string cacheName, string key, string[] dependencyNames,  T cached);

        void Flush(string cacheName, string dependencyName);

        void Remove(string cacheName, string key);

        bool HasConfiguration(string cacheName);

        IDisposable AcquireLock(string cacheName, string key);

        void ClearRedirected<T>(string cacheName, string key, T cached);
    }
}
