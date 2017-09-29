using System;
using Microsoft.Practices.Unity;

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

        public override object GetValue()
        {
            return createCallback();
        }

        public override void RemoveValue()
        {
            // NOOP
        }

        public override void SetValue(object newValue)
        {
            // NOOP 
        }
    }
}
