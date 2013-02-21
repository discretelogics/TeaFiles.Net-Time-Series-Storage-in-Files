// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.SampleItems;

namespace TeaTime
{
    [TestClass]
    public class FieldTest
    {
        [TestMethod]
        public void ExoticCoverage()
        {
            var f = new Field();
            Executing.This(() => f.GetValue(null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetValueTest()
        {
            var filename = "FieldTest_GetValueTest.tea";
            using (var tf = TeaFile<OHLCV>.Create(filename))
            {
                tf.Write(Enumerable.Range(1, 10).Select(i => new OHLCV {Close = i * 101}));
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                var id = tf.Description.ItemDescription;
                var field = id.Fields.First(ff => ff.Name == "Close");

                Item item = tf.Items.First();
                field.GetValue(item).Should().Be(101.0);

                item = tf.Items[3];
                field.GetValue(item).Should().Be(404.0);
            }
        }
    }
}
