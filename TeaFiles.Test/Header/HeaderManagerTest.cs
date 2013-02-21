// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Header
{
    [TestClass]
    public class HeaderManagerTest
    {
        [TestInitialize]
        public void Init()
        {
            // get english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        //[TestCleanup]
        //public void TestTearDown()
        //{
        //    HeaderManager.Instance.Initialized();            
        //}

        [TestMethod]
        public void WriteCoreHeader()
        {
            var ms = new MemoryStream();
            HeaderManager.Instance.WriteHeader(new FormattedWriter(new FileIO(ms)), null);
        }

        [TestMethod]
        public void CreateSectionsTest()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            wc.ItemAreaStart.Should().Be(0); //  not set yet
            HeaderManager.Instance.CreateSections(wc);
            wc.ItemAreaStart.Should().Be(32);
        }

        [TestMethod]
        public void CoreHeaderRoundTrip()
        {
            var ms = new MemoryStream();
            HeaderManager.Instance.WriteHeader(new FormattedWriter(new FileIO(ms)), null);
            ms.Length.Should().Be(4 * 8);
            ms.Position = 0;
            var rc = HeaderManager.Instance.ReadHeader(new FormattedReader(new FileIO(ms)));
            rc.Should().Not.Be.Null();
            rc.ItemAreaStart.Should().Be(32);
            rc.ItemAreaEnd.Should().Be(0);
            rc.SectionCount.Should().Be(0);
            ms.Position.Should().Be(ms.Length);
        }

        [TestMethod]
        public void HeaderHasLengthOfItemStart()
        {
            var ms = new MemoryStream();
            var desc = new TeaFileDescription();
            desc.ContentDescription = "a";
            var wc = HeaderManager.Instance.WriteHeader(new FormattedWriter(new FileIO(ms)), desc);
            ms.Length.Should().Be(wc.ItemAreaStart);
            ms.Position.Should().Be(wc.ItemAreaStart);
            (ms.Position % 8).Should().Be(0);
        }

        [TestMethod]
        public void HeaderWithContentDescription()
        {
            var ms = new MemoryStream();
            var desc = new TeaFileDescription();
            desc.ContentDescription = "a";
            var wc = HeaderManager.Instance.WriteHeader(new FormattedWriter(new FileIO(ms)), desc);
            ms.Length.Should().Be.GreaterThan(4 * 8);
            ms.Position.Should().Be(wc.ItemAreaStart);
            (ms.Position % 8).Should().Be(0);
            ms.Position = 0;
            var rc = HeaderManager.Instance.ReadHeader(new FormattedReader(new FileIO(ms)));
            rc.Should().Not.Be.Null();
            rc.ItemAreaStart.Should().Be.GreaterThan(32);
            rc.ItemAreaEnd.Should().Be(0);
            rc.SectionCount.Should().Be(1);
            ms.Position.Should().Be(ms.Length);
            ms.Position.Should().Be(rc.ItemAreaStart);
            (ms.Position % 8).Should().Be(0);
        }

        [TestMethod]
        public void SignatureError()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(123L);
            ms.Position = 0;

            Executing.This(() => HeaderManager.Instance.ReadHeader(new FormattedReader(new FileIO(ms)))).Should().Throw<FileFormatException>()
                .Exception.Message.Should().Contain("Expected Signature not found");
        }

        [TestMethod]
        public void NextSectionOffsetWrong()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(0x0d0e0a0402080500);
            bw.Write(32L);
            bw.Write(0L);
            bw.Write(1L); // 1 section
            bw.Write(0x80);
            bw.Write(11111);
            ms.Position = 0;

            Executing.This(() => HeaderManager.Instance.ReadHeader(new FormattedReader(new FileIO(ms)))).Should().Throw<FileFormatException>()
                .Exception.Message.Should().Contain("NextSectionOffset of section");
        }

        [TestMethod]
        public void BadSection()
        {
            var badSectionFormatter = new Mock<ISectionFormatter>(MockBehavior.Strict);
            badSectionFormatter.Setup(sf => sf.Id).Returns(0x1000);
            badSectionFormatter.Setup(sf => sf.Read(It.IsAny<ReadContext>())).Callback((ReadContext rc) => rc.Reader.SkipBytes(16));
            HeaderManager hm = new HeaderManager();
            hm.AddSectionFormatter(badSectionFormatter.Object);

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(0x0d0e0a0402080500);
            bw.Write(200L);
            bw.Write(0L);
            bw.Write(1L); // 1 section
            bw.Write(badSectionFormatter.Object.Id);
            bw.Write(2);

            20.Times(() => bw.Write(0L));

            ms.Position = 0;

            Executing.This(() => hm.ReadHeader(new FormattedReader(new FileIO(ms)))).Should().Throw<FileFormatException>()
                .Exception.Message.Should().Contain("Section read too many bytes");
        }

        [TestMethod]
        public void GeneralExceptionWhileReadingHeader()
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception("testException"));

            Executing.This(() => HeaderManager.Instance.ReadHeader(new FormattedReader(new FileIO(stream.Object)))).Should().Throw<FileFormatException>()
                .Exception.Message.Should().Contain("Error reading TeaFile Header: testException");
        }
    }
}
