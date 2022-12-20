using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Enferno.Public.Logging;

namespace Enferno.Public.Caching.Configuration
{
    public class CacheConfiguration
    {
        #region Singleton stuff
        private static readonly Dictionary<string, CacheConfiguration> instances = new Dictionary<string, CacheConfiguration>();
        private static readonly object syncRoot = new object();

        public static CacheConfiguration Instance(string cacheName)
        {
            if (instances.ContainsKey(cacheName)) return instances[cacheName];
            lock (syncRoot)
            {
                if (!instances.ContainsKey(cacheName)) instances[cacheName] = new CacheConfiguration(cacheName);
            }
            return instances[cacheName];
        }
        #endregion

        public bool HasFile { get; }

        /// <summary>
        /// Default duration in minutes.
        /// </summary>
        public int? DefaultDuration { get; private set; }

        private readonly string configFile;
        private readonly string path;
        private static Random random = new();
        
        private string FilePath => Path.Combine(path, configFile);

        private FileSystemWatcher watcher;

        private Dictionary<string, CacheDefinition> configuration;

        protected CacheConfiguration(string cacheName)
        {
            this.configFile = string.Format("{0}.Cache.xml", cacheName); 
            this.path = GetExecutionPath(configFile);

            HasFile = File.Exists(FilePath);

            SetupWatcher();
            LoadCacheTimes();
        }

        private void SetupWatcher()
        {
            if (!HasFile) return;

            watcher = new FileSystemWatcher(path, this.configFile) {NotifyFilter = NotifyFilters.LastWrite};
            watcher.Changed += ConfigChanged;

            watcher.EnableRaisingEvents = true;
        }

        private static string GetExecutionPath(string filename)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)!.Remove(0, 6);
            if (File.Exists(Path.Combine(path, filename))) return path;
            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            return Path.Combine(path, "App_Data");
        }

        private void ConfigChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                LoadCacheTimes();
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        private void LoadCacheTimes()
        {
            var newConfig = new Dictionary<string, CacheDefinition>();
            int? defaultDuration = null;

            try
            {
                if (!HasFile) return;

                Log.LogEntry.Categories(CategoryFlags.Debug).Message("Loading cache configuration {0}.", FilePath).WriteVerbose();

                var xml = XDocument.Load(FilePath);
                var cfg = xml.Element("CacheConfiguration");

                if (cfg == null) return;

                defaultDuration = int.Parse((string)cfg.Attribute("duration")) ;

                var items = cfg.Descendants("Item");
                foreach (var item in items)
                {
                    newConfig.Add(item.Attribute("name").Value, CreateDefinition(defaultDuration.Value, item));
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Failed to load cache configuration: {0}", ex.Message));
            }
            finally
            {
                lock (syncRoot)
                {
                    DefaultDuration = defaultDuration;
                    configuration = newConfig;
                }
            }
        }

        private CacheDefinition CreateDefinition(int defaultDuration, XElement item)
        {
            int duration;
            if (item.Attribute("duration") == null || !int.TryParse(item.Attribute("duration").Value, out duration)) duration = defaultDuration;
            duration = OffsetDuration(duration);
            
            var format = item.Attribute("redirectformat") != null ? item.Attribute("redirectformat").Value : null;
            var propertyPath = item.Attribute("propertypath") != null ? item.Attribute("propertypath").Value : null;
            var argumentName = item.Attribute("argumentname") != null ? item.Attribute("argumentname").Value : null;
            var refreshFormat = item.Attribute("refreshformat") != null ? item.Attribute("refreshformat").Value : null;

            return new CacheDefinition
            {
                Name = item.Attribute("name").Value,
                Duration = duration,
                RedirectFormat = format,
                PropertyPath = propertyPath,
                ArgumentName = argumentName,
                RefreshFormat = refreshFormat,
            };
        }

        private static int OffsetDuration(int duration)
        {
            int min = duration * 90 / 100;
            int max = duration * 110 / 100;

            return random.Next(min, max);
        }

        public int GetCacheTime(string itemName)
        {
            CacheDefinition definition;
            if (configuration.TryGetValue(itemName, out definition))
            {
                return definition.Duration.GetValueOrDefault(DefaultDuration.GetValueOrDefault(0));
            }

            return 0;
        }

        public string GetRedirectFormat(string itemName)
        {
            CacheDefinition definition;
            return configuration.TryGetValue(itemName, out definition) ? definition.RedirectFormat : null;
        }

        public string GetPropertyPath(string itemName)
        {
            CacheDefinition definition;
            return configuration.TryGetValue(itemName, out definition) ? definition.PropertyPath : null;
        }

        public string GetArgumentName(string itemName)
        {
            CacheDefinition definition;
            return configuration.TryGetValue(itemName, out definition) ? definition.ArgumentName : null;
        }

        public string GetRefreshFormat(string itemName)
        {
            CacheDefinition definition;
            return configuration.TryGetValue(itemName, out definition) ? definition.RefreshFormat : null;
        }
    }
}
