// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class NameValueTest
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
            Executing.This(() => new NameValue("astring", null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => new NameValue(null, 1)).Should().Throw<ArgumentNullException>();
            Executing.This(() => new NameValue(null, 1.23)).Should().Throw<ArgumentNullException>();
            Executing.This(() => new NameValue(null, "duffy duck")).Should().Throw<ArgumentNullException>();
            Executing.This(() => new NameValue(null, Guid.Empty)).Should().Throw<ArgumentNullException>();

            var nvc = new NameValueCollection();
            IEnumerable enumerable = nvc;
            enumerable.GetEnumerator().Should().Not.Be.Null();
        }

        [TestMethod]
        public void NameValueCollectionAdd()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("anint", 17);
            nvc.Add("adouble", 1.234);
            nvc.Add("astring", "bugs bunny");
            nvc.Add("aguid", Guid.Empty);

            nvc.Count.Should().Be(4);

            nvc.GetValue<int>("anint").Should().Be(17);
            nvc.GetValue<double>("adouble").Should().Be(1.234);
            nvc.GetValue<string>("astring").Should().Be("bugs bunny");
            nvc.GetValue<Guid>("aguid").Should().Be(Guid.Empty);

            int i = 1;
            10.Times(() => nvc.Add("x" + i, i++));

            nvc.ToString().Should().Contain("anint");
            nvc.ToString().Should().Contain("17");
            nvc.ToString().Should().Contain("adouble");
            nvc.ToString().Should().Contain("bugs bunny");
            nvc.ToString().Should().Contain("aguid");
            nvc.ToString().Should().Contain("...");
        }

        [TestMethod]
        public void ValueTextTest()
        {
            string s = new NameValue("name", 1.23).ValueText;
            (s == "1.23" || s == "1,23").Should().Be.True(); // comma will be either . or ,

            new NameValue("name", "bugs bunny").ValueText.Should().Be("bugs bunny");
            new NameValue("name", Guid.Empty).ValueText.Should().Be(Guid.Empty.ToString());
        }

        [TestMethod]
        public void NameValueCollectionFactoryTest()
        {
            var nvc = NameValueCollection.From("aname", 42, "bname", Guid.Empty, "cname", "cvalue", "dname", 12.34);
            nvc.Count.Should().Be(4);
            nvc.Select(nv => nv.Name).Should().Have.SameSequenceAs("aname", "bname", "cname", "dname");
            nvc.Select(nv => nv.Kind).Should().Have.SameSequenceAs(NameValue.ValueKind.Int32, NameValue.ValueKind.Guid, NameValue.ValueKind.Text, NameValue.ValueKind.Double);
            nvc.Select(nv => nv.ValueText).Should().Have.SameSequenceAs("42", Guid.Empty.ToString(), "cvalue", 12.34.ToString());
        }

        [TestMethod]
        public void NameValueCollectionFactoryExceptions()
        {
            Executing.This(() => NameValueCollection.From(111, 42)).Should().Throw<ArgumentException>();
            Executing.This(() => NameValueCollection.From(111, 42, 11)).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void NameValueShouldNeverHaveNullValueTest()
        {
            Executing.This(() => new NameValue("name1", null)).Should().Throw();
        }
    }
}
