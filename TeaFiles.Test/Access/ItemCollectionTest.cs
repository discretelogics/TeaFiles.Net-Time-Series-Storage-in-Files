// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class ItemCollectionTest
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
            var r = new Mock<IItemReader>(MockBehavior.Strict);
            var collection = new ItemCollection(r.Object);
            IEnumerable enumerable = collection;
            enumerable.GetEnumerator().Should().Not.Be.Null();
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            var filename = "ItemCollectionTest_GetEnumeratorTest.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.Write(Enumerable.Range(2, 4));
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                var e = tf.Items.GetEnumerator(0);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(2);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(3);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(4);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(5);
                e.MoveNext().Should().Be.False();

                e = tf.Items.GetEnumerator(2);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(4);
                e.MoveNext().Should().Be.True();
                e.Current.Values[0].Should().Be(5);
                e.MoveNext().Should().Be.False();
            }
        }
    }
}
