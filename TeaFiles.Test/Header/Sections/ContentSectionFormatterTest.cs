// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Header.Sections
{
    [TestClass]
    public class ContentSectionFormatterTest
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
            ISectionFormatter f = new ContentSectionFormatter();
            Executing.This(() => f.Write(null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => f.Read(null)).Should().Throw<ArgumentNullException>();
            f.Id.Should().Be(0x80);
        }

        [TestMethod]
        public void ContentSectionFormatterEmptyDescription()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            ISectionFormatter f = new ContentSectionFormatter();
            f.Write(wc);
            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        [TestMethod]
        public void ContentSectionFormatterDescriptionSetButNoContentDescription()
        {
            var fw = new Mock<IFormattedWriter>(MockBehavior.Strict);
            var wc = new WriteContext(fw.Object);
            wc.Description = new TeaFileDescription();
            ISectionFormatter f = new ContentSectionFormatter();
            f.Write(wc);
            // due to the strict behavior of the fw mock, this test succeeds only if no method of fw was called.
        }

        [TestMethod]
        public void ContentSectionFormatterRoundTrip()
        {
            const string testValue = "Météo pour Paris, France. @€";

            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            wc.Description.ContentDescription = testValue;
            ISectionFormatter f = new ContentSectionFormatter();
            f.Write(wc);
            ms.Position = 0;
            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            f.Read(rc);
            rc.Description.Should().Not.Be.Null();
            rc.Description.ContentDescription.Should().Be(testValue);
        }
    }
}
