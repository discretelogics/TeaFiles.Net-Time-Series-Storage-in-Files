// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class TimeScaleTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void ExoticCoverage()
        {
            Timescale.Java.GetHashCode().Should().Not.Be.EqualTo(Timescale.Net.GetHashCode()); // otherwise our Hashcode generation would be questionable
            Executing.This(() => Timescale.FromEpoch(1, TimeSpan.TicksPerDay * 2)).Should().Throw<ArgumentException>();
            Executing.This(() => Timescale.FromEpoch(1, 1, 1, TimeSpan.TicksPerDay * 2)).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TickScaleTest1to1()
        {
            var scale = Timescale.FromEpoch(1, 1, 1, TimeSpan.TicksPerDay);
            scale.Epoch.Should().Be(0);
            scale.TicksPerDay.Should().Be(TimeSpan.TicksPerDay);
            scale.NetToScale(1234567).Should().Be(1234567);
            scale.ScaleToNet(1234567).Should().Be(1234567);
        }

        [TestMethod]
        public void TickScaleTest1to7()
        {
            var scale = Timescale.FromEpoch(1, 1, 1, TimeSpan.TicksPerDay / 7);
            scale.Epoch.Should().Be(0);
            scale.TicksPerDay.Should().Be(TimeSpan.TicksPerDay / 7);
            scale.NetToScale(1234567).Should().Be(1234567 / 7);
            scale.ScaleToNet(1234567).Should().Be(1234567 * 7);
        }

        [TestMethod]
        public void Equality()
        {
            (Timescale.Java == Timescale.Net).Should().Be.False();
            (Timescale.Java != Timescale.Net).Should().Be.True();
            Timescale.Java.Equals(711).Should().Be.False();
        }

        [TestMethod]
        public void WellknownNames()
        {
            Timescale.Net.WellKnownName.Should().Be("Net");
            Timescale.Java.WellKnownName.Should().Be("Java");
            Timescale.FromEpoch(222, 33).WellKnownName.Should().Be("Custom");
        }

        [TestMethod]
        public void ToStringTest()
        {
            Timescale.Net.ToString().Should().Be("Net");
            Timescale.Java.ToString().Should().Be("Java");
            Timescale.FromEpoch(1000, 1).ToString().Should().Be("1000,1");
        }

        [TestMethod]
        public void UsingJavaTime()
        {
            Time.Scale = Timescale.Java;

            Time t = new Time(2012, 1, 2); // use Time the same way as you use DateTime
            DateTime dt = t; // implicit conversion

            // all logical values are the same
            t.Year.Should().Be(dt.Year);
            t.Month.Should().Be(dt.Month);
            t.Day.Should().Be(dt.Day);
            t.Hour.Should().Be(dt.Hour);
            t.Minute.Should().Be(dt.Minute);
            t.Second.Should().Be(dt.Second);

            // but the Tick representation is different
            t.Ticks.Should().Be.LessThan(dt.Ticks); // TeaTime.Time.Ticks < System.DateTime.Ticks
        }

        [TestMethod]
        public void UsingNetTime()
        {
            Time.Scale = Timescale.Net; // <- using System.DateTime Tick representation

            // as in the previous example
            Time t = new Time(2012, 1, 2);
            DateTime dt = t;
            t.Year.Should().Be(dt.Year);
            t.Month.Should().Be(dt.Month);
            t.Day.Should().Be(dt.Day);
            t.Hour.Should().Be(dt.Hour);
            t.Minute.Should().Be(dt.Minute);
            t.Second.Should().Be(dt.Second);

            // now, Ticks are equal!
            t.Ticks.Should().Be.EqualTo(dt.Ticks); // TeaTime.Time.Ticks == System.DateTime.Ticks
        }

        [TestMethod]
        public void JavaScaleValueTest()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(1970, 1, 3);
            t.Ticks.Should().Be(2 * 86400 * 1000);
        }

        [TestMethod]
        public void DateTimeToTimeConversionConsidersScale()
        {
            Time.Scale = Timescale.Java;
            Time t = new DateTime(1970, 1, 3);
            t.Ticks.Should().Be(2 * 86400 * 1000);
        }
    }
}
