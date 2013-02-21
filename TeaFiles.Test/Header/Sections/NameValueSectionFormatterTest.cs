// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Header.Sections
{
    [TestClass]
    public class NameValueSectionFormatterTest
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
            ISectionFormatter f = new NameValueSectionFormatter();
            Executing.This(() => f.Write(null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => f.Read(null)).Should().Throw<ArgumentNullException>();
            f.Id.Should().Be(0x81);
        }

        [TestMethod]
        public void NoNameValueSectionTest()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);

            ISectionFormatter f = new NameValueSectionFormatter();
            f.Write(wc);

            wc.Description = new TeaFileDescription();
            f.Write(wc);

            wc.Description.NameValues = new NameValueCollection();
            f.Write(wc);

            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        [TestMethod]
        public void NameValueSectionIOTest()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            fw.Setup(w => w.WriteInt32(1));
            fw.Setup(w => w.WriteNameValue(It.Is<NameValue>(nv => nv.Name == "someName")));
            var wc = new WriteContext(fw.Object);

            ISectionFormatter f = new NameValueSectionFormatter();
            wc.Description = new TeaFileDescription();
            wc.Description.NameValues = new NameValueCollection();
            wc.Description.NameValues.Add(new NameValue("someName", 1.23));
            f.Write(wc);

            fw.Verify();
        }

        [TestMethod]
        public void NameValueSectionRoundTrip1EntryTest()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);

            ISectionFormatter f = new NameValueSectionFormatter();
            wc.Description = new TeaFileDescription();
            wc.Description.NameValues = new NameValueCollection();
            wc.Description.NameValues.Add(new NameValue("someName", 1.23));
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            f.Read(rc);

            rc.Description.Should().Not.Be.Null();
            rc.Description.NameValues.Should().Not.Be.Null();
            rc.Description.NameValues.Should().Have.Count.EqualTo(1);
            rc.Description.NameValues.First().Name.Should().Be("someName");
            rc.Description.NameValues.First().GetValue<double>().Should().Be(1.23);
        }

        [TestMethod]
        public void NameValueSectionRoundTrip3EntriesTest()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);

            ISectionFormatter f = new NameValueSectionFormatter();
            wc.Description = new TeaFileDescription();
            wc.Description.NameValues = new NameValueCollection();
            wc.Description.NameValues.Add(new NameValue("someName", 1.23));
            wc.Description.NameValues.Add(new NameValue("someName2", "second value"));
            wc.Description.NameValues.Add(new NameValue("someName3", 333));
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            f.Read(rc);

            rc.Description.Should().Not.Be.Null();
            rc.Description.NameValues.Should().Not.Be.Null();
            rc.Description.NameValues.Should().Have.Count.EqualTo(3);
            rc.Description.NameValues.Select(nv => nv.Name).Should().Have.SameSequenceAs("someName", "someName2", "someName3");
            rc.Description.NameValues.Select(nv => nv.GetValue<object>()).Should().Have.SameSequenceAs(1.23, "second value", 333);
        }
    }
}
