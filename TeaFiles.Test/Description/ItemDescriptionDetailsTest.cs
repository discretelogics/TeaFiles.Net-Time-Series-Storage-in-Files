// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class ItemDescriptionDetailsTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void Usage()
        {
            var allButNoName = ItemDescriptionElements.All & ~ItemDescriptionElements.ItemName;
            allButNoName.HasFlag(ItemDescriptionElements.ItemName).Should().Be.False();
            allButNoName.HasFlag(ItemDescriptionElements.ItemSize).Should().Be.True();
            allButNoName.HasFlag(ItemDescriptionElements.FieldOffsets).Should().Be.True();
            allButNoName.HasFlag(ItemDescriptionElements.FieldNames).Should().Be.True();
            allButNoName.HasFlag(ItemDescriptionElements.FieldTypes).Should().Be.True();
        }
    }
}
