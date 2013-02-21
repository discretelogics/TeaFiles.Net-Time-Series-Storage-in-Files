// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class MemoryMappedAccessTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void AccessItemsViaMemoryMapping()
        {
            var filename = "MemoryMappedAccessTest_AccessItemsViaMemoryMapping.tea";
            using (var f = TeaFile<Event<int>>.Create(filename))
            {
                var dt = new DateTime(2000, 1, 1);
                for (int i = 0; i < 27; i++)
                {
                    f.Write(new Event<int> {Time = dt.AddDays(i), Value = i * 10});
                }
            }
            using (var view = TeaFile<Event<int>>.OpenMemoryMapping(filename))
            {
                Event<int> e = view.Read(0);
                e.Value.Should().Be(0);
                e.Time.Should().Be(new DateTime(2000, 1, 1));

                e = view.Read(0);
                e.Value.Should().Be(0);
                e.Time.Should().Be(new DateTime(2000, 1, 1));

                e = view.Read(1);
                e.Value.Should().Be(10);
                e.Time.Should().Be(new DateTime(2000, 1, 2));

                e = view.Read(2);
                e.Value.Should().Be(20);
                e.Time.Should().Be(new DateTime(2000, 1, 3));

                var t = new DateTime(2000, 1, 1);
                for (int i = 0; i < 27; i++)
                {
                    e = view.Read(i);
                    e.Value.Should().Be(i * 10);
                    e.Time.Should().Be(t.AddDays(i));
                    e.Should().Be(view[i]);
                }
            }
        }

        [TestMethod]
        public void ReadingBeyondEntOfItemArea()
        {
            var filename = "MemoryMappedAccessTest_ReadingBeyondEntOfItemArea.tea";
            using (var f = TeaFile<Event<int>>.Create(filename))
            {
                Time t = new DateTime(2000, 1, 1);
                3.Times(() => f.Write(new Event<int> {Time = t, Value = 777}));
            }
            using (var view = TeaFile<Event<int>>.OpenMemoryMapping(filename))
            {
                view.Read(0);
                view.Read(1);
                view.Read(2);
                Executing.This(() => view.Read(3)).Should().Throw<ArgumentOutOfRangeException>();
            }
        }
    }
}
