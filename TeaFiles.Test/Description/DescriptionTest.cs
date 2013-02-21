// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class DescriptionTest
    {
        [TestMethod]
        public void DescriptionOfZeroFileIsNotNull()
        {
            var filename = "DescriptionTest_DescriptionOfZeroFileIsNotNull.tea";
            using (TeaFile<StructA>.Create(filename))
            {
            }
            using (var tf = TeaFile<StructA>.OpenRead(filename))
            {
                tf.Description.Should().Not.Be.Null();
            }
        }

        public struct StructA
        {
            public byte A;
            public double B;
        }

        [TestMethod]
        public void IsTimeSeriesFalse()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                var d = tf.Description;
                d.ItemDescription.HasEventTime.Should().Be.False();
            }
        }

        [TestMethod]
        public void IsTimeSeriesTrue()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
                var d = tf.Description;
                d.ItemDescription.HasEventTime.Should().Be.True();
            }
        }
    }
}
