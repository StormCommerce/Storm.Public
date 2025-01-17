using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Enferno.Public.Extensions;
using Enferno.Public.Utils;
using System;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Enferno.Public.Test.Extensions
{
    [TestClass]
    public class ActivityExtensionsTests
    {

        [TestMethod, TestCategory("UnitTest")]
        public void SetClientId_GivenInt_AssertBaggageContainsValue()
        {
            // Arrange
            var testActivity = new Activity("Test");
            int? clientId = 5;

            // Act
            testActivity.SetClientId(clientId);

            // Assert
            Assert.AreEqual(testActivity.GetBaggageItem(TagNames.ClientId), clientId?.ToString());
            
        }

        [TestMethod, TestCategory("UnitTest")]
        public void SetClientId_GivenInt_AssertTagContainsValue()
        {
            // Arrange
            var testActivity = new Activity("Test");
            int? clientId = 5;

            // Act
            testActivity.SetClientId(clientId);

            // Assert
            Assert.AreEqual(testActivity.GetTagItem(TagNames.ClientId), clientId);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void SetApplicationId_GivenInt_AssertBaggageContainsValue()
        {
            // Arrange
            var testActivity = new Activity("Test");
            int? applicationId = 5;

            // Act
            testActivity.SetApplicationId(applicationId);

            // Assert
            Assert.AreEqual(testActivity.GetBaggageItem(TagNames.ApplicationId), applicationId?.ToString());

        }

        [TestMethod, TestCategory("UnitTest")]
        public void SetApplicationId_GivenInt_AssertGetTagContainsValue()
        {
            // Arrange
            var testActivity = new Activity("Test");
            int? applicationId = 5;
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
           string a = string.Format("{0}, PublicKey={1}",
                assemblyName.Name,
                string.Join("", assemblyName.GetPublicKey().Select(m => string.Format("{0:x2}", m))));
            // Act
            testActivity.SetApplicationId(applicationId);

            // Assert
            Assert.AreEqual(testActivity.GetTagItem(TagNames.ApplicationId), applicationId);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetProperty_GivenClientKey_WhenClientHasBeenSet_ReturnsClientId()
        {
            // Arrange
            var testActivity = new Activity("Test");
            int? clientId = 5;
            testActivity.SetClientId(clientId);

            // Act
            var result = testActivity.GetProperty(TagNames.ClientId);

            // Assert
            Assert.AreEqual(result, clientId?.ToString());
        }
    }
}