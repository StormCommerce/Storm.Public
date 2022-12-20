﻿using Enferno.Public.Caching;
using Enferno.Public.Caching.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Public.Test
{
    public class ObjectWithAnId
    {
        public int Id { get; set; }
    }

    public class ObjectWithAnIdAndData
    {
        public int Id { get; set; }
        public string Data { get; set; }
    }

    [TestClass]
    public class CacheManagerTests
    {
        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigDefaultDurationTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            //Assert
            Assert.AreEqual(300, CacheConfiguration.Instance(cacheName).DefaultDuration, "Default duration");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigGetEntityWithDefaultDurationDurationTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            //Assert
            var betweenTestData = new BetweenTestData
            {
                Actual = CacheConfiguration.Instance(cacheName).GetCacheTime("GetEntityWithDefaultDuration"),

                MinValue = 270,
                MaxValue = 330
                
            };
            Assert.IsTrue(betweenTestData.IsValid, $"Item with default duration: {betweenTestData.GetErrorMessage()}");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigGetEntityWith1MinuteDurationDurationTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            var offsetted60SecondsDuration = CacheConfiguration.Instance(cacheName).GetCacheTime("GetEntityWith1MinuteDuration");

            var betweenTestData = new BetweenTestData
            {
                Actual = offsetted60SecondsDuration,

                MinValue = 54,
                MaxValue = 66
                
            };
            Assert.IsTrue(betweenTestData.IsValid, $"Item 1 min duration {betweenTestData.GetErrorMessage()}");

        }

        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigGetEntityWithZeroDurationDurationTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            //Assert
            Assert.AreEqual(0, CacheConfiguration.Instance(cacheName).GetCacheTime("GetEntityWithZeroDuration"), "Item 0 duration");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigNonExistingItemDurationTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            //Assert
            Assert.AreEqual(0, CacheConfiguration.Instance(cacheName).GetCacheTime("NonExistingItem"), "Non-existing 0 duration");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CacheConfigDurationWhenNoFileTest()
        {
            // Arrange
            const string cacheName = "CacheWithNoFile";

            var cache = new InMemoryTestCache(cacheName);
            var unused = new CacheManager(cache);

            //Assert
            Assert.AreEqual(null, CacheConfiguration.Instance(cacheName).DefaultDuration, "Default duration");
            Assert.AreEqual(0, CacheConfiguration.Instance(cacheName).GetCacheTime("WhatEver"));
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetBasketWithExecuteFunctionWithRedirectTest()
        {
            const string cacheName = "AccessClient";
            const int basketId = 1;
            var basket = new ObjectWithAnId { Id = basketId };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            var cacheKeyBasket = cacheManager.GetKey("GetBasket", basketId);

            //Act
            var cached = cacheManager.ExecuteFunction(cacheName, cacheKeyBasket, () => basket);

            // Assert
            Assert.AreSame(cached, basket);
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheKeyBasket, out cached));

            Assert.IsTrue(cache.TryGet("Basket1", out cached));
            Assert.AreSame(cached, basket);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetBasketWithExecuteFunctionWithRedirectAndClearTest()
        {
            const string cacheName = "AccessClient";
            const int basketId = 1;
            var basket = new ObjectWithAnId { Id = basketId };
            var checkout = new ObjectWithAnId { Id = basketId };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            var cacheKeyBasket = cacheManager.GetKey("GetBasket", basketId);
            var cacheKeyCheckout = cacheManager.GetKey("GetCheckout", basketId);

            //Act
            var cachedCheckout = cacheManager.ExecuteFunction(cacheName, cacheKeyCheckout, () => checkout);
            var cachedBasket = cacheManager.ExecuteFunction(cacheName, cacheKeyBasket, () => basket);

            // Assert
            Assert.AreSame(cachedBasket, basket);
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheKeyBasket, out cachedBasket));
            Assert.AreSame(cachedCheckout, checkout);
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheKeyCheckout, out cachedCheckout));

            // Act 2
            var modifiedBasket = new ObjectWithAnId { Id = basketId };
            var cacheKeyInsert = cacheManager.GetKey("InsertBasketItem", modifiedBasket.Id);
            cacheManager.Add(cacheName, cacheKeyInsert, modifiedBasket);

            // Assert 2
            Assert.IsFalse(cacheManager.TryGet(cacheName, cacheKeyCheckout, out cachedCheckout));
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheKeyBasket, out cachedBasket));
            Assert.AreSame(cachedBasket, modifiedBasket);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void FlushItemsByTagTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);


            // Act
            var key = cacheManager.GetKey("GetSomethingWithFlush", 1);
            var somethingIn = new ObjectWithAnId { Id = 1 };
            var somethingOut = cacheManager.ExecuteFunction(cacheName, key, "Tag" + 1, () => somethingIn);
            Assert.IsTrue(cacheManager.TryGet(cacheName, key, out ObjectWithAnId _));

            key = cacheManager.GetKey("GetWhateverWithFlush", 1);
            const int whateverIn = 1;
            var whateverOut = cacheManager.ExecuteFunction(cacheName, key, "Tag" + 1, () => whateverIn);
            Assert.IsTrue(cacheManager.TryGet(cacheName, key, out int _));

            key = cacheManager.GetKey("GetSomethingWithFlush", 2);
            var somethingIn2 = new ObjectWithAnId { Id = 2 };
            var somethingOut2 = cacheManager.ExecuteFunction(cacheName, key, "Tag" + 2, () => somethingIn2);
            Assert.IsTrue(cacheManager.TryGet(cacheName, key, out ObjectWithAnId _));

            // Assert 1
            Assert.AreSame(somethingIn, somethingOut);
            Assert.AreEqual(whateverIn, whateverOut);
            Assert.AreSame(somethingIn2, somethingOut2);

            // Act 2
            var updated = new ObjectWithAnId { Id = 1 };
            cacheManager.Flush(cacheName, "Tag" + updated.Id);
            var updateKey = cacheManager.GetKey("GetSomethingWithFlush", updated.Id);
            cacheManager.Add(cacheName, updateKey, "Tag" + 1, updated);

            // Assert 2
            key = cacheManager.GetKey("GetWhateverWithFlush", 1);
            Assert.IsFalse(cacheManager.TryGet(cacheName, key, out int _));
            key = cacheManager.GetKey("GetSomethingWithFlush", 1);
            Assert.IsTrue(cacheManager.TryGet(cacheName, key, out ObjectWithAnId _));
            key = cacheManager.GetKey("GetSomethingWithFlush", 2);
            Assert.IsTrue(cacheManager.TryGet(cacheName, key, out ObjectWithAnId _));
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetNullDataAndThenFlushItToCreateNew()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const string email = "patrik@attentia.se";
            var newObject = new ObjectWithAnId { Id = 1 };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            var cacheKey = cacheManager.GetKey("GetCustomerByEmail", email);

            //Act
            if (!cacheManager.HasConfiguration(cacheName)) Assert.Fail("Should have a cache config file");
            var added = cacheManager.Add(cacheName, cacheKey, (ObjectWithAnId)null);
            Assert.IsFalse(added, "Should not be added.");


            cacheKey = cacheManager.GetKey("CreateCustomer", newObject);
            cacheManager.TryGet(cacheName, cacheKey, out object cached);
            Assert.IsNull(cached, "Should not have value in cache");

            added = cacheManager.Add(cacheName, cacheKey, newObject);
            Assert.IsFalse(added, "Should not add creates.");

            cacheKey = cacheManager.GetKey("GetCustomerByEmail", email);
            cacheManager.Add(cacheName, cacheKey, newObject);
            cacheManager.TryGet(cacheName, cacheKey, out cached);
            Assert.AreSame(newObject, cached, "Now it should be cached.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetBasketThenInsertWithCacheRefresh()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const int basketId = 1;
            var basket = new ObjectWithAnId { Id = basketId };
            var checkout = new ObjectWithAnId { Id = basketId };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            var cacheKeyBasket = cacheManager.GetKey("GetBasket", basketId);
            var cacheKeyCheckout = cacheManager.GetKey("GetCheckout", basketId);

            //Act
            if (!cacheManager.HasConfiguration(cacheName)) Assert.Fail("Should have a cache config file");
            var success = cacheManager.Add(cacheName, cacheKeyBasket, basket);
            Assert.IsTrue(success, "basket should be added.");

            success = cacheManager.Add(cacheName, cacheKeyCheckout, checkout);
            Assert.IsTrue(success, "Checkout should be added.");


            var cacheKeyInsertBasket = cacheManager.GetKey("InsertBasketItem", basket);
            success = cacheManager.TryGet(cacheName, cacheKeyInsertBasket, out object cached);
            Assert.IsFalse(success, "Should not be there on that key.");

            var newBasket = new ObjectWithAnId { Id = basketId };
            success = cacheManager.Add(cacheName, cacheKeyInsertBasket, newBasket);
            Assert.IsTrue(success, "New basket should now be added on original key.");

            cacheManager.TryGet(cacheName, cacheKeyBasket, out cached);
            Assert.AreSame(newBasket, cached, "Now it should be cached.");

            cacheManager.TryGet(cacheName, cacheKeyCheckout, out cached);
            Assert.IsNull(cached, "Checkout should have been flushed");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetCustomerThenUpdateWithRedirectFormatTest()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const int customerId = 1;
            var existingCustomer = new ObjectWithAnIdAndData { Id = customerId, Data = "Data1" };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            //Act
            var cacheKeyCustomer = cacheManager.GetKey("GetCustomer", customerId);
            if (!cacheManager.HasConfiguration(cacheName)) Assert.Fail("Should have a cache config file");
            var success = cacheManager.Add(cacheName, cacheKeyCustomer, existingCustomer);
            Assert.IsTrue(success, "customer should be added.");


            var cacheKeyUpdateCustomer = cacheManager.GetKey("UpdateCustomer", customerId);
            success = cacheManager.TryGet(cacheName, cacheKeyUpdateCustomer, out object _);
            Assert.IsFalse(success, "Should not be there on that key.");

            var updatedCustomer = new ObjectWithAnIdAndData { Id = customerId, Data = "Data2" };
            success = cacheManager.Add(cacheName, cacheKeyUpdateCustomer, updatedCustomer);
            Assert.IsFalse(success, "Updated customer should now have flushed original key but not added this one.");
            success = cacheManager.TryGet(cacheName, cacheKeyCustomer, out object _);
            Assert.IsFalse(success, "customer should have been flushed.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetCustomerThenUpdateWithRefreshFormatTest()
        {
            // Arrange
            const string cacheName = "AccessClient";
            const int customerId = 1;
            var existingCustomer = new ObjectWithAnIdAndData { Id = customerId, Data = "Data1" };

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);

            //Act
            var cacheKeyCustomer = cacheManager.GetKey("GetCustomer", customerId);
            if (!cacheManager.HasConfiguration(cacheName)) Assert.Fail("Should have a cache config file");
            var success = cacheManager.Add(cacheName, cacheKeyCustomer, existingCustomer);
            Assert.IsTrue(success, "customer should be added.");


            var cacheKeyUpdateCustomer = cacheManager.GetKey("UpdateCustomer2", customerId);
            success = cacheManager.TryGet(cacheName, cacheKeyUpdateCustomer, out object cached);
            Assert.IsFalse(success, "Should not be there on that key.");

            var updatedCustomer = new ObjectWithAnIdAndData { Id = customerId, Data = "Data2" };
            success = cacheManager.Add(cacheName, cacheKeyUpdateCustomer, updatedCustomer);
            Assert.IsTrue(success, "Updated customer should now have replaced original key");
            cacheManager.TryGet(cacheName, cacheKeyCustomer, out cached);
            Assert.AreSame(updatedCustomer, cached, "Should be the same in the cache after update");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void FlushItemsByMultipleDependencyNamesTest()
        {
            // Arrange
            const string cacheName = "AccessClient";

            var cache = new InMemoryTestCache(cacheName);
            var cacheManager = new CacheManager(cache);


            // Act
            var dependency1 = new[] { "Customer1" };
            var dependency2 = new[] { "Customer1", "Company1", "PriceList1" };

            var dependency3 = new[] { "Customer2" };
            var dependency4 = new[] { "Customer2", "Company2", "PriceList2" };

            cacheManager.ExecuteFunction(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 1), dependency1, () => 1);
            cacheManager.ExecuteFunction(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 1), dependency2, () => 2);
            cacheManager.ExecuteFunction(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 2), dependency3, () => 3);
            cacheManager.ExecuteFunction(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 2), dependency4, () => 4);

            // Assert 1 All in cache
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 1), out int _));
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 1), out int _));
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 2), out int _));
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 2), out int _));

            // Act 2
            cacheManager.Flush(cacheName, "Customer1");
            cacheManager.Flush(cacheName, "PriceList2");

            // Assert 2 Just item 3 in cache
            Assert.IsFalse(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 1), out int _));
            Assert.IsFalse(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 1), out int _));
            Assert.IsTrue(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetSomethingWithFlush", 2), out int _));
            Assert.IsFalse(cacheManager.TryGet(cacheName, cacheManager.GetKey("GetWhateverWithFlush", 2), out int _));
        }
    }
    
    internal class BetweenTestData
    {
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int Actual { get; set; }

        public bool IsValid => MaxValue >= Actual && Actual >= MinValue;
        public string GetErrorMessage()
        {
            return $"Expected: {MaxValue} >= Actual >= {MinValue} Actual: <{Actual}>.";
        }
    }
    
}