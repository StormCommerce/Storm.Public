using System.Xml.Serialization;

namespace Enferno.Public.Caching.Configuration
{
    /// <summary>
    /// This class represents one cache directive in the cache.xml file.
    /// </summary>
    public class CacheDefinition
    {
        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Cache duration in minutes.
        /// </summary>
        [XmlAttribute]
        public int? Duration { get; set; }
        /// <summary>
        /// Defines an alternate cache key format based on either PropertyPath or ArgumentName.
        /// </summary>
        [XmlAttribute]
        public string RedirectFormat { get; set; }
        /// <summary>
        /// Defines which property on the cache object that should be used as value in the RedirectFormat.
        /// </summary>
        [XmlAttribute]
        public string PropertyPath { get; set; }
        /// <summary>
        /// Defines which argument in the called method that should be used as value in the RedirectFormat.
        /// </summary>
        [XmlAttribute]
        public string ArgumentName { get; set; }
        /// <summary>
        /// Defines an optional RefreshFormat that can be used to re-cache objects on inserts/updates or other changes.
        /// </summary>
        [XmlAttribute]
        public string RefreshFormat { get; set; }
    }
}
