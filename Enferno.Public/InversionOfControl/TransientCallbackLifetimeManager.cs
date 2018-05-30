using System;
using Unity.Lifetime;

namespace Enferno.Public.InversionOfControl
{
    /// <summary>
    /// The TransientCallbackLifetimeManager takes a callback which should create the instance you want.
    /// </summary>
    /// <typeparam name="T">This is the type the lifetime manager should return</typeparam>
    public class TransientCallbackLifetimeManager<T> : LifetimeManager
    {
        private readonly Func<T> createCallback;

        public TransientCallbackLifetimeManager(Func<T> createCallback)
        {
            this.createCallback = createCallback;
        }

        public override object GetValue(ILifetimeContainer container = null)
        {
            return createCallback();
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return this;
        }
    }
}
