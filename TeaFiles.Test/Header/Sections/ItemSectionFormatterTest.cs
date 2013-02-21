// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Header.Sections
{
    [TestClass]
    public class ItemSectionFormatterTest
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
            ISectionFormatter f = new ItemSectionFormatter();
            f.Id.Should().Be(0x0a);
        }

        public struct OHLCV
        {
            public double Open;
            public double High;
            public double Low;
            public double Close;
            public long Volume;
        }

        [TestMethod]
        public void ItemSectionRoundTrip()
        {
            var ms = new MemoryStream();
            var fio = new FileIO(ms);
            var fw = new FormattedWriter(fio);
            var wc = new WriteContext(fw);
            wc.Description = new TeaFileDescription();
            var writeID =
                wc.Description.ItemDescription = ItemDescription.FromAnalysis<Event<OHLCV>>();
            ISectionFormatter f = new ItemSectionFormatter();
            f.Write(wc);

            ms.Position = 0;

            var fr = new FormattedReader(fio);
            var rc = new ReadContext(fr);
            rc.Description.Should().Not.Be.Null();
            f.Read(rc);
            rc.Description.Should().Not.Be.Null();
            var id = rc.Description.ItemDescription;

            id.ItemTypeName.Should().Be(typeof (Event<OHLCV>).GetLanguageName());
            id.ItemSize.Should().Be(wc.Description.ItemDescription.ItemSize);
            id.Fields.Select(ff => ff.Name).Should().Have.SameValuesAs("Time", "Open", "High", "Low", "Close", "Volume");
            id.Fields.Select(ff => ff.Index).Should().Have.SameValuesAs(0, 1, 2, 3, 4, 5);
            id.Fields.Select(ff => ff.FieldType).Should().Have.SameValuesAs(FieldType.Int64, FieldType.Double, FieldType.Double, FieldType.Double, FieldType.Double);
            id.Fields.Select(ff => ff.Offset).Should().Have.SameValuesAs(writeID.Fields.Select(ff => ff.Offset));

            ms.Position.Should().Be(ms.Length); // very important, all bytes must have been read
        }
    }
}
