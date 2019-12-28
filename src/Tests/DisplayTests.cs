using System;
using GitT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DisplayTests
    {
        [TestMethod]
        public void FormatDate()
        {
            Assert.AreEqual("just now", FormatDate(TimeSpan.FromSeconds(1)));
            Assert.AreEqual("just now", FormatDate(TimeSpan.FromSeconds(59)));
            Assert.AreEqual("just a minute ago", FormatDate(TimeSpan.FromSeconds(60)));
            Assert.AreEqual("just a minute ago", FormatDate(TimeSpan.FromSeconds(61)));
            Assert.AreEqual("just a minute ago", FormatDate(TimeSpan.FromSeconds(2 * 60 - 1)));
            Assert.AreEqual("2 minutes ago", FormatDate(TimeSpan.FromSeconds(2 * 60 + 1)));
            Assert.AreEqual("59 minutes ago", FormatDate(TimeSpan.FromSeconds(59 * 60 + 29)));
            Assert.AreEqual("60 minutes ago", FormatDate(TimeSpan.FromSeconds(59 * 60 + 30)));
            Assert.AreEqual("60 minutes ago", FormatDate(TimeSpan.FromSeconds(59 * 60 + 59)));
            Assert.AreEqual("an hour ago", FormatDate(TimeSpan.FromSeconds(60 * 60)));
            Assert.AreEqual("2 hours ago", FormatDate(TimeSpan.FromSeconds(2 * 60 * 60)));
            Assert.AreEqual("4 hours ago", FormatDate(TimeSpan.FromSeconds(4 * 60 * 60)));
            Assert.AreEqual("today at 05:00", FormatDate(TimeSpan.FromSeconds(5 * 60 * 60)));
            Assert.AreEqual("today at 04:00", FormatDate(TimeSpan.FromSeconds(6 * 60 * 60)));
            Assert.AreEqual("today at 00:00", FormatDate(TimeSpan.FromSeconds(10 * 60 * 60)));
            Assert.AreEqual("yesterday at 23:59", FormatDate(TimeSpan.FromSeconds(10 * 60 * 60 + 1)));
            Assert.AreEqual("yesterday at 00:00", FormatDate(TimeSpan.FromSeconds(34 * 60 * 60)));
            Assert.AreEqual("2018-07-15 23:59", FormatDate(TimeSpan.FromSeconds(34 * 60 * 60 + 1)));
        }
        private static string FormatDate(TimeSpan diff)
        {
            var now = new DateTime(2018, 07, 17, 10, 00, 00);
            return DateTools.FormatDate(now - diff, now);
        }
    }
}
