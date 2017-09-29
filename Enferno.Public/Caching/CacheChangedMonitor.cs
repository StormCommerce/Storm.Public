
using System;
using System.Runtime.Caching;

namespace Enferno.Public.Caching
{
    public class CacheChangedEventArgs : EventArgs
    {
        public string Name { get; }

        public CacheChangedEventArgs(string name = null)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Cache change monitor that allows an app to fire a change notification to all associated cache items.
    /// </summary>
    public class CacheChangedMonitor : ChangeMonitor
    {
        private static event EventHandler<CacheChangedEventArgs> Flushed;

        private readonly string dependencyName;

        public override string UniqueId { get; } = Guid.NewGuid().ToString();

        public CacheChangedMonitor(string dependencyName = null)
        {
            this.dependencyName = dependencyName;
            Flushed += OnFlushRaised;
            InitializationComplete();
        }

        /// <summary>
        /// s
        /// </summary>
        /// <param name="dependencyName"></param>
        public static void Flush(string dependencyName = null)
        {
            Flushed?.Invoke(null, new CacheChangedEventArgs(dependencyName));
        }

        protected override void Dispose(bool disposing)
        {
            Flushed -= OnFlushRaised;
        }

        private void OnFlushRaised(object sender, CacheChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Name) || string.Compare(e.Name, dependencyName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                OnChanged(null);
            }
        }
    }
}
