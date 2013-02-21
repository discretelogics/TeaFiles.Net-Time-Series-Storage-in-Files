// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Header.Sections
{
    [TestClass]
    public class TimeSectionFormatterTest
    {
        // test items
        [StructLayout(LayoutKind.Explicit)]
        public struct A
        {
            [FieldOffset(0)]
            public DateTime Time;

            [FieldOffset(8)]
            public double X;
        }

        //struct B
        //{
        //    public DateTime Time;
        //    public double x;
        //    public DateTime Time2;
        //}

        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestCleanup]
        public void TearDown()
        {
            Time.Scale = Timescale.Net;
        }

        [TestMethod]
        public void ExoticCoverageTest()
        {
            ISectionFormatter f = new TimeSectionFormatter();
            f.Id.Should().Be(0x40);
        }

        [TestMethod]
        public void TimeSectionFormatterEmptyDescriptionTest()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);
            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        [TestMethod]
        public void TimeSectionFormatterDescriptionEmptyItemDescription()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            wc.Description = new TeaFileDescription();
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);
            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        [TestMethod]
        public void TimeSectionFormatterDescriptionItemDescriptionNoTimeFields()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = new ItemDescription(DescriptionSource.File);
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);
            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        public struct C
        {
            public double a;
            public int b;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct C2
        {
            [FieldOffset(1)]
            public Time Time;

            [FieldOffset(9)]
            public Time Time2;
        }

        [TestMethod]
        public void TimeSectionRoundTrip()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = ItemDescription.FromAnalysis<Event<C>>();
            wc.Description.Timescale = Time.Scale;
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);

            ms.Position = 0;
            ms.Length.Should().Be(24); // epoch(8) ticksperday(8) fieldcount(4) + timefieldoffset(4) = 24

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ItemDescription = wc.Description.ItemDescription; // this makes the test a bit weaker, but we need some item description here
            f.Read(rc);
            rc.Description.Should().Not.Be.Null();
            rc.Description.Timescale.HasValue.Should().Be.True();
        }

        [TestMethod]
        public void TimeSectionRoundTripFieldIsNotInItemDescriptionError()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = ItemDescription.FromAnalysis<Event<C>>();
            wc.Description.Timescale = Time.Scale;
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ItemDescription = ItemDescription.FromAnalysis<C2>();

            Executing.This(() => f.Read(rc)).Should().Throw<FileFormatException>();
        }

        [TestMethod]
        public void FirstTimeFieldIsAutomaticallyEventTime()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = ItemDescription.FromAnalysis<C2>();
            wc.Description.Timescale = Time.Scale;
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ItemDescription = ItemDescription.FromAnalysis<C2>();

            f.Read(rc);

            var fields = rc.Description.ItemDescription.Fields;
            fields[0].IsEventTime.Should().Be.True();
            fields[1].IsEventTime.Should().Be.False();
        }

        [TestMethod]
        public void EventTimeAttributeRoundTrip()
        {
            Time.Scale = Timescale.Net;

            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = ItemDescription.FromAnalysis<Event<C>>();
            wc.Description.Timescale = Time.Scale;
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ItemDescription = wc.Description.ItemDescription;
            rc.Description.ItemDescription.Fields.ForEach(ff => ff.IsEventTime = ff.IsTime = false); // reset flags

            f.Read(rc);

            rc.Description.ItemDescription.Fields.Count(ff => ff.IsTime).Should().Be(1);
            rc.Description.ItemDescription.Fields.Count(ff => ff.IsEventTime).Should().Be(1);
            rc.Description.Timescale.Value.Epoch.Should().Be(0);
            rc.Description.Timescale.Value.TicksPerDay.Should().Be(TimeSpan.TicksPerDay);
        }

        [TestMethod]
        public void TimeSectionValuesRoundTrip()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ItemDescription = ItemDescription.FromAnalysis<Event<C>>();
            Time.Scale = Timescale.FromEpoch(33, 77);
            wc.Description.Timescale = Time.Scale;
            ISectionFormatter f = new TimeSectionFormatter();
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ItemDescription = wc.Description.ItemDescription;

            f.Read(rc);
            rc.Description.Timescale.Value.Epoch.Should().Be(33);
            rc.Description.Timescale.Value.TicksPerDay.Should().Be(77);

            Executing.This(() => f.Read(rc)).Should().Throw<EndOfStreamException>();
        }
    }
}
