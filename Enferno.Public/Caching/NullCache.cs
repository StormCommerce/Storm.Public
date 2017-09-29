namespace Enferno.Public.Caching
{
    public class NullCache : BaseCache
    {
        public NullCache(string name) : base(name)
        {
        }

        public override bool TryGet<T>(string key, out T cached)
        {
            cached = default(T);
            return false;
        }
    }
}
