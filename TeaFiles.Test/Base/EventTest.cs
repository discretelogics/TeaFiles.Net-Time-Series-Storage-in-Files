// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class EventTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void EventToString()
        {
            Event<long> e = new Event<long>();
            e.Time = new Time(2000, 1, 2);
            e.Value = 172;

            string datetString = new DateTime(2000, 1, 2).ToString();
            e.ToString().Should().Be(datetString + "\t172");
            Console.WriteLine(e);
        }

        [TestMethod]
        public void EventCtorTest()
        {
            var t = new DateTime(2000, 1, 2);
            Event<long> e = new Event<long>(t, 72);
            e.Value.Should().Be(72);
            e.Time.Should().Be(t);
        }
    }
}
