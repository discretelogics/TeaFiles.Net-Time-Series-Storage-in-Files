// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime.Layout
{
    [StructLayout(LayoutKind.Explicit)]
    struct WeirdStruct
    {
        [FieldOffset(3)]
        public Decimal dec1;

        [FieldOffset(13)]
        public sbyte i8;

        [FieldOffset(21)]
        public Int16 i16;

        [FieldOffset(29)]
        public Int32 i32;

        [FieldOffset(39)]
        public Int64 i64;

        [FieldOffset(51)]
        public byte ui8;

        [FieldOffset(57)]
        public UInt16 ui16;

        [FieldOffset(67)]
        public UInt32 ui32;

        [FieldOffset(75)]
        public UInt64 ui64;

        [FieldOffset(99)]
        public double d;

        [FieldOffset(120)]
        public float f;

        [FieldOffset(141)]
        public DateTime dt;

        [FieldOffset(153)]
        public Time t;

        [FieldOffset(165)]
        public Decimal dec;
    }

    public struct AB
    {
        public double A;
        public int B;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AB2
    {
        [FieldOffset(8)]
        public double A;

        [FieldOffset(0)]
        public int B;
    }

    public struct DateTimeOnly
    {
        public DateTime T;
    }

    public struct TimeOnly
    {
        public Time T;
    }

    [TestClass]
    public unsafe class LayoutAnalyzerTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void ExoticCoverageTest()
        {
            var la = new LayoutAnalyzer<AB>();
            Executing.This(() => la.GetOffset((byte*)0, null)).Should().Throw<ArgumentNullException>().Exception.Message.Contains("fieldPath");
            la.ReleaseHandleTestAccessor().Should().Be.True();
        }


        [TestMethod]
        public void GetStructWithOneFieldSetTest()
        {
            FieldInfo f1 = typeof (Event<AB>).GetField("Value");
            FieldInfo f2 = typeof (AB).GetField("B");

            var la = new LayoutAnalyzer<Event<AB>>();
            object testInstance = la.GetStructWithOneFieldSet(new List<FieldInfo> {f1, f2}, 711);

            testInstance.Should().Not.Be.Null();
            testInstance.Should().Be.OfType<Event<AB>>();
            Event<AB> eab = (Event<AB>)testInstance;
            eab.Value.B.Should().Be(711);
            eab.Value.A.Should().Be(0);

            f2 = typeof (AB).GetField("A");
            testInstance = la.GetStructWithOneFieldSet(new List<FieldInfo> {f1, f2}, 411);

            testInstance.Should().Not.Be.Null();
            testInstance.Should().Be.OfType<Event<AB>>();
            eab = (Event<AB>)testInstance;
            eab.Value.B.Should().Be(0);
            eab.Value.A.Should().Be(411);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DecimalStruct
        {
            [FieldOffset(191)]
            public Decimal dec;
        }

        [TestMethod]
        public void DecimalStructTest()
        {
            var la = new LayoutAnalyzer<DecimalStruct>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(af => af.Name).Should().Have.SameSequenceAs("dec");
            analyzedFields.Select(af => af.Offset).Should().Have.SameSequenceAs(191);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void WeirdStructOffsetTest()
        {
            var la = new LayoutAnalyzer<WeirdStruct>();
            List<AnalyzedField> analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(af => af.Name).Should().Have.SameSequenceAs("dec1", "i8", "i16", "i32", "i64", "ui8", "ui16", "ui32", "ui64", "d", "f", "dt", "t", "dec");
            analyzedFields.Select(af => af.Offset).Should().Have.SameSequenceAs(3, 13, 21, 29, 39, 51, 57, 67, 75, 99, 120, 141, 153, 165);

            la.TypeSize.Should().Be.GreaterThanOrEqualTo(sizeof (WeirdStruct));
            (la.TypeSize - sizeof (WeirdStruct)).Should().Be.LessThanOrEqualTo(8);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void DateTimeOnlyAnalysisTest()
        {
            var la = new LayoutAnalyzer<DateTimeOnly>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
            analyzedFields.Should().Have.Count.EqualTo(1);
        }

        [TestMethod]
        public void TimeOnlyAnalysisTest()
        {
            var la = new LayoutAnalyzer<TimeOnly>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.ForEach(Console.WriteLine);

            analyzedFields.Should().Have.Count.EqualTo(1);
            var af = analyzedFields.First();
            af.FieldPath.Last.FieldType.Should().Be(typeof (Time));
            af.Offset.Should().Be(0);
            af.Name.Should().Be("T");
        }

        public struct InvalidFieldStruct
        {
            public bool UnAvailableFieldType;
            public char UnAvailableFieldType2;
        }

        [TestMethod]
        public void UnsupportedTypeTest()
        {
            var la = new LayoutAnalyzer<InvalidFieldStruct>();
            Executing.This(() => la.AnalyzeLayout()).Should().Throw<ItemException>();
        }

        /// <summary>
        /// Note: This test depends on the layout the compiler choses for Event&lt;long&gt;. It might easily fail. If so, 
        /// set it to ignore. It is now included to get awareness about if and when it will fail.
        /// </summary>
        [TestMethod]
        public void AnalyzeLayoutReturnsCorrectLayoutForEventT()
        {
            var la = new LayoutAnalyzer<Event<long>>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(f => f.Name).Should().Have.SameSequenceAs("Time", "Value");
            analyzedFields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8);
            la.ByteLength.Should().Be(16);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void EventDoubleOffsets()
        {
            var la = new LayoutAnalyzer<Event<double>>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(f => f.FieldPath.DotPath).Should().Have.SameValuesAs("Time", "Value");
            analyzedFields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void EventABOffsets()
        {
            var la = new LayoutAnalyzer<Event<AB>>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(f => f.FieldPath.DotPath).Should().Have.SameValuesAs("Time", "A", "B");
            analyzedFields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void EventAB2Offsets()
        {
            var la = new LayoutAnalyzer<Event<AB>>();
            var analyzedFields = la.AnalyzeLayout();

            analyzedFields.Select(f => f.FieldPath.DotPath).Should().Have.SameValuesAs("Time", "B", "A");
            analyzedFields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 16, 8);

            analyzedFields.ForEach(af => Console.WriteLine(af.ToString()));
        }

        [TestMethod]
        public void NestedFieldPathsTest()
        {
            var la = new LayoutAnalyzer<Event<AB>>();
            var fieldpaths = la.GetPrimitiveFields().ToList();

            foreach (var fieldPath in fieldpaths)
            {
                Console.WriteLine(fieldPath);
            }

            fieldpaths.Should().Have.Count.EqualTo(3);
            fieldpaths.Select(fp => fp.ToString()).Should().Have.SameValuesAs("Time", "A", "B");
        }

        [TestMethod]
        public void GetFieldsOfStructTest()
        {
            var la = new LayoutAnalyzer<Event<AB>>();
            var fields = la.AnalyzeLayout();
            fields.Select(f => f.Name).Should().Have.SameValuesAs("Time", "A", "B");
            fields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16);
        }

        public struct S1
        {
            public double a;
        }

        public struct S2
        {
            public double b;
        }

        public struct S3
        {
            public S1 s1;
            public S2 s2;
        }

        public struct S4
        {
            public S3 s3;

            public S1 s1;
            public S2 s2;
        }

        [TestMethod]
        public void DotPathOfNestedStructTest()
        {
            var la = new LayoutAnalyzer<Event<S4>>();
            var fields = la.AnalyzeLayout();
            fields.Select(f => f.Name).Should().Have.SameValuesAs("Time", "s3.s1.a", "s3.s2.b", "s1.a", "s2.b");
            fields.Select(f => f.FieldPath.DotPath).Should().Have.SameValuesAs("Time", "s3.s1.a", "s3.s2.b", "s1.a", "s2.b");
        }

        [TestMethod]
        public void DottedPathIsFlattenedIfPossible()
        {
            var la = new LayoutAnalyzer<Event<S3>>();
            var fields = la.AnalyzeLayout();
            fields.Select(f => f.Name).Should().Have.SameValuesAs("Time", "a", "b");
            fields.Select(f => f.FieldPath.DotPath).Should().Have.SameValuesAs("Time", "s1.a", "s2.b");
        }

        [TestMethod]
        public void IsTimeTest()
        {
            var la = new LayoutAnalyzer<Event<AB>>();
            var fields = la.AnalyzeLayout();
            fields.Count(f => f.FieldPath.Last.FieldType == typeof (Time)).Should().Be(1);
        }
    }
}
