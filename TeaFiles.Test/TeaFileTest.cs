// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.Header;
using TeaTime.SampleItems;

namespace TeaTime
{
    [TestClass]
    public class
        TeaFileTest
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
            Executing.This(() => TeaFile.OpenRead((string)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile.OpenRead((Stream)null)).Should().Throw<ArgumentNullException>();
            var stream = TestUtils.GetTeaFileEventInt7Values();
            var tf = TeaFile.OpenRead(stream);
            Executing.This(() => tf.GetFieldValue(null, null)).Should().Throw<ArgumentNullException>();
            Item item = new Item(2);
            Executing.This(() => tf.GetFieldValue(item, null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void OpeningEmptyFileThrowsFileFormatException()
        {
            const string filename = "TeaFileTest_OpeningEmptyFileThrowsException.tea";
            File.WriteAllBytes(filename, new byte[0]);
            var ex = Executing.This(() => TeaFile.OpenRead(filename)).Should().Throw<FileFormatException>().Exception;
            Console.WriteLine(ex.Message);
        }

        [TestMethod]
        public void OpeningFileWithWrongSignatureThrowsFileFormatException()
        {
            const string filename = "TeaFileTest_OpeningFileWithWrongSignatureThrowsFileFormatException.tea";
            File.WriteAllBytes(filename, BitConverter.GetBytes(1700L)); // wrong signature value
            Executing.This(() => TeaFile.OpenRead(filename)).Should().Throw<FileFormatException>()
                .Exception.Message.Should().Contain("Signature");
        }

        [TestMethod]
        public void OpeningFileWithCorrrectSignatureThrowsExceptionWithoutSignatureMessage()
        {
            var filename = Path.GetTempFileName();
            File.WriteAllBytes(filename, BitConverter.GetBytes(0x0d0e0a0402080500));
            var ex = Executing.This(() => TeaFile.OpenRead(filename)).Should().Throw<FileFormatException>().Exception;
            ex.Message.Should().Not.Contain("Signature");
            Console.WriteLine(ex.Message);
        }

        [TestMethod]
        public void ZeroTeaFile()
        {
            var filename = Path.GetFullPath("TeaFileTest_ZeroTeaFile.tea");
            using (var ms = new FileStream(filename, FileMode.Create))
            {
                var w = new BinaryWriter(ms);
                w.Write(HeaderManager.Signature); // sig
                w.Write((long)32); // itemstart
                w.Write((long)0); // itemendmarker
                w.Write((long)0); // sectioncount
            }

            var fi = new FileInfo(filename);
            fi.Length.Should().Be(32);

            using (var tf = TeaFile.OpenRead(filename))
            {
                ((IItemReader)tf).CanRead.Should().Be.False();
                tf.Name.Should().Be(filename);

                Executing.This(() => tf.Items.ToString()).Should().Throw<InvalidOperationException>();
                Executing.This(() => tf.Read()).Should().Throw<InvalidOperationException>();
                Executing.This(() => tf.GetFieldValue(0, null)).Should().Throw<ArgumentNullException>();
                Executing.This(() => tf.GetEventTime(null)).Should().Throw<InvalidOperationException>();
                tf.Description.Should().Not.Be.Null();

                // in summary, a TeaFile that opens a TeaFile without description cannot do much. Being able to open it
                // gives the information that the file has no description. Moreover, a TeaFile might hold no item description 
                // but for instance a content description which would be readable although no access to the items is available.
            }
        }

        [TestMethod]
        public void ZeroTeaFilesEvilBrother()
        {
            var filename = Path.GetFullPath("TeaFileTest_ZeroTeaFilesEvilBrother.tea");
            using (var ms = new FileStream(filename, FileMode.Create))
            {
                var w = new BinaryWriter(ms);
                w.Write(HeaderManager.Signature); // sig
                w.Write((long)32); // itemstart
                w.Write((long)32); // itemendmarker <- zero tea file's evil brother has a marker with 32
                w.Write((long)0); // sectioncount
            }

            var fi = new FileInfo(filename);
            fi.Length.Should().Be(32);

            using (var tf = TeaFile.OpenRead(filename))
            {
                ((IItemReader)tf).CanRead.Should().Be.False();
                tf.Name.Should().Be(filename);

                tf.Description.Should().Not.Be.Null();

                Executing.This(() => tf.Items.ToString()).Should().Throw<InvalidOperationException>();
                Executing.This(() => tf.Read()).Should().Throw<InvalidOperationException>();
            }
        }

        [TestMethod]
        public void ZeroTeaFilesGoodBrother()
        {
            var filename = Path.GetFullPath(MethodBase.GetCurrentMethod() + ".tea");
            using (var ms = new FileStream(filename, FileMode.Create))
            {
                var w = new BinaryWriter(ms);
                w.Write(HeaderManager.Signature); // sig
                w.Write((long)32); // itemstart
                w.Write((long)32); // itemendmarker <- zero tea file's evil brother has a marker with 32
                w.Write((long)0); // sectioncount
                10.Times(() => w.Write(0L)); // increase file length, now the preallocation makes sense, and evil brother becomes a good one
            }

            var fi = new FileInfo(filename);
            fi.Length.Should().Be(32 + 10 * 8);

            using (var tf = TeaFile.OpenRead(filename))
            {
                ((IItemReader)tf).CanRead.Should().Be.False();
                tf.Name.Should().Be(filename);

                tf.Description.Should().Not.Be.Null(); // the description should never be null

                Executing.This(() => tf.Items.ToString()).Should().Throw<InvalidOperationException>();
                Executing.This(() => tf.Read()).Should().Throw<InvalidOperationException>();
            }
        }

        [TestMethod]
        public void DateTimeFieldMappingIsCorrect()
        {
            const string filename = "TeaFileTest_DateTimeFieldMappingIsCorrect.tea";
            using (var tf = TeaFile<OHLCV3>.Create(filename))
            {
                tf.Write(new OHLCV3 {High = 71, Low = 20, Open = 33, Close = 21, Time = new DateTime(2000, 7, 6)});
                tf.Write(new OHLCV3 {High = 72, Low = 20, Open = 34, Close = 22, Time = new DateTime(2000, 3, 4)});
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                var id = tf.Description.ItemDescription;
                var item = tf.Read();
                var timeField = id.GetFieldByName("Time");
                tf.GetFieldValue(item, timeField).Should().Be.OfType<Time>();
            }
        }

        [TestMethod]
        public void TimeValuesAreCorrect()
        {
            Time.Scale = Timescale.Net;

            const string filename = "TeaFileTest_TimeValuesAreCorrect.tea";
            using (var tf = TeaFile<OHLCV3>.Create(filename))
            {
                tf.Write(new OHLCV3 {High = 71, Low = 20, Open = 33, Close = 21, Time = new DateTime(2000, 7, 6)});
                tf.Write(new OHLCV3 {High = 72, Low = 20, Open = 34, Close = 22, Time = new DateTime(2000, 7, 11)});
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                tf.Items.Count.Should().Be(2);
                var timeField = tf.Description.ItemDescription.GetFieldByName("Time");

                var item = tf.Read();
                var time = tf.GetFieldValue(item, timeField);
                time.Should().Be(new Time(2000, 7, 6));

                item = tf.Read();
                time = tf.GetFieldValue(item, timeField);
                time.Should().Be(new Time(2000, 7, 11));
            }
        }

        [TestMethod]
        public void Description()
        {
        }

        [TestMethod]
        public void TimeScale()
        {
            Time.Scale = Timescale.Java;
            var stream = new MemoryStream();
            using (var tf = TeaFile<Event<OHLCV>>.Create(stream))
            {
                tf.Write(new Event<OHLCV> {Time = new Time(1970, 1, 1), Value = new OHLCV {Open = 11, Close = 22}});
                tf.Write(new Event<OHLCV> {Time = new Time(1970, 1, 2), Value = new OHLCV {Open = 11, Close = 22}});
                tf.Write(new Event<OHLCV> {Time = new Time(1970, 1, 3), Value = new OHLCV {Open = 11, Close = 22}});
            }
            stream.Position = 0;
            using (var tf = TeaFile.OpenRead(stream))
            {
                tf.Description.Timescale.Value.Epoch.Should().Be(719162);
                tf.Description.Timescale.Value.TicksPerDay.Should().Be(Timescale.MillisecondsPerDay);

                var time = tf.Description.ItemDescription.GetFieldByName("Time");
                var open = tf.Description.ItemDescription.GetFieldByName("Open");

                foreach (var item in tf.Items)
                {
                    Console.WriteLine(item);
                    Console.WriteLine(tf.GetFieldValue(item, time));
                    Console.WriteLine(tf.GetFieldValue(item, open));
                }

                tf.GetFieldValue(tf.Items[0], time).Should().Be(new Time(1970, 1, 1));

                tf.GetFieldValue(0, time).Should().Be(new Time(1970, 1, 1));
                tf.GetFieldValue(1, time).Should().Be(new Time(1970, 1, 2));
                tf.GetFieldValue(2, time).Should().Be(new Time(1970, 1, 3));

                var t = (Time)tf.GetFieldValue(0, time);
                t.Should().Be(new DateTime(1970, 1, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PaddedStruct
        {
            [FieldOffset(1)]
            public Int32 i;

            [FieldOffset(7)]
            public Int16 s;

            [FieldOffset(14)]
            public byte b;
        }

        [TestMethod]
        public void FieldSpacingsComputation()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<PaddedStruct>.Create(stream))
            {
            }
            stream.Position = 0;
            using (var tf = TeaFile.OpenRead(stream))
            {
                var itemSize = tf.Description.ItemDescription.ItemSize;
                tf.GetFieldSpacings().Should().Have.SameSequenceAs(1, 2, 5, itemSize - 15);
            }
        }

        [TestMethod]
        public void PaddingRead()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<PaddedStruct>.Create(stream))
            {
                tf.Write(new PaddedStruct {i = 7, s = 3, b = 9});
                tf.Write(new PaddedStruct {i = 17, s = 13, b = 19});
            }
            stream.Position = 0;
            using (var tf = TeaFile.OpenRead(stream))
            {
                var item = tf.Read();
                item.Values.Should().Have.SameValuesAs(7, (short)3, (byte)9);
            }
        }

        [TestMethod]
        public void TimeScaleCollisionThrowsTimescaleException()
        {
            Time.ScaleCollisionBehavior = ScaleCollisionBehavior.ThrowException;
            Time.Scale = Timescale.FromEpoch(2000, 1, 1, 86400);
            var stream = new MemoryStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
            }
            stream.Position = 0;
            Time.Scale = Timescale.Net;
            Executing.This(() => TeaFile.OpenRead(stream)).Should().Throw<TimescaleException>();
        }

        [TestMethod]
        public void TimeScaleCollisionIgnored()
        {
            Time.ScaleCollisionBehavior = ScaleCollisionBehavior.Ignore;
            Time.Scale = Timescale.FromEpoch(2000, 1, 1, 86400);
            var stream = new MemoryStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
            }
            stream.Position = 0;
            Time.Scale = Timescale.Net;
            TeaFile.OpenRead(stream);
        }

        [TestMethod]
        public void TimeScaleCollisionUse()
        {
            Time.ScaleCollisionBehavior = ScaleCollisionBehavior.UseNewScale;
            Time.Scale = Timescale.FromEpoch(2000, 1, 1, 86400);
            var stream = new MemoryStream();
            using (TeaFile<Event<int>>.Create(stream))
            {
            }
            stream.Position = 0;
            Time.Scale = Timescale.Net;
            TeaFile.OpenRead(stream);
            Time.Scale.Should().Be(Timescale.FromEpoch(2000, 1, 1, 86400));
        }

        [TestMethod]
        public void NameValueDescription()
        {
            var stream = new MemoryStream();
            var namevalues = new NameValueCollection();
            namevalues.Add("name1", 123).Add("name2", 1.23);
            using (var tf = TeaFile<Event<int>>.Create(stream, "my text", namevalues))
            {
            }
            stream.Position = 0;
            using (var tf = TeaFile.OpenRead(stream))
            {
                tf.Description.NameValues.GetValue<int>("name1").Should().Be(123);
                tf.Description.NameValues.GetValue<double>("name2").Should().Be(1.23);
                tf.Description.ContentDescription.Should().Be("my text");
            }
        }

        [TestMethod]
        public void ItemString()
        {
            var stream = TestUtils.GetTeaFileEventInt7Values();
            using (var tf = TeaFile.OpenRead(stream))
            {
                tf.Items.Count.Should().Be(7);
                var id = tf.Description.ItemDescription;

                // demo - view the test output to see the results
                Console.WriteLine("namevalue");
                tf.Items.ForEach(item => Console.WriteLine(id.GetValueString(item)));
                Console.WriteLine("separated");
                tf.Items.ForEach(item => Console.WriteLine(id.GetValueString(item, " ~ ")));
                Console.WriteLine("time");
                tf.Items.ForEach(item => Console.WriteLine(tf.GetEventTime(item)));

                var e = tf.Items.GetEnumerator();
                e.MoveNext().Should().Be.True();
                id.GetValueString(e.Current).Should().Be(new DateTime(2000, 1, 1) + " 0");
                e.MoveNext().Should().Be.True();
                id.GetValueString(e.Current).Should().Be(new DateTime(2000, 1, 2) + " 1100");
            }
        }

        [TestMethod]
        public void EventTimeFieldIsNullIfNoSuchFieldExists()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Write(1);
            }
            stream.Position = 0;
            using (var tf = TeaFile.OpenRead(stream))
            {
                tf.Description.ItemDescription.EventTimeField.Should().Be.Null();
            }
        }

        [TestMethod]
        public void UntypedEnumeratorTest()
        {
            var filename = "TeaFileTest_UntypedEnumeratorTest.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.Write(Enumerable.Range(10, 3));
            }
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
                var e = ((IEnumerable)tf.Items).GetEnumerator();
                e.MoveNext().Should().Be.True();
                e.Current.Should().Be(10);
                e.MoveNext().Should().Be.True();
                e.Current.Should().Be(11);
                e.MoveNext().Should().Be.True();
                e.Current.Should().Be(12);
                e.MoveNext().Should().Be.False();
            }
        }

        [TestMethod]
        public void ItemAreaTest()
        {
            var filename = "TeaFileTest_ItemAreaTest.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.ItemAreaSize.Should().Be(0);
                tf.Write(3);
                tf.ItemAreaSize.Should().Be(4);
                tf.Write(71);
                tf.ItemAreaSize.Should().Be(8);
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                tf.ItemAreaSize.Should().Be(8);
                tf.ItemAreaStart.Should().Be.GreaterThan(0);
                tf.ItemAreaEnd.Should().Be.GreaterThan(0);
                tf.ItemAreaSize.Should().Be.EqualTo(tf.ItemAreaEnd - tf.ItemAreaStart);
            }
        }
    }
}
