
using System;
using Microsoft.Practices.Unity.Configuration;
using Unity;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;

namespace Enferno.Public.InversionOfControl
{
    public static class IoC
    {
        private static readonly Lazy<IUnityContainer> LazyContainer = new Lazy<IUnityContainer>(() => new UnityContainer().LoadConfiguration());

        public static IUnityContainer Container => LazyContainer.Value;

        public static IUnityContainer LoadConfiguration()
        {
            return Container.LoadConfiguration();            
        }

        public static IUnityContainer LoadConfiguration(string containerName)
        {
            return Container.LoadConfiguration(containerName);
        }

        public static IUnityContainer RegisterType<T, TS>(params InjectionMember[] injectionMembers) where TS : T
        {
            return Container.RegisterType<T, TS>(injectionMembers);
        }

        public static IUnityContainer RegisterType<T, TS>(string name, params InjectionMember[] injectionMembers) where TS : T
        {
            return Container.RegisterType<T, TS>(name, injectionMembers);
        }

        public static IUnityContainer RegisterType<T, TS>(LifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TS : T
        {
            return Container.RegisterType<T, TS>(lifetimeManager, injectionMembers);
        }

        public static IUnityContainer RegisterType<T, TS>(string name, LifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TS : T
        {
            return Container.RegisterType<T, TS>(name, lifetimeManager, injectionMembers);
        }

        public static IUnityContainer RegisterType(Type from, Type to, LifetimeManager lifetimeManager)
        {
            return Container.RegisterType(from, to, lifetimeManager);
        }

        public static IUnityContainer RegisterInstance(Type type, object instance)
        {
            return Container.RegisterInstance(type, instance);
        }

        public static T Resolve<T>(params ResolverOverride[] resolverOverrides)
        {
            return Container.Resolve<T>(resolverOverrides);
        }

        public static T Resolve<T>(string name, params ResolverOverride[] resolverOverrides)
        {
            return Container.Resolve<T>(name, resolverOverrides);
        }

        public static bool IsRegistered<T>()
        {
            return Container.IsRegistered<T>();
        }

        public static bool IsRegistered<T>(string name)
        {
            return Container.IsRegistered<T>(name);
        }
    }
}
