using System;
using System.Linq;
using Enferno.Public.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Public.Test
{
    [TestClass]
    public class LoggingTest
    {
        [TestMethod]
        public void AddSamePropertyToLogTest()
        {
            // Arrange
            var log = Log.LogEntry;

            // Act
            log.Property("Test", "Old");
            log.Property("Test", "New");

            // Assert
            Assert.AreEqual("New", log.Property("Test"));
        }

        [TestMethod]
        public void AddCategoryTest()
        {
            CheckAddCategory(CategoryFlags.Alert, "Alert");
        }

        [TestMethod]
        public void AddCategoryTest2()
        {
            CheckAddCategory(CategoryFlags.Debug, "Debug");
        }

        [TestMethod]
        public void AddCategoryTest3()
        {
            CheckAddCategory(CategoryFlags.Debug | CategoryFlags.Profiling | CategoryFlags.TrackingEvent, "Debug", "Profiling", "TrackingEvent");
        }

        [TestMethod]
        public void AddCategoryTest4()
        {
            CheckAddCategory(CategoryFlags.ClientNotification | CategoryFlags.Profiling | CategoryFlags.TrackingEvent, "ClientNotification", "Profiling", "TrackingEvent");
        }

        [TestMethod]
        public void AddCategoryTest5()
        {
            CheckAddCategoryNotExpected(CategoryFlags.ClientNotification | CategoryFlags.Profiling | CategoryFlags.TrackingEvent, "Alert", "Debug");
        }

        [TestMethod]
        public void BadlyFormattedMessagesTest()
        {
            // Arrange
            var log = Log.LogEntry;

            // Act
            log.Categories(CategoryFlags.Debug).Message("Error in Class.Method: Jalla{0}").Write();
            log.Categories(CategoryFlags.Debug).Message("Error in Class.Method: Jalla{}").Write();

            // Assert
            // Ignore assert. Above should not fail.
        }

        [TestMethod]
        public void AddExceptionWithMultipleMessagesTest()
        {
            // Arrange
            var ex1 = new ApplicationException("Message1");
            var ex2 = new ApplicationException("Message2");

            var log = Log.LogEntry;

            // Act
            log.Categories(CategoryFlags.Debug);
            log.Message(ex1.Message);
            log.Exceptions(ex1);
            log.Exceptions(ex2);


            // Assert
            log.WriteError();
        }

        private static void CheckAddCategory(CategoryFlags categories, params string[] expectedCategories)
        {
            // Arrange
            var log = Log.LogEntry;

            // Act
            log.Categories(categories);

            foreach (var expectedCategory in expectedCategories)
            {
                // Assert
                Assert.IsTrue(log.CategoryStrings.Contains(expectedCategory), $"Category {expectedCategory} expected");
            }
            
        }
        private static void CheckAddCategoryNotExpected(CategoryFlags categories, params string[] notExpectedCategories)
        {
            // Arrange
            var log = Log.LogEntry;

            // Act
            log.Categories(categories);

            foreach (var expectedCategory in notExpectedCategories)
            {
                // Assert
                Assert.IsTrue(!log.CategoryStrings.Contains(expectedCategory), $"Category {expectedCategory} exists, but not expected");
            }

        }
    }
}
