// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.SampleItems;

namespace TeaTime
{
    namespace Other
    {
        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct A2 // size difference
        {
        }
    }

    namespace FieldOffsetChange
    {
        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct A2
        {
            [FieldOffset(1)] // field offset change
                public double a;

            [FieldOffset(8)]
            public double b;
        }
    }

    namespace FieldTypeChange
    {
        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct A2
        {
            [FieldOffset(0)]
            public int a; // field type change

            [FieldOffset(8)]
            public double b;
        }
    }

    namespace FieldNameChange
    {
        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct A2
        {
            [FieldOffset(0)]
            public double aa; // field name change

            [FieldOffset(8)]
            public double b;
        }
    }

    [TestClass]
    public class ItemDescriptionTest
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
            var id = new ItemDescription(DescriptionSource.File);
            Executing.This(() => id.GetFieldByName(null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => id.GetFieldByName("nonexistingfield")).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public unsafe void Fields()
        {
            var id = ItemDescription.FromAnalysis<TOHLCV>();

            id.ItemTypeName.Should().Be("TOHLCV");
            id.ItemSize.Should().Be.GreaterThanOrEqualTo(sizeof (TOHLCV));
            id.ItemSize.Should().Be.LessThanOrEqualTo(sizeof (TOHLCV) + 8);

            id.Fields.Select(f => f.Name).Should().Have.SameValuesAs("Time", "Open", "High", "Low", "Close", "Volume");
            id.Fields.Select(f => f.Index).Should().Have.SameSequenceAs(0, 1, 2, 3, 4, 5);

            // the following assertions might fail, since the layout is determined by the Jit compiler and depends on the runtime
            if (Environment.Is64BitProcess)
            {
                id.Fields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16, 24, 32, 40);
            }
            else
            {
                id.Fields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16, 24, 32, 40);
            }
        }

        /// <summary>
        /// A failure of this test does not indicate an error.
        /// </summary>
        [TestMethod]
        public void FieldsOffset_might_fail_dependent_on_Jit_TypeLayout_Decision()
        {
            var id = ItemDescription.FromAnalysis<TOHLCV>();

            // the following assertions might fail, since the layout is determined by the Jit compiler and depends on the runtime
            if (Environment.Is64BitProcess)
            {
                id.Fields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16, 24, 32, 40);
            }
            else
            {
                id.Fields.Select(f => f.Offset).Should().Have.SameValuesAs(0, 8, 16, 24, 32, 40);
            }
        }

        [TestMethod]
        public void EmptyStructIsNotSupported()
        {
            Executing.This(() => ItemDescription.FromAnalysis<EmptyStruct>()).Should().Throw<ItemException>();
        }

        [TestMethod]
        public void IsAccessible()
        {
            var id1 = ItemDescription.FromAnalysis<Event<OHLCV>>();
            var id2 = ItemDescription.FromAnalysis<Event<OHLCV>>();
            id1.IsAccessibleWith(id2, ItemDescriptionElements.All);
        }

        public struct A
        {
            public double a;
            public double b;
        }

        public struct A1 // name devaiation
        {
            public double a;
            public double b;
        }

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct A2 // size devaiation
        {
            [FieldOffset(0)]
            public double a;

            [FieldOffset(8)]
            public double b;
        }

        [TestMethod]
        public void IsAccessibleDetectsDifferentName()
        {
            var ida = ItemDescription.FromAnalysis<Event<A>>();
            var ida1 = ItemDescription.FromAnalysis<Event<A1>>();
            ida.IsAccessibleWith(ida1, ItemDescriptionElements.None);
            var ex = Executing.This(() => ida.IsAccessibleWith(ida1, ItemDescriptionElements.ItemName)).Should().Throw<TypeMismatchException>().Exception;
            Console.WriteLine(ex);
        }

        [TestMethod]
        public void IsAccessibleDetectsDifferentSize()
        {
            var ida = ItemDescription.FromAnalysis<Event<A>>();
            var ida1 = ItemDescription.FromAnalysis<Event<A2>>();
            ida.IsAccessibleWith(ida1, ItemDescriptionElements.None);
            var ex = Executing.This(() => ida.IsAccessibleWith(ida1, ItemDescriptionElements.ItemSize)).Should().Throw<TypeMismatchException>().Exception;
            Console.WriteLine(ex);
        }

        [TestMethod]
        public unsafe void IsAccessibleDetectsStructHasNoFieldsException()
        {
            sizeof (A2).Should().Be(20); // has 2 fields, a, b
            sizeof (Other.A2).Should().Be(20); // an empty struct

            var ida = ItemDescription.FromAnalysis<A2>();
            var ida2 = new ItemDescription(DescriptionSource.File);
            ida2.ItemSize = 20;
            ida2.ItemTypeName = "A2";

            ida.IsAccessibleWith(ida2, ItemDescriptionElements.None);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemName);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemSize);
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldOffsets))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields");
            Executing.This(() => ida.IsAccessibleWith(ida2, ItemDescriptionElements.FieldOffsets))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields Accessor");

            // FieldNames checking includes FieldOffset checking, so the exception remains its Source
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldNames))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields");
            Executing.This(() => ida.IsAccessibleWith(ida2, ItemDescriptionElements.FieldNames))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields Accessor");

            // FieldTypes checking includes FieldOffset checking, so the exception remains its Source
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldTypes))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields");
            Executing.This(() => ida.IsAccessibleWith(ida2, ItemDescriptionElements.FieldTypes))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("No ItemFields Accessor");
        }

        [TestMethod]
        public unsafe void IsAccessibleDetectsFieldOffsetDifference()
        {
            sizeof (A2).Should().Be(20);
            sizeof (FieldOffsetChange.A2).Should().Be(20);

            var ida = ItemDescription.FromAnalysis<A2>();
            var ida2 = ItemDescription.FromAnalysis<FieldOffsetChange.A2>();

            ida.IsAccessibleWith(ida2, ItemDescriptionElements.None);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemName);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemSize);
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldOffsets))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("FieldOffsets Check");
        }

        [TestMethod]
        public unsafe void IsAccessibleDetectsFieldNameDifference()
        {
            sizeof (A2).Should().Be(20);
            sizeof (FieldNameChange.A2).Should().Be(20);

            var ida = ItemDescription.FromAnalysis<A2>();
            var ida2 = ItemDescription.FromAnalysis<FieldNameChange.A2>();

            ida.IsAccessibleWith(ida2, ItemDescriptionElements.None);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemName);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemSize);
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldNames))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("FieldNames Check");
        }

        [TestMethod]
        public unsafe void IsAccessibleDetectsFieldTypeDifference()
        {
            sizeof (A2).Should().Be(20);
            sizeof (FieldTypeChange.A2).Should().Be(20);

            var ida = ItemDescription.FromAnalysis<A2>();
            var ida2 = ItemDescription.FromAnalysis<FieldTypeChange.A2>();

            ida.IsAccessibleWith(ida2, ItemDescriptionElements.None);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemName);
            ida.IsAccessibleWith(ida2, ItemDescriptionElements.ItemSize);
            Executing.This(() => ida2.IsAccessibleWith(ida, ItemDescriptionElements.FieldTypes))
                .Should().Throw<TypeMismatchException>()
                .Exception.Source.Should().Be("FieldTypes Check");
        }

        public struct UnTimed
        {
            public double d;
        }

        public struct Timed
        {
            public Time time;
        }

        public struct DateTimed
        {
            public DateTime time;
        }

        [TestMethod]
        public void TimeSeriesFlag()
        {
            var ida = ItemDescription.FromAnalysis<UnTimed>();
            ida.HasEventTime.Should().Be.False();

            var ida2 = ItemDescription.FromAnalysis<Event<UnTimed>>();
            ida2.HasEventTime.Should().Be.True();

            var ida3 = ItemDescription.FromAnalysis<Event<Timed>>();
            ida3.HasEventTime.Should().Be.True();

            Executing.This(() => ItemDescription.FromAnalysis<Event<DateTimed>>()).Should().Throw<InvalidFieldTypeException>();
        }

        public struct PrivateTimed
        {
            public Time time;
        }

        [TestMethod]
        public void TheFirstTimeFieldIsUsedAsEventTime()
        {
            var ida = ItemDescription.FromAnalysis<PrivateTimed>();
            ida.HasEventTime.Should().Be.True();
        }

        [TestMethod]
        public void EventTimeIsSetWithoutEventTimeAttribute()
        {
            ItemDescription id = ItemDescription.FromAnalysis<TOHLCV>();
            id.Fields.Count(f => f.IsEventTime).Should().Be(1);
        }

#pragma warning disable 0649
        struct Timed1
        {
            public Time T1;
        }

        struct Timed2
        {
            public Time T1;
            public Time T2;
        }

        struct Timed3
        {
            public Time T1;

            [EventTime]
            public Time T2;
        }
#pragma warning restore 0649

        [TestMethod]
        public void EventTimeAttributeTest()
        {
            var id = ItemDescription.FromAnalysis<Timed1>();
            id.Fields[0].IsEventTime.Should().Be.True();

            id = ItemDescription.FromAnalysis<Timed2>();
            id.Fields[0].IsEventTime.Should().Be.True();
            id.Fields[1].IsEventTime.Should().Be.False();

            id = ItemDescription.FromAnalysis<Timed3>();
            id.Fields[0].IsEventTime.Should().Be.False();
            id.Fields[1].IsEventTime.Should().Be.True();
        }
    }
}
