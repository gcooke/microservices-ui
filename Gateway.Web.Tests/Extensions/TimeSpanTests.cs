using System;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Extensions
{
    [TestFixture]
    public class TimeSpanTests
    {
        private readonly Func<string, string, string> _failedMessage = (x, y) => string.Format("Expected {0} and received {1}", x, y);

        public void Minutes_returned_in_expected_friendly_format()
        {
            const string expectedString = "2m";
            var timeSpan = new TimeSpan(0, 0, 2, 0, 0);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Milliseconds_returned_in_expected_friendly_format()
        {
            const string expectedString = "600ms";
            var timeSpan = new TimeSpan(0, 0, 0, 0, 600);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Seconds_returned_in_expected_friendly_format()
        {
            const string expectedString = "6s";
            var timeSpan = new TimeSpan(0, 0, 0, 6, 0);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Milliseconds_converted_to_seconds_friendly_format()
        {
            const string expectedString = "6s";
            var timeSpan = new TimeSpan(0, 0, 0, 0, 6000);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Minutes_including_ms_returned_in_minutes_and_seconds()
        {
            const string expectedString = "2m 4s";
            var timeSpan = new TimeSpan(0, 0, 2, 4, 201);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Seconds_converted_to_minute_friendly_format()
        {
            const string expectedString = "1m 26s";
            var timeSpan = new TimeSpan(0, 0, 0, 86, 580);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }

        [Test]
        public void Seconds_and_milliseconds_converted_to_minute_friendly_format()
        {
            const string expectedString = "1m 27s";
            var timeSpan = new TimeSpan(0, 0, 0, 86, 1028);
            var friendlyString = timeSpan.Humanize();
            Assert.AreEqual(expectedString, friendlyString, _failedMessage(expectedString, friendlyString));
        }
    }
}