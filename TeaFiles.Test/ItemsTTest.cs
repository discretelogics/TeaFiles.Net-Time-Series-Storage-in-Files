// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeaTime.SampleItems;

namespace TeaTime
{
    [TestClass]
    public class ItemTTest
    {
        [TestMethod]
        public void ItemsT_property_returns_value()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                var items = f.Items;
                Assert.IsNotNull(items);
            }
        }

        [TestMethod]
        public void ItemsT_property_has_count_0()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                var items = f.Items;
                Assert.AreEqual(0, items.Count);
            }
        }

        [TestMethod]
        public void ItemsT_property_has_count_1()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                f.Write(new OHLCV {Open = 111});
                var items = f.Items;
                Assert.AreEqual(1, items.Count);
            }
        }

        [TestMethod]
        public void ItemsT_property_has_count_4()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                4.Times(() => f.Write(new OHLCV {Open = 111}));
                var items = f.Items;
                Assert.AreEqual(4, items.Count);
            }
        }

        //[TestMethod]
        //public void ItemsT_FirstItem_and_LastItem_of_empty_file_have_struct_default_value()
        //{
        //    using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create("lab1.tea"))
        //    {
        //        var items = f.Items;
        //        Assert.AreEqual(default(OHLCV), items.First);
        //        Assert.AreEqual(default(OHLCV), items.Last);
        //    }
        //}

        //[TestMethod]
        //public void ItemsT_FirstItem_and_LastItem_have_value_of_single_value()
        //{
        //    using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create("lab1.tea"))
        //    {
        //        var firstValue = new OHLCV { Open = 111 };
        //        f.Write(firstValue);
        //        var items = f.Items;
        //        Assert.AreEqual(items.First, items.First);
        //        Assert.AreEqual(items.First, items.Last, "Since there is only one value, first and last must be the same value");
        //    }
        //}

        //[TestMethod]
        //public void ItemsT_FirstItem_and_LastItem_have_correct_values()
        //{
        //    using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create("lab1.tea"))
        //    {
        //        var firstValue = new OHLCV { Open = 111 };
        //        var someValue = new OHLCV { Open = 222 };
        //        var lastValue = new OHLCV { Open = 333 };
        //        f.Write(firstValue);
        //        f.Write(someValue);
        //        f.Write(someValue);
        //        f.Write(someValue);
        //        f.Write(lastValue);
        //        var items = f.Items;
        //        Assert.AreEqual(firstValue, items.First);
        //        Assert.AreEqual(lastValue, items.Last);
        //    }
        //}

        [TestMethod]
        public void ItemsT_Enumerating_Returns_Correct_Values()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                var firstValue = new OHLCV {Open = 111};
                var someValue = new OHLCV {Open = 222};
                var lastValue = new OHLCV {Open = 333};

                List<OHLCV> values = new List<OHLCV>();
                values.Add(firstValue);
                values.Add(someValue);
                values.Add(lastValue);

                int i = 0;
                foreach (OHLCV ohlcv in f.Items)
                {
                    Assert.AreEqual(values[i], ohlcv);
                }
            }
        }

        [TestMethod]
        public void ItemsT_Enumerating_No_Enumeration_On_Empty_Collection()
        {
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(Guid.NewGuid() + "lab1.tea"))
            {
                foreach (OHLCV ohlcv in f.Items)
                {
                    Assert.Fail("no values are available, so no iteration should occur");
                }
            }
        }

        [TestMethod]
        public void ItemsT_CountProperty_reflects_actual_number_of_items()
        {
            string filename = Guid.NewGuid() + "lab1.tea";
            using (TeaFile<OHLCV> f = TeaFile<OHLCV>.Create(filename))
            {
                Assert.AreEqual(0, f.Items.Count);
                f.Write(new OHLCV {Open = 111});
                Assert.AreEqual(1, f.Items.Count, "After writing an item, Count is 1");
            }

            using (var f = TeaFile<OHLCV>.OpenRead(filename))
            {
                Assert.AreEqual(1, f.Items.Count);
            }
        }
    }
}
