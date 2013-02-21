// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace TeaTime.IO
{
    [TestClass]
    public class FileIOTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void ReadInt32Test()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), 0, 4))
                .Returns((byte[] bytes, int i, int n) =>
                    {
                        BitConverter.GetBytes(0x7f665544).CopyTo(bytes, 0);
                        return 4;
                    });
            FileIO f = new FileIO(stream.Object);
            f.ReadInt32().Should().Be(0x7f665544);
        }

        [TestMethod]
        public void ReadInt64Test()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), 0, 8))
                .Returns((byte[] bytes, int i, int n) =>
                    {
                        BitConverter.GetBytes(0x7f66554433221100).CopyTo(bytes, 0);
                        return 8;
                    });
            FileIO f = new FileIO(stream.Object);
            f.ReadInt64().Should().Be(0x7f66554433221100);
        }

        [TestMethod]
        public void ReadDoubleTest()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), 0, 8))
                .Returns((byte[] bytes, int i, int n) =>
                    {
                        BitConverter.GetBytes(7.123).CopyTo(bytes, 0);
                        return 8;
                    });
            FileIO f = new FileIO(stream.Object);
            f.ReadDouble().Should().Be(7.123);
        }

        [TestMethod]
        public void ReadBytesTest()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), 0, 3))
                .Returns((byte[] bytes, int i, int n) =>
                    {
                        bytes[0] = 0x11;
                        bytes[1] = 0x22;
                        bytes[2] = 0x33;
                        return 3;
                    });
            FileIO f = new FileIO(stream.Object);
            f.ReadBytes(3).Should().Have.SameValuesAs<byte>(0x11, 0x22, 0x33);
        }

        [TestMethod]
        public void SkipBytesTest()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.ReadByte()).Returns(11); // any value, it is ignored anyway
            FileIO f = new FileIO(stream.Object);
            f.SkipBytes(3);
            stream.Verify(s => s.ReadByte(), Times.Exactly(3));
        }

        [TestMethod]
        public void WriteInt32Test()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanWrite).Returns(true); // a BinaryWriter will be created on the stream, whose ctor checks for CanWrite
            stream.Setup(s => s.Write(It.Is<byte[]>(
                bytes => bytes.Take(4).SequenceEqual(BitConverter.GetBytes(0x11223344))), 0, 4));
            FileIO f = new FileIO(stream.Object);
            f.WriteInt32(0x11223344);
            stream.Verify(s => s.Write(It.IsAny<byte[]>(), 0, 4), Times.Once());
        }

        [TestMethod]
        public void WriteInt64Test()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanWrite).Returns(true); // a BinaryWriter will be created on the stream, whose ctor checks for CanWrite
            stream.Setup(s => s.Write(It.Is<byte[]>(
                bytes => bytes.Take(8).SequenceEqual(BitConverter.GetBytes(0x1122334455667788))), 0, 8));
            FileIO f = new FileIO(stream.Object);
            f.WriteInt64(0x1122334455667788);
            stream.Verify(s => s.Write(It.IsAny<byte[]>(), 0, 8), Times.Once());
        }

        [TestMethod]
        public void WriteDoubleTest()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanWrite).Returns(true); // a BinaryWriter will be created on the stream, whose ctor checks for CanWrite
            stream.Setup(s => s.Write(It.Is<byte[]>(
                bytes => bytes.Take(8).SequenceEqual(BitConverter.GetBytes(144.123))), 0, 8));
            FileIO f = new FileIO(stream.Object);
            f.WriteDouble(144.123);
            stream.Verify(s => s.Write(It.IsAny<byte[]>(), 0, 8), Times.Once());
        }

        [TestMethod]
        public void WriteBytesTest()
        {
            byte[] sampleBytes = Enumerable.Range(0, 71).Select(n => (byte)n).ToArray();

            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanWrite).Returns(true); // a BinaryWriter will be created on the stream, whose ctor checks for CanWrite
            stream.Setup(s => s.Write(It.Is<byte[]>(
                bytes => bytes.Take(71).SequenceEqual(sampleBytes)), 0, 71));
            FileIO f = new FileIO(stream.Object);
            f.WriteBytes(sampleBytes);
            stream.Verify(s => s.Write(It.IsAny<byte[]>(), 0, 71), Times.Once());
        }

        [TestMethod]
        public void WriteZeroByteTest()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanWrite).Returns(true); // a BinaryWriter will be created on the stream, whose ctor checks for CanWrite
            stream.Setup(s => s.WriteByte(It.IsAny<byte>()));
            FileIO f = new FileIO(stream.Object);
            f.WriteZeroByte();
            stream.Verify(s => s.WriteByte(It.IsAny<byte>()), Times.Exactly(1));
        }

        [TestMethod]
        public void RoundTripTest()
        {
            var filename = "FileIOTest_RoundTripTest.tea";
            File.Create(filename).Close();
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                FileIO f = new FileIO(fs);
                fs.Position.Should().Be(0);

                f.WriteInt32(0x7f665544);
                fs.Position.Should().Be(4);

                f.WriteInt64(0x7f66554433221100);
                fs.Position.Should().Be(12);

                f.WriteZeroByte();
                fs.Position.Should().Be(13);

                f.WriteBytes(new byte[] {1, 5, 81});
                fs.Position.Should().Be(16);
            }
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                FileIO f = new FileIO(fs);
                fs.Position.Should().Be(0);

                f.ReadInt32().Should().Be(0x7f665544);
                fs.Position.Should().Be(4);

                f.ReadInt64().Should().Be(0x7f66554433221100);
                fs.Position.Should().Be(12);

                f.SkipBytes(1);
                fs.Position.Should().Be(13);

                f.ReadBytes(3).Should().Have.SameValuesAs(new byte[] {1, 5, 81});
                fs.Position.Should().Be(16);
            }
        }
    }
}
