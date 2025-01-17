using Enferno.Public.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Public.Test.Utils
{
    [TestClass]
    public class LogTagUtilsTests
    {
        [TestMethod, TestCategory("UnitTest")]
        public void AddKeyToLog_GivenTestKey_AssertKeyIsAddedToKeysToLog()
        {
            // Arrange
            string key = "test";

            // Act
            LogTagUtils.AddKeyToLog(key);

            // Assert
            Assert.IsTrue(LogTagUtils.KeysToLog.ContainsKey(key));
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddKeyToLog_GivenTestKeyAndParseFunction_AssertKeyIsAddedToKeysToLog()
        {
            // Arrange
            string key = "test";

            // Act
            LogTagUtils.AddKeyToLog(key,(string a)=>a);

            // Assert
            Assert.IsTrue(LogTagUtils.KeysToLog.ContainsKey(key));
        }
    }
}