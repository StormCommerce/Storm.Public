
namespace Enferno.Public.Caching
{
    public interface ICache
    {
        string Name { get; }
        int? DurationMinutes { get; set; }
        bool TryGet<T>(string key, out T cached);
        void Add<T>(string key, T cached, int? durationminutes = null);
        void Add<T>(string key, T cached, string dependencyName = null, int? durationMinutes = null);
        void Add<T>(string key, T cached, string[] dependencyNames, int? durationMinutes = null);
        void FlushTag(string dependencyName);
        void Remove(string key);
    }
}
