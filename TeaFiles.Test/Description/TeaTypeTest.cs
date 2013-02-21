// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class TeaTypeTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void TeaTypeMapping()
        {
            typeof (SByte).GetFieldType().Should().Be(FieldType.Int8);
            typeof (Int16).GetFieldType().Should().Be(FieldType.Int16);
            typeof (Int32).GetFieldType().Should().Be(FieldType.Int32);
            typeof (Int64).GetFieldType().Should().Be(FieldType.Int64);

            typeof (Byte).GetFieldType().Should().Be(FieldType.UInt8);
            typeof (UInt16).GetFieldType().Should().Be(FieldType.UInt16);
            typeof (UInt32).GetFieldType().Should().Be(FieldType.UInt32);
            typeof (UInt64).GetFieldType().Should().Be(FieldType.UInt64);

            typeof (float).GetFieldType().Should().Be(FieldType.Float);
            typeof (double).GetFieldType().Should().Be(FieldType.Double);

            typeof (Time).GetFieldType().Should().Be(FieldType.Int64);

            typeof (decimal).GetFieldType().Should().Be(FieldType.NetDecimal);

            Executing.This(() => typeof (DateTime).GetFieldType()).Should().Throw<InvalidFieldTypeException>();
            Executing.This(() => typeof (string).GetFieldType()).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TeaTypeSize()
        {
            typeof (SByte).GetFieldType().GetSize().Should().Be(sizeof (SByte));
            typeof (Int16).GetFieldType().GetSize().Should().Be(sizeof (Int16));
            typeof (Int32).GetFieldType().GetSize().Should().Be(sizeof (Int32));
            typeof (Int64).GetFieldType().GetSize().Should().Be(sizeof (Int64));

            typeof (Byte).GetFieldType().GetSize().Should().Be(sizeof (byte));
            typeof (UInt16).GetFieldType().GetSize().Should().Be(sizeof (UInt16));
            typeof (UInt32).GetFieldType().GetSize().Should().Be(sizeof (UInt32));
            typeof (UInt64).GetFieldType().GetSize().Should().Be(sizeof (UInt64));

            typeof (float).GetFieldType().GetSize().Should().Be(sizeof (float));
            typeof (double).GetFieldType().GetSize().Should().Be(sizeof (double));

            typeof (decimal).GetFieldType().GetSize().Should().Be(sizeof (decimal));

            Executing.This(() => FieldType.None.GetSize()).Should().Throw<InvalidOperationException>();
            Executing.This(() => ((FieldType)777).GetSize()).Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
