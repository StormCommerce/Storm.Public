using Enferno.Public.Extensions;
using Enferno.Public.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Enferno.Public.Test.Extensions
{
    [TestClass]
    public class LogEntryExtensionsTests
    {
        private const string SpanIdKey = "span_id";
        private const string TraceIdKey = "trace_id";

        public static ActivitySource ActivitySource = new ActivitySource("Test");

        [TestInitialize]
        public void Init()
        {
            var activityListener = new ActivityListener
            {
                ShouldListenTo = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(activityListener);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddTraceIdAndSpanId_WhenCurrentActivityIsNull_AssertTraceOrSpanIsNotSet()
        {
            // Arrange
            var logEntry = new LogEntry();

            // Act
            logEntry.AddTraceIdAndSpanId();

            // Assert
            Assert.IsFalse(logEntry.ExtendedProperties.ContainsKey(SpanIdKey));
            Assert.IsFalse(logEntry.ExtendedProperties.ContainsKey(TraceIdKey));
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddTraceIdAndSpanId_WhenCurrentActivityIsSet_AssertTraceOrSpanIsSet()
        {
            // Arrange
            var logEntry = new LogEntry();
            var activity = ActivitySource.StartActivity("test");

            // Act
            logEntry.AddTraceIdAndSpanId();

            // Assert
            Assert.IsTrue(logEntry.ExtendedProperties.ContainsKey(SpanIdKey));
            Assert.IsTrue(logEntry.ExtendedProperties.ContainsKey(TraceIdKey));
            Assert.AreEqual(logEntry.ExtendedProperties[TraceIdKey], activity.TraceId);
            Assert.AreEqual(logEntry.ExtendedProperties[SpanIdKey], activity.SpanId);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddActivityKeysToLog_WhenNoKeysAreSet()
        {
            // Arrange
            var logEntry = new LogEntry();

            // Act
            logEntry.AddActivityKeysToLog();

            // Assert
            Assert.IsFalse(logEntry.ExtendedProperties.Any());
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddActivityKeysToLog_WhenAllKeysAreSet_AssertAllKeysExistInLog()
        {
            // Arrange
            var logEntry = new LogEntry();
            var activity = ActivitySource.StartActivity("test");
            var testGuid = Guid.NewGuid();
            var testInt = 123;
            var testString = "abc";
            activity.SetPropertyOnSpan(TagNames.JobKey, testGuid);
            activity.SetPropertyOnSpan(TagNames.ApplicationId, testInt);
            activity.SetPropertyOnSpan(TagNames.ClientId, testInt);
            activity.SetPropertyOnSpan(TagNames.BasketId, testInt);
            activity.SetPropertyOnSpan(TagNames.OrderId, testString);
            activity.SetPropertyOnSpan(TagNames.JobId, testInt);

            // Act
            logEntry.AddActivityKeysToLog();

            // Assert
            Assert.IsTrue(logEntry.ExtendedProperties.Any());
            Assert.AreEqual(logEntry.ExtendedProperties.Count, LogTagUtils.KeysToLog.Count);

            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.JobKey], testGuid);
            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.ApplicationId], testInt);
            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.ClientId], testInt);
            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.BasketId], testInt);
            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.OrderId], testString);
            Assert.AreEqual(logEntry.ExtendedProperties[TagNames.JobId], testInt);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddActivityKeysToLog_WhenKeyNotExistingInKeysToLogExists_AssertKeyIsNotLogged()
        {
            // Arrange
            var logEntry = new LogEntry();
            var activity = ActivitySource.StartActivity("test");

            var testString = "123";
            activity.SetPropertyOnSpan("RandomKey", testString);

            // Act
            logEntry.AddActivityKeysToLog();

            // Assert
            Assert.IsFalse(logEntry.ExtendedProperties.Any());
        }

        [TestMethod, TestCategory("UnitTest")]
        public void AddActivityKeysToLog_WhenValueIsAddedWithWrongType_AssertKeyIsNotAddedToLog()
        {
            // Arrange
            var logEntry = new LogEntry();
            var activity = ActivitySource.StartActivity("test");
            var testString = "abc";
            activity.SetPropertyOnSpan(TagNames.ApplicationId, testString);

            // Act
            logEntry.AddActivityKeysToLog();

            // Assert
            Assert.IsFalse(logEntry.ExtendedProperties.Any());
        }
    }
}