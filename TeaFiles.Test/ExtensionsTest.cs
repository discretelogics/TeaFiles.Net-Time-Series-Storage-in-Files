// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void ToSafeStringTest()
        {
            7.SafeToString("o is null").Should().Be(7.ToString());

            object o = null;
            o.SafeToString("o is null").Should().Be("o is null");
        }

        [TestMethod]
        public void ToSafeStringEmptyTest()
        {
            7.SafeToString().Should().Be(7.ToString());

            object o = null;
            o.SafeToString().Should().Be("empty");
        }

        [TestMethod]
        public void IsDefinedTest()
        {
            MethodBase.GetCurrentMethod().IsDefined<TestMethodAttribute>().Should().Be.True();
            MethodBase.GetCurrentMethod().IsDefined<SerializableAttribute>().Should().Be.False();
        }

        [TestMethod]
        public void TimesTest()
        {
            int n = 0;
            0.Times(() => n++);
            n.Should().Be(0);

            3.Times(() => n++);
            n.Should().Be(3);
        }

        struct Test
        {
            public string S;
            public int N;
        }

        [TestMethod]
        public void ToStringFromFieldsTest()
        {
            Test t = new Test {S = "s", N = 71};
            t.ToStringFromFields("#").Should().Be("S=s#N=71");
        }

        [TestMethod]
        public void ToStringFromFieldsDefaultTest()
        {
            Test t = new Test {S = "s", N = 71};
            t.ToStringFromFields().Should().Be("S=s N=71");
        }

        [TestMethod]
        public void FormattedTest()
        {
            "{0} => {1}".Formatted(42, 71).Should().Be("42 => 71");
        }

        [TestMethod]
        public void JoinedTest()
        {
            var arr = new[] {2, 3, 5};
            arr.Joined("-").Should().Be("2-3-5");
        }

        [TestMethod]
        public void GetFirstPartTest()
        {
            "123456".GetFirstPart('4').Should().Be("123");
            "123456".GetFirstPart('9').Should().Be("123456");
            "".GetFirstPart('9').Should().Be("");
        }

        [TestMethod]
        public void ForEachTest()
        {
            int i = 0;
            IEnumerable<double> e = new[] {1.1, 2.2, 3.3};
            var list = e.ToList();
            e.ForEach((v) => v.Should().Be(list[i++]));
        }

        [TestMethod]
        public void ReadTeaType()
        {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            w.Write((byte)11);
            w.Write((UInt16)22);
            w.Write(33);
            w.Write((UInt64)44L);
            w.Write((sbyte)-11);
            w.Write((short)-22);
            w.Write(-33);
            w.Write(-44L);
            w.Write(7.22f);
            w.Write(1.23d);
            w.Write(12345m);

            ms.Position = 0;
            var r = new BinaryReader(ms);
            r.Read(FieldType.UInt8).Should().Be((byte)11);
            r.Read(FieldType.UInt16).Should().Be((UInt16)22u);
            r.Read(FieldType.UInt32).Should().Be(33u);
            r.Read(FieldType.UInt64).Should().Be(44Lu);

            r.Read(FieldType.Int8).Should().Be((sbyte)-11);
            r.Read(FieldType.Int16).Should().Be((Int16)(-22));
            r.Read(FieldType.Int32).Should().Be(-33);
            r.Read(FieldType.Int64).Should().Be((-44L));

            r.Read(FieldType.Float).Should().Be(7.22f);
            r.Read(FieldType.Double).Should().Be(1.23d);
            r.Read(FieldType.NetDecimal).Should().Be(12345m);

            Executing.This(() => r.Read((FieldType)777)).Should().Throw<ArgumentOutOfRangeException>();
            Executing.This(() => ((BinaryReader)null).Read(FieldType.UInt8)).Should().Throw<ArgumentNullException>();
        }
    }
}
