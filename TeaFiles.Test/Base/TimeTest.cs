// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    /// <summary>
    /// Test of the Time class.
    ///  </summary>
    /// <remarks>
    /// The test here must not run concurrently, as the static property <see cref="Time.Scale"/> is modified.
    /// The current test settings adhere to this requirement. When using other test runners or new test settings, 
    /// keep this in mind.
    /// <seealso cref="http://blogs.msdn.com/b/vstsqualitytools/archive/2009/12/01/executing-unit-tests-in-parallel-on-a-multi-cpu-core-machine.aspx"/>
    /// </remarks>
    [TestClass]
    public class TimeTest
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
            Time t = new Time(1986, 4, 5);
            t.GetHashCode().Should().Be(t.Ticks.GetHashCode());
        }

        [TestMethod]
        public unsafe void TimeFootprintIs8()
        {
            sizeof (Time).Should().Be(8);
        }

        [TestMethod]
        public void UnixEpochTestTime0()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(1970, 1, 1);
            t.Ticks.Should().Be(0);
        }

        [TestMethod]
        public void UnixEpochTestDay1()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(1970, 1, 2);
            t.Ticks.Should().Be(86400 * 1000);
        }

        [TestMethod]
        public void UnixEpochTestDayMinus1()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(1969, 12, 31);
            t.Ticks.Should().Be(- 86400 * 1000);
        }

        [TestMethod]
        public void UnixDateAndTime()
        {
            Time.Scale = Timescale.Java;

            Time t = new Time(1980, 5, 9, 10, 22, 7, 133);
            t.Year.Should().Be(1980);
            t.Month.Should().Be(5);
            t.Day.Should().Be(9);
            t.Hour.Should().Be(10);
            t.Minute.Should().Be(22);
            t.Second.Should().Be(7);
            t.Millisecond.Should().Be(133);

            DateTime dt = t;
            t.Ticks.Should().Be.LessThan(dt.Ticks);

            t.DayOfWeek.Should().Be(dt.DayOfWeek);
            t.DayOfYear.Should().Be(dt.DayOfYear);
        }

        [TestMethod]
        public unsafe void PointerAccess()
        {
            Time.Scale = Timescale.Java;

            long ticks = 1000L * 86400L * 3; // the 3rd day in java time epoch
            Time* pt = (Time*)(&ticks);

            pt->Year.Should().Be(1970);
            pt->Month.Should().Be(1);
            pt->Day.Should().Be(4);
            pt->NetTime.TimeOfDay.Should().Be(TimeSpan.Zero);
        }

        [TestMethod]
        public unsafe void PointerToJavaTimeAssignedToDateTime()
        {
            Time.Scale = Timescale.Java;

            long ticks = 1000L * 86400L * 3; // the 3rd day in java time epoch
            Time* pt = (Time*)(&ticks);
            DateTime dt = *pt;

            dt.Should().Be(new DateTime(1970, 1, 4));
        }

        [TestMethod]
        public unsafe void ComparisonTimevsDateTime()
        {
            Time.Scale = Timescale.Java;
            long ticks = 1000L * 86400L * 3; // the 3rd day in java time epoch
            Time* pt = (Time*)(&ticks);
            pt->Year.Should().Be(1970);
            pt->Month.Should().Be(1);
            pt->Day.Should().Be(4);

            var dt = new DateTime(1970, 1, 4);

            (*pt == dt).Should().Be.True();
            (dt == *pt).Should().Be.True();
        }

        [TestMethod]
        public void J2000EpochDayResolution()
        {
            Time.Scale = Timescale.FromEpoch(2000, 1, 1, 1);
            Time t = new Time(2000, 1, 2);
            t.Ticks.Should().Be(1);
            t.Year.Should().Be(2000);
            t.Month.Should().Be(1);
            t.Day.Should().Be(2);
        }

        [TestMethod]
        public void J2000EpochDay365Resolution()
        {
            Time.Scale = Timescale.FromEpoch(2000, 1, 1, 1);
            Time t = new Time(2001, 1, 1);
            t.Ticks.Should().Be(366);
            t.Year.Should().Be(2001);
            t.Month.Should().Be(1);
            t.Day.Should().Be(1);
        }

        [TestMethod]
        public void NetTime()
        {
            Time.Scale = Timescale.Java;

            var t = new Time(2050, 1, 1);
            Console.WriteLine(t.ToString("YYYY.MM.DD HH:mm:ss"));

            var dt = new DateTime(2050, 1, 1);
            Console.WriteLine(dt.ToString("YYYY.MM.DD HH:mm:ss"));

            (t.NetTime == dt).Should().Be.True();

            t.Year.Should().Be(dt.Year);
            t.Month.Should().Be(dt.Month);
            t.Day.Should().Be(dt.Day);
            t.Hour.Should().Be(dt.Hour);
            t.Minute.Should().Be(dt.Minute);
            t.Second.Should().Be(dt.Second);
        }

        [TestMethod]
        public void ConversionTimeToNetTime()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(2011, 5, 9);

            DateTime dt = t;

            dt.Year.Should().Be(2011);
            dt.Month.Should().Be(5);
            dt.Day.Should().Be(9);
            dt.TimeOfDay.Ticks.Should().Be(0);

            t.Year.Should().Be(2011);
            t.Month.Should().Be(5);
            t.Day.Should().Be(9);
            t.NetTime.TimeOfDay.Ticks.Should().Be(0);
        }

        [TestMethod]
        public void TicksDifferForJavaScale()
        {
            Time.Scale = Timescale.Java;
            Time t = new Time(2011, 5, 9);

            DateTime dt = t;

            t.Ticks.Should().Be.LessThan(dt.Ticks);
        }

        [TestMethod]
        public void TimeScale()
        {
            Time.Scale = Timescale.Java;
            Time.Scale.TicksPerDay.Should().Be(Timescale.MillisecondsPerDay);
            Time.Scale = Timescale.Net;
            Time.Scale.TicksPerDay.Should().Be(Timescale.MicrosecondsPerDay * 10);
            Time.Scale = Timescale.Java;
            Time.Scale.TicksPerDay.Should().Be(Timescale.MillisecondsPerDay);
        }

        [TestMethod]
        public void ToStringFormat()
        {
            Time t = new Time(1972, 5, 9, 10, 20, 7, 133);
            t.ToString("yyyy_MM_dd HH:mm:ss:fff").Should().Be("1972_05_09 10:20:07:133");
        }

        [TestMethod]
        public void Equality()
        {
            Time t1 = new Time(2000, 1, 2, 3, 4, 5, 6);
            Time t2 = new Time(2003, 9, 1, 13, 44, 55, 6);
            Time t3 = new Time(2000, 1, 2, 3, 4, 5, 6);

            t1.Equals(t2).Should().Be.False();
            t2.Equals(t1).Should().Be.False();
            (t1 == t2).Should().Be.False();
            (t2 == t1).Should().Be.False();
            (t1 != t2).Should().Be.True();
            (t2 != t1).Should().Be.True();

            t1.Equals(t3).Should().Be.True();
            t3.Equals(t1).Should().Be.True();
            (t1 == t3).Should().Be.True();
            (t3 == t1).Should().Be.True();
            (t1 != t3).Should().Be.False();
            (t3 != t1).Should().Be.False();
        }

        [TestMethod]
        public void DateTimeInterop()
        {
            Time t1 = new Time(2000, 1, 2, 3, 4, 5, 6);
            DateTime t2 = new DateTime(2003, 9, 1, 13, 44, 55, 6);
            DateTime t3 = new DateTime(2000, 1, 2, 3, 4, 5, 6);

            t1.Equals(t2).Should().Be.False();
            t2.Equals(t1).Should().Be.False();
            (t1 == t2).Should().Be.False();
            (t2 == t1).Should().Be.False();
            (t1 != t2).Should().Be.True();
            (t2 != t1).Should().Be.True();

            t1.Equals(t3).Should().Be.True();
            t3.Equals(t1).Should().Be.True();
            (t1 == t3).Should().Be.True();
            (t3 == t1).Should().Be.True();
            (t1 != t3).Should().Be.False();
            (t3 != t1).Should().Be.False();
        }

        [TestMethod]
        public void AddDays()
        {
            Time.Scale = Timescale.Net;
            Time t = new Time(2010, 2, 3);
            Time t2 = t.AddDays(5);
            (t2.NetTime - t.NetTime).TotalDays.Should().Be(5);

            Time.Scale = Timescale.Java;
            t = new Time(2010, 2, 3);
            t2 = t.AddDays(5);
            (t2.NetTime - t.NetTime).TotalDays.Should().Be(5);
        }

        [TestMethod]
        public void AddMinutes()
        {
            Time t = new Time(2010, 2, 3);
            Time t2 = t.AddMinutes(5);

            (t2.NetTime - t.NetTime).TotalMinutes.Should().Be(5);
        }

        [TestMethod]
        public void AddSeconds()
        {
            Time t = new Time(2010, 2, 3);
            Time t2 = t.AddSeconds(5);

            (t2.NetTime - t.NetTime).TotalSeconds.Should().Be(5);
        }

        [TestMethod]
        public void Operators()
        {
            Time t1 = new Time(2000, 1, 2);
            Time t1b = new Time(2000, 1, 2);
            Time t2 = new Time(2000, 3, 4);

            (t1 == t1b).Should().Be.True();
            (t1 != t1b).Should().Be.False();
            (t1 > t1b).Should().Be.False();
            (t1 < t1b).Should().Be.False();

            (t1 == t2).Should().Be.False();
            (t1 != t2).Should().Be.True();
            (t1 > t2).Should().Be.False();
            (t1 < t2).Should().Be.True();
        }

        [TestMethod]
        public void ConversionFromNetTimeTest()
        {
            Time.Scale = Timescale.Net;
            DateTime dt = DateTime.Now;
            dt.Kind.Should().Be(DateTimeKind.Local);
            Time t = dt;
            t.Ticks.Should().Be(dt.Ticks);
            DateTime dt2 = t;
            dt2.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        [TestMethod]
        public void DateTest()
        {
            Time.Scale = Timescale.Java;

            var t = new Time(2000, 1, 1, 0, 0, 0, 0);
            t.Date.Should().Be.EqualTo(t);

            t = new Time(2000, 1, 1, 3, 4, 5, 66);
            t.Date.Should().Be.EqualTo(new Time(2000, 1, 1));

            t = new Time(2000, 1, 1, 0, 0, 0, 66);
            t.Date.Should().Be.EqualTo(new Time(2000, 1, 1));

            Time.Scale = Timescale.Net;

            t = new Time(2000, 1, 1, 0, 0, 0, 0);
            t.Date.Should().Be.EqualTo(t);

            t = new Time(2000, 1, 1, 3, 4, 5, 66);
            t.Date.Should().Be.EqualTo(new Time(2000, 1, 1));

            t = new Time(2000, 1, 1, 0, 0, 0, 66);
            t.Date.Should().Be.EqualTo(new Time(2000, 1, 1));
        }
    }
}
