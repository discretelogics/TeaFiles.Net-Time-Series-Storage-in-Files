// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace TeaTime.IO
{
    [TestClass]
    public class FormattedReaderTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void FormattedReaderExoticCoverageTest()
        {
            var fio = new Mock<IFileIO>();
            var r = new FormattedReader(fio.Object);
            fio.Setup(io => io.ReadBytes(0)); // will cause name to be the empty string
            fio.Setup(io => io.ReadInt32()).Returns((int)NameValue.ValueKind.Invalid);
            Executing.This(() => r.ReadNameValue()).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CtorTest()
        {
            var fio = new Mock<IFileIO>();
            new FormattedReader(fio.Object);
        }

        [TestMethod]
        public void ReadInt32Test()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.ReadInt32()).Returns(0x7fcc1111);
            var r = new FormattedReader(fio.Object);

            r.ReadInt32().Should().Be(0x7fcc1111);

            fio.Verify(f => f.ReadInt32(), Times.Once());
        }

        [TestMethod]
        public void ReadInt64Test()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.ReadInt64()).Returns(0x7fcc111122223333);
            var r = new FormattedReader(fio.Object);

            r.ReadInt64().Should().Be(0x7fcc111122223333);

            fio.Verify(f => f.ReadInt64(), Times.Once());
        }

        [TestMethod]
        public void ReadDoubleTest()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.ReadDouble()).Returns(1.2345);
            var r = new FormattedReader(fio.Object);

            r.ReadDouble().Should().Be(1.2345);

            fio.Verify(f => f.ReadDouble(), Times.Once());
        }

        // ReadGuid was changed to private, since its intentional use is inside header reading only
        //[TestMethod]
        //public void ReadGuidTest()
        //{
        //    Guid g = new Guid("1976C195-8975-4E73-B777-23E1C548BC71");
        //    var fio = new Mock<IFileIO>(MockBehavior.Strict);
        //    fio.Setup(io => io.ReadBytes(16)).Returns(g.ToByteArray());
        //    var r = new FormattedReader(fio.Object);

        //    r.ReadGuid().Should().Be(g);

        //    fio.Verify(f => f.ReadBytes(16), Times.Once());
        //}

        // ReadBytes was moved to FileIO
        //[TestMethod]
        //public void ReadBytesTest()
        //{
        //    var fio = new Mock<IFileIO>(MockBehavior.Strict);
        //    fio.Setup(io => io.ReadInt32()).Returns(3);
        //    fio.Setup(io => io.ReadBytes(3)).Returns(new byte[]{7,4,11});
        //    var r = new FormattedReader(fio.Object);

        //    r.ReadBytes().Should().Have.SameSequenceAs(new byte[]{7,4,11});

        //    fio.Verify(f => f.ReadInt32(), Times.Once());
        //    fio.Verify(f => f.ReadBytes(3), Times.Once());
        //}

        //[TestMethod]
        //public void ReadEmptyBytesTest()
        //{
        //    var fio = new Mock<IFileIO>(MockBehavior.Strict);
        //    fio.Setup(io => io.ReadInt32()).Returns(0);
        //    var r = new FormattedReader(fio.Object);

        //    var bytes = r.ReadBytes();

        //    bytes.Should().Not.Be.Null();
        //    bytes.Length.Should().Be(0);
        //    fio.Verify(f => f.ReadInt32(), Times.Once());
        //}

        [TestMethod]
        public void ReadTextTest()
        {
            const string text = "Ce plat est originaire de la Grèce antique, de l'époque de la fondation de Marseille (Massalia) au VIIe siècle av. €";
            var encoding = new UTF8Encoding(false, true);
            var bytes = encoding.GetBytes(text);
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.ReadInt32()).Returns(bytes.Length);
            fio.Setup(io => io.ReadBytes(bytes.Length)).Returns(bytes);
            var r = new FormattedReader(fio.Object);

            r.ReadText().Should().Be(text);
        }
    }
}
