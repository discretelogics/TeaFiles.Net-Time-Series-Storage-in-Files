// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.SampleItems;

namespace TeaTime.Access
{
    [TestClass]
    public class RawMemoryMappingTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public unsafe void AccessItemsViaMemoryMapping()
        {
            const string filename = "RawMemoryMappingTest_AccessItemsViaMemoryMapping.tea";
            using (var f = TeaFile<Tick>.Create(filename))
            {
                var dt = new DateTime(2000, 1, 1);
                for (int i = 0; i < 27; i++)
                {
                    f.Write(new Tick {Time = dt.AddDays(i), Volume = i * 10});
                }
            }
            using (var view = TeaFile<Tick>.OpenRawMemoryMapping(filename))
            {
                Tick* tick = (Tick*)view.ItemAreaStart;
                var dt = new DateTime(2000, 1, 1);
                for (int i = 0; i < 27; i++)
                {
                    tick->Volume.Should().Be(i * 10);
                    tick->Time.Should().Be(dt.AddDays(i));
                    tick++;
                }
            }
        }

        [TestMethod]
        public unsafe void AccessItemsViaMemoryMappingPointers()
        {
            const string filename = "AccessItemsViaMemoryMappingPointers.tea";
            using (var f = TeaFile<Tick>.Create(filename))
            {
                var dt = new DateTime(2000, 1, 1);
                for (int i = 0; i < 3; i++)
                {
                    f.Write(new Tick { Time = dt.AddDays(i), Volume = i * 10 });
                }
            }
            using (var view = TeaFile<Tick>.OpenRawMemoryMapping(filename))
            {
                Tick* tick = (Tick*)view.ItemAreaStart;
                Tick* end = (Tick*)view.ItemAreaEnd;
                ((int)tick).Should().Be.LessThanOrEqualTo((int)end);
                
                int i = 0;
                var dt = new DateTime(2000, 1, 1);
                while(tick < end)
                {
                    tick->Volume.Should().Be(i * 10);
                    tick->Time.Should().Be(dt.AddDays(i));
                    tick++;
                    i++;
                }
            }
        }
    }
}
