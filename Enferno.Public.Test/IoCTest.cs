
using System;
using Enferno.Public.InversionOfControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;

namespace Enferno.Public.Test
{
    [TestClass]
    public class IoCTest
    {
         [TestMethod, TestCategory("UnitTest")]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadContainerTest1()
        {
            // Arrange
            // Act
            IoC.LoadConfiguration("non existing");
            // Assert 
            Assert.Fail("Should not get here");
        }

         [TestMethod, TestCategory("UnitTest")]
        public void LoadContainerTest2()
        {
            // Arrange
            // Act
            var container = IoC.LoadConfiguration();
            // Assert 
            Assert.IsNotNull(container);
        }

         [TestMethod, TestCategory("UnitTest")]
        [Description("Tests manual create of IoC")]
        public void RegisterTypeTest1()
        {
            IoC.RegisterType<ITest, CTest>();
            IoC.RegisterType<ITest, DTest>();
            IoC.RegisterInstance(typeof(ITest), new ETest());

            var data = IoC.Resolve<ITest>();
            Assert.IsNotNull(data);
            Assert.IsInstanceOfType(data, typeof(ETest));
        }

         [TestMethod, TestCategory("UnitTest")]
        [Description("Tests manual create of IoC")]
        public void RegisterTypeTest2()
        {
            // Arrange
            var existing = IoC.RegisterType<ITest, FTest>();
            Assert.IsNotNull(existing);

            // Act
            Assert.IsFalse(IoC.IsRegistered<ITest2>());
            IoC.RegisterType<ITest2, ATest2>();

            // Assert 
            Assert.IsTrue(IoC.IsRegistered<ITest2>());
        }

         [TestMethod, TestCategory("UnitTest")]
        public void IsRegisteredFalseTest()
        {
            // Assert 
            Assert.IsFalse(IoC.IsRegistered<INoneExisting>());
        }
        
         [TestMethod, TestCategory("UnitTest")]
        public void RegisterTransientTypeTest1()
        {
            // Arrange
            IoC.RegisterType<ITest2, BTest2>();
            IoC.RegisterType<ITest2, ATest2>(new TransientLifetimeManager());
            // Act
            var data = IoC.Resolve<ITest2>();
            // Assert 
            Assert.IsInstanceOfType(data, typeof(ATest2));
        }

         [TestMethod, TestCategory("UnitTest")]
        public void RegisterTransientTypeTest2()
        {
            // Arrange
            IoC.RegisterType<ITest2, BTest2>();
            IoC.RegisterType<ITest2, ATest2>(new TransientLifetimeManager());

            // Act
            var data1 = IoC.Resolve<ITest2>();
            var data2 = IoC.Resolve<ITest2>();
            // Assert 
            Assert.AreNotSame(data1, data2);
        }

         [TestMethod, TestCategory("UnitTest")]
        public void RegisterSingletonTypeTest1()
        {
            // Arrange
            IoC.RegisterType<ITest2, BTest2>();
            IoC.RegisterType<ITest2, ATest2>(new ContainerControlledLifetimeManager());

            // Act
            var data1 = IoC.Resolve<ITest2>();
            var data2 = IoC.Resolve<ITest2>();
            // Assert 
            Assert.AreSame(data1, data2);
        }

         [TestMethod, TestCategory("UnitTest")]
        public void ResolveTransientTest()
        {
            // Arrange
            IoC.RegisterType<ITest2, ATest2>(new TransientLifetimeManager());
            // Act
            var data1 = IoC.Resolve<ITest2>();
            var data2 = IoC.Resolve<ITest2>();
            // Assert 
            Assert.AreNotSame(data1, data2);
        }

         [TestMethod, TestCategory("UnitTest")]
        public void ResolveSingletonTest()
        {
            // Arrange
            IoC.RegisterType<ITest2, ATest2>(new ContainerControlledLifetimeManager());
            // Act
            var data1 = IoC.Resolve<ITest2>();
            var data2 = IoC.Resolve<ITest2>();
            // Assert 
            Assert.AreSame(data1, data2);
        }

         [TestMethod, TestCategory("UnitTest")]
        public void ResolveDisposableTest()
        {
            // Arrange
            IoC.RegisterType<ITestDisposable, DisposableObject>(new TransientLifetimeManager());
            // Act
            using (var data1 = IoC.Resolve<ITestDisposable>())
            {
                using (var data2 = IoC.Resolve<ITestDisposable>())
                {
                    // Assert 
                    Assert.AreNotSame(data1, data2);
                }
            }

            var data3 = IoC.Resolve<ITestDisposable>();
            Assert.AreEqual("DisposableObject", data3.GetValule());
        }

         [TestMethod, TestCategory("UnitTest")]
        public void TransientCallbackLifetimeManagerTest()
        {
            IoC.RegisterType<ITest2, BTest2>(new TransientCallbackLifetimeManager<ITest2>(() => new BTest2()));

            // Act
            var obj1 = IoC.Resolve<ITest2>();
            var obj2 = IoC.Resolve<ITest2>();
            // Assert
            Assert.IsInstanceOfType(obj1, typeof(BTest2));
            Assert.AreNotSame(obj1, obj2);
        }
    }

    #region Test classes and interfaces
    public interface ITest
    {
        string GetValule();
    }

    public class CTest : ITest
    {
        public string GetValule()
        {
            return "ctest";
        }
    }

    public class DTest : ITest
    {
        public string GetValule()
        {
            return "dtest";
        }
    }

    public class ETest : ITest
    {
        public string GetValule()
        {
            return "etest";
        }
    }

    public class FTest : ITest
    {
        public string GetValule()
        {
            return "ftest";
        }
    }

    public interface ITest2
    {
        string GetValule();
    }

    public class ATest2 : ITest2
    {
        public string GetValule()
        {
            return "ctest2";
        }
    }
    public class BTest2 : ITest2
    {
        public string GetValule()
        {
            return "btest2";
        }
    }

    internal interface ITestDisposable : ITest2, IDisposable
    {
    }

    public class DisposableObject : ITestDisposable
    {
        public string GetValule()
        {
            return "DisposableObject";
        }

        public void Dispose()
        {
            
        }
    }

    public interface INoneExisting
    {
    }

    #endregion
}
