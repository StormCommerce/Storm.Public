
using System;

namespace Enferno.Public.Caching
{
    [Serializable]
    public class NullObject
    {
        public override bool Equals(object obj)
        {
            return (obj is NullObject);
        }

        public override int GetHashCode()
        {
// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }
    }

    public abstract class BaseCache : ICache
    {
        public string Name { get; protected set; }
        public int? DurationSeconds { get; set; }

        protected static readonly NullObject CacheNullObject = new NullObject();

        protected BaseCache(string name)
        {
            Name = name;
        }

        public virtual bool TryGet<T>(string key, out T cached)
        {
            cached = default(T);

            var o = GetItem(key);

            if (o == null) return false;
            if (!CacheNullObject.Equals(o)) cached = (T)o;

            return true;
        }

        protected virtual object GetItem(string key)
        {
            return null;
        }

        public virtual void Add<T>(string key, T cached, int? durationMinutes = null)
        {
            Add(key, cached, (string)null, durationMinutes);
        }

        public virtual void Add<T>(string key, T cached, string dependencyName = null, int? durationMinutes = null)
        {
            Add(key, cached, string.IsNullOrWhiteSpace(dependencyName) ? null : new[] { dependencyName }, durationMinutes);
        }

        public void Add<T>(string key, T cached, string[] dependencyNames, int? durationMinutes = null)
        {
            var duration = GetDurationTimeSpan(durationMinutes);

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (cached == null) AddItem(key, CacheNullObject, duration, dependencyNames);
            else AddItem(key, cached, duration, dependencyNames);
        }

        protected virtual void AddItem(string key, object cached, TimeSpan duration)
        {
            /*NOOP*/
        }

        protected virtual void AddItem(string key, object cached, TimeSpan duration, string dependencyName)
        {
            /*NOOP*/
        }

        protected virtual void AddItem(string key, object cached, TimeSpan duration, string[] dependencyNames)
        {
            /*NOOP*/
        }

        public virtual void FlushTag(string name)
        {
            /* NOOP */
        }

        public virtual void Remove(string key)
        {
            RemoveItem(key);
        }

        protected virtual void RemoveItem(string key)
        {
            /*NOOP*/
        }

        private TimeSpan GetDurationTimeSpan(int? durationSeconds)
        {
            var duration = durationSeconds.HasValue && durationSeconds.Value != 0
                ? TimeSpan.FromSeconds(durationSeconds.Value)
                : DurationSeconds.HasValue
                    ? TimeSpan.FromSeconds(DurationSeconds.Value)
                    : TimeSpan.FromSeconds(5*60);
            return duration;
        }
    }
}
