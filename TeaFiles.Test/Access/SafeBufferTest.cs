// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TeaTime
{
    [TestClass]
    public class SafeBufferTest
    {
        // tests adopted from CppUtilsTest
        //[TestMethod]
        //unsafe public void WriteStructIntoFileTest()
        //{
        //    using (var f = new FileStream("test", FileMode.Create))
        //    {
        //        var u = new CppUtils<A>(f);
        //        A a = new A() { d = 1.7, i16 = 1, ui8 = 32 };
        //        u.Write(a);
        //        Assert.AreEqual(sizeof(A), f.Position);
        //    }
        //}

        //[TestMethod]
        //unsafe public void WriteAndReadStructIntoFileTest()
        //{
        //    using (var f = new FileStream("test", FileMode.Create))
        //    {
        //        var u = new CppUtils<A>(f);
        //        A a = new A() { d = 1.7, i16 = 1, ui8 = 32 };
        //        u.Write(a);
        //        f.Position = 0; // rewind
        //        A aa = u.Read();
        //        Assert.AreEqual(aa, a);
        //    }
        //}
    }
}
