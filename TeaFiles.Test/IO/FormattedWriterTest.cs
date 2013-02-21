// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace TeaTime.IO
{
    [TestClass]
    public class FormattingWriterTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void CtorTest()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            new FormattedWriter(fio.Object);
        }

        [TestMethod]
        public void WriteInt32Test()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteInt32(1743));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteInt32(1743);
            fio.Verify(f => f.WriteInt32(1743), Times.Once());
        }

        [TestMethod]
        public void WriteInt64Test()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteInt64(0x7fffeeeebbbbaaaa));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteInt64(0x7fffeeeebbbbaaaa);
            fio.Verify(f => f.WriteInt64(0x7fffeeeebbbbaaaa), Times.Once());
        }

        [TestMethod]
        public void WriteDoubleTest()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteDouble(1.2345));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteDouble(1.2345);
            fio.Verify(f => f.WriteDouble(1.2345), Times.Once());
        }

        [TestMethod]
        public void WriteGuidTest()
        {
            Guid g = Guid.NewGuid(); // not strictly deterministic but deterministic enough
            var guidBytes = g.ToByteArray();
            var fio = new Mock<IFileIO>(MockBehavior.Default);
            fio.Setup(io => io.WriteBytes(It.IsAny<byte[]>()));

            var fw = new FormattedWriter(fio.Object);
            fw.WriteGuid(g);

            fio.Verify(f => f.WriteBytes(It.IsAny<byte[]>()), Times.Once());
        }

        [TestMethod]
        public void WriteRawTest()
        {
            byte[] bytes = new byte[] {1, 3, 7, 8, 9};
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteBytes(It.IsAny<byte[]>()));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteRaw(bytes);
            fio.Verify(f => f.WriteBytes(It.Is<byte[]>(bs => bs.SequenceEqual(bytes))), Times.Once());
        }

        [TestMethod]
        public void WriteRawArgumentTest()
        {
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            FormattedWriter fw = new FormattedWriter(fio.Object);
            Executing.This(() => fw.WriteRaw(null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WriteTextTest()
        {
            const string testText =
                "Traditionnellement, il est conseillé d'accompagner cette soupe et ses poissons soit d'un vin rosé issu du vignoble de Provence, soit d'un vin rouge. €";
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteInt32(It.IsAny<int>()));
            fio.Setup(io => io.WriteBytes(It.IsAny<byte[]>()));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteText(testText);
            fio.Verify(f => f.WriteInt32(It.Is<int>(n => n >= testText.Length)), Times.Once());
            fio.Verify(f => f.WriteBytes(It.Is<byte[]>(bytes => (new UTF8Encoding(false, true).GetString(bytes)) == testText)), Times.Once());
        }

        [TestMethod]
        public void WriteEmptyTextTest()
        {
            const string testText = "";
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteInt32(It.IsAny<int>()));
            fio.Setup(io => io.WriteBytes(It.IsAny<byte[]>()));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteText(testText);
            fio.Verify(f => f.WriteInt32(0), Times.Once());
        }

        [TestMethod]
        public void WriteNullTextTest()
        {
            const string testText = null;
            var fio = new Mock<IFileIO>(MockBehavior.Strict);
            fio.Setup(io => io.WriteInt32(It.IsAny<int>()));
            fio.Setup(io => io.WriteBytes(It.IsAny<byte[]>()));
            FormattedWriter fw = new FormattedWriter(fio.Object);
            fw.WriteText(testText);
            fio.Verify(f => f.WriteInt32(0), Times.Once());
        }

        [TestMethod]
        public void NameValueRoundTripTest()
        {
            var stream = new MemoryStream();
            var fio = new FileIO(stream);
            var w = new FormattedWriter(fio);

            Guid g = new Guid("1116C195-8975-4E73-B777-23E1C548BC71");
            const string testText = "some text with special characters €,@, ʤǤǄƪҗ∰";
            {
                var nv1 = new NameValue("name1", 1.23);
                var nv2 = new NameValue("name2", 722);
                var nv3 = new NameValue("name3", g);
                var nv4 = new NameValue("name4", testText);

                w.WriteNameValue(nv1);
                w.WriteNameValue(nv2);
                w.WriteNameValue(nv3);
                w.WriteNameValue(nv4);
            }

            stream.Position = 0;

            var r = new FormattedReader(fio);
            {
                var nv1 = r.ReadNameValue();
                nv1.Name.Should().Be("name1");
                nv1.GetValue<double>().Should().Be(1.23);
                nv1.Kind.Should().Be(NameValue.ValueKind.Double);

                var nv2 = r.ReadNameValue();
                nv2.Name.Should().Be("name2");
                nv2.GetValue<int>().Should().Be(722);
                nv2.Kind.Should().Be(NameValue.ValueKind.Int32);

                var nv3 = r.ReadNameValue();
                nv3.Name.Should().Be("name3");
                nv3.GetValue<Guid>().Should().Be(g);
                nv3.Kind.Should().Be(NameValue.ValueKind.Guid);

                var nv4 = r.ReadNameValue();
                nv4.Name.Should().Be("name4");
                nv4.GetValue<string>().Should().Be(testText);
                nv4.Kind.Should().Be(NameValue.ValueKind.Text);
            }
        }
    }
}
