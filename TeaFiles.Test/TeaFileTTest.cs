// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.SampleItems;

namespace TeaTime
{
    [TestClass]
    public class TeaFileTTest
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
            Executing.This(() => TeaFile<int>.Create((string)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Create((string)null, "contento", null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Create((string)null, "a", null, true)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Create((Stream)null, "a", null, true)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Create((Stream)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Create((Stream)null, "contento", null)).Should().Throw<ArgumentNullException>();

            Executing.This(() => TeaFile<int>.OpenRead((Stream)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenRead((Stream)null, ItemDescriptionElements.None)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenRead((string)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenRead((string)null, ItemDescriptionElements.None)).Should().Throw<ArgumentNullException>();

            Executing.This(() => TeaFile<int>.OpenWrite((Stream)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenWrite((Stream)null, ItemDescriptionElements.None)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenWrite((string)null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.OpenWrite((string)null, ItemDescriptionElements.None)).Should().Throw<ArgumentNullException>();

            Executing.This(() => TeaFile<int>.Append(null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Append(null)).Should().Throw<ArgumentNullException>();
            Executing.This(() => TeaFile<int>.Append(null, ItemDescriptionElements.None)).Should().Throw<ArgumentNullException>();

            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Write(Enumerable.Range(100, 7));
            }
            stream.Position = 0;
            using (var tf = TeaFile<int>.OpenRead(stream))
            {
                tf.Items.GetEnumerator().Should().Not.Be.Null();
            }
        }

        [TestMethod]
        public void CreateAndReadTeaFile()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Description.Should().Not.Be.Null();
                tf.Description.ItemDescription.Should().Not.Be.Null();
                tf.Description.NameValues.Should().Be.Null();
                tf.Description.ContentDescription.Should().Be.Null();

                var id = tf.Description.ItemDescription;
                id.ItemTypeName.Should().Be("Int32");
                id.ItemSize.Should().Be(4);
                id.Fields.Should().Have.Count.EqualTo(1);

                var f = id.Fields.First();
                f.Name.Should().Be("m_value");
                f.FieldType.Should().Be(FieldType.Int32);
                f.Offset.Should().Be(0);
            }
            stream.Position = 0;
            using (var tf = TeaFile<int>.OpenRead(stream))
            {
                tf.Description.ItemDescription.ItemSize.Should().Be(sizeof (int));
                tf.Description.ItemDescription.ItemTypeName.Should().Be("Int32");
                tf.Description.NameValues.Should().Be.Null();
                tf.Description.ContentDescription.Should().Be.Null();

                var id = tf.Description.ItemDescription;
                id.ItemTypeName.Should().Be("Int32");
                id.ItemSize.Should().Be(4);
                id.Fields.Should().Have.Count.EqualTo(1);

                var f = id.Fields.First();
                f.Name.Should().Be("m_value");
                f.FieldType.Should().Be(FieldType.Int32);
                f.Offset.Should().Be(0);
            }
        }

        [TestMethod]
        public void FilePositionAfterCreate()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                Console.WriteLine(tf.ToString());
                Console.WriteLine(tf.Description);
                Console.WriteLine("Stream.Position:\t" + stream.Position);
                Console.WriteLine("Stream.Length:\t" + stream.Length);

                tf.ItemAreaStart.Should().Be.GreaterThan(32);
                tf.ItemAreaEnd.Should().Be.GreaterThanOrEqualTo(tf.ItemAreaStart);
                tf.ItemAreaSize.Should().Be(0);
                stream.Position.Should().Be(stream.Length);
                stream.Position.Should().Be(tf.ItemAreaStart);
            }
        }

        [TestMethod]
        public void FilePositionAfterWritingItems()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.ItemAreaSize.Should().Be(0);
                stream.Position.Should().Be(stream.Length);
                stream.Position.Should().Be(tf.ItemAreaStart);
                tf.ItemAreaStart.Should().Be(80);

                tf.Write(11);

                tf.ItemAreaStart.Should().Be(80);
                tf.ItemAreaEnd.Should().Be(84);
                tf.ItemAreaSize.Should().Be(4);
                tf.Count.Should().Be(1);
                stream.Position.Should().Be(84);
                stream.Position.Should().Be(84);

                tf.Write(7);

                tf.ItemAreaStart.Should().Be(80);
                tf.ItemAreaEnd.Should().Be(88);
                tf.ItemAreaSize.Should().Be(8);
                tf.Count.Should().Be(2);
                stream.Position.Should().Be(88);
                stream.Position.Should().Be(88);
            }
        }

        [TestMethod]
        public void TeaFileDoesNotDisposeExternalStream()
        {
            var stream = new TestStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
            }
            stream.WasDisposed.Should().Be.False();
        }

        [TestMethod]
        public void TestStreamTest()
        {
            TestStream stream;
            using (stream = new TestStream())
            using (var tf = TeaFile<int>.Create(stream))
            {
            }
            stream.WasDisposed.Should().Be.True();
        }

        [TestMethod]
        public void CreatedFileIsDisposed()
        {
            const string filename = "TeaFileTTest_CreatedFileIsDisposed.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                Executing.This(() => File.Move(filename, filename + "moved")).Should().Throw<IOException>();
            }
            File.Move(filename, filename + "moved"); // the file is movable, so it was disposed.

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [TestMethod]
        public void OpenReadOfNonExistentFile()
        {
            const string filename = "TeaFileTTest_OpenReadOfNonExistentFile.tea";
            Executing.This(() => TeaFile<int>.OpenRead(filename)).Should().Throw();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [TestMethod]
        public void CreateAndOpenReadOfFile()
        {
            const string filename = "TeaFileTTest_CreateAndOpenReadOfFile.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
            }
            TestUtils.IsLocked(filename).Should().Be.False();
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
            }
            TestUtils.IsLocked(filename).Should().Be.False();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [TestMethod]
        public void FinalizerClosesNonDisposedFile()
        {
            const string filename = "TeaFileTTest_FinalizerClosesNonDisposedFile.tea";

            TeaFile<int>.Create(filename);
            TestUtils.IsLocked(filename).Should().Be.True();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            TestUtils.IsLocked(filename).Should().Be.False();
        }

        [TestMethod]
        public void NameProperty()
        {
            var filename = "TeaFileTTest_NameProperty.tea";
            filename = Path.GetFullPath(filename);

            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.Name.Should().Be(filename);
            }
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                Executing.This(() => tf.Name.SafeToString()).Should().Throw<InvalidOperationException>();
            }
        }

        [TestMethod]
        public void OpenWriteOnNonExistingFile()
        {
            var filename = "TeaFileTTest_OpenWriteOnNonExistingFile.tea";
            Executing.This(() => TeaFile<int>.OpenWrite(filename)).Should().Throw();
        }

        [TestMethod]
        public void OpenWrite()
        {
            var filename = "TeaFileTTest_OpenWrite.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.Count.Should().Be(0);
                tf.Write(Enumerable.Range(1, 10));
                tf.Count.Should().Be(10);
            }
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
                tf.Items.ForEach(Console.WriteLine);
            }
            using (var tf = TeaFile<int>.OpenWrite(filename))
            {
                tf.Count.Should().Be(10);
                tf.Write(Enumerable.Range(1, 5));
                tf.Count.Should().Be(10);
                tf.Write(Enumerable.Range(1, 5));
                tf.Count.Should().Be(10);
                tf.Write(Enumerable.Range(1, 5));
                tf.Count.Should().Be(15);
            }
            Console.WriteLine("..................");
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
                tf.Items.ForEach(Console.WriteLine);
            }
        }

        [TestMethod]
        public void Flush()
        {
            var filename = "TeaFileTTest_Flush.tea";
            var tf = TeaFile<int>.Create(filename);
            tf.Write(Enumerable.Range(1, 10));

            tf.Flush(); // we can easily test only that the call does not except
            tf.Close();
            Executing.This(tf.Flush).Should().Throw(); // and that it fails after closing the stream
        }

        [TestMethod]
        public void FlushIsCalled()
        {
            var stream = new TestStream();
            var tf = TeaFile<int>.Create(stream);
            tf.Write(Enumerable.Range(1, 10));
            stream.FlushWasCalled = false; // the header was flushed already, so we reset the flag here

            stream.FlushWasCalled.Should().Be.False();
            tf.Flush(); // we can easily test only that the call does not except
            stream.FlushWasCalled.Should().Be.True();

            tf.Close();
        }

        //[TestMethod]
        //public void AppendItemArea()
        //{
        //    const string filename = "TeaFileTTest_AppendItemArea.tea";
        //    using (var tf = TeaFile<int>.Create(filename))
        //    {
        //        tf.Count.Should().Be(0);
        //        tf.Write(Enumerable.Range(1, 10));
        //        tf.Count.Should().Be(10);
        //    }
        //    using (var tf1 = TeaFile<int>.OpenRead(filename, ItemDescriptionElements.None))
        //    using (var tf2 = TeaFile<int>.Append(filename))
        //    {
        //        tf2.ItemAreaStart.Should().Be(tf1.ItemAreaStart);
        //        tf2.ItemAreaEnd.Should().Be(tf1.ItemAreaEnd);
        //        tf2.ItemAreaSize.Should().Be(tf1.ItemAreaSize);
        //    }
        //}

        [TestMethod]
        public void Append()
        {
            var filename = "TeaFileTTest_Append.tea";
            long fileLength0;
            FileInfo fi = new FileInfo(filename);
            using (var tf = TeaFile<int>.Create(filename))
            {
                fi.Refresh();
                fileLength0 = fi.Length;
                tf.Count.Should().Be(0);

                tf.Write(5);
                tf.Flush();

                fi.Refresh();
                tf.Count.Should().Be(1);
                fi.Length.Should().Be(fileLength0 + 4);
            }
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
                Console.WriteLine(tf);
                fi.Refresh();
                fi.Length.Should().Be(fileLength0 + 4);
                tf.Count.Should().Be(1);
            }
            using (var tf = TeaFile<int>.Append(filename))
            {
                fi.Refresh();
                fi.Length.Should().Be(fileLength0 + 4);
                tf.Count.Should().Be(1);
                tf.Write(711);
                tf.Flush();
                fi.Refresh();
                fi.Length.Should().Be(fileLength0 + 8);
                tf.Count.Should().Be(2);
            }
        }

        [TestMethod]
        public void OpenWriteStream()
        {
            Stream stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Write(Enumerable.Range(1, 10));
            }
            stream.Position = 0;
            using (var tf = TeaFile<int>.OpenWrite(stream))
            {
                tf.Write(Enumerable.Range(70, 5));
            }
            stream.Position = 0;
            using (var tf = TeaFile<int>.OpenRead(stream))
            {
                tf.Count.Should().Be(10);

                tf.Read().Should().Be(70);
                tf.Read().Should().Be(71);
                tf.Read().Should().Be(72);
                tf.Read().Should().Be(73);
                tf.Read().Should().Be(74);
                tf.Read().Should().Be(6);
                tf.Read().Should().Be(7);
                tf.Read().Should().Be(8);
                tf.Read().Should().Be(9);
                tf.Read().Should().Be(10);
            }
        }

        [TestMethod]
        public void AppendStreamTest()
        {
            const string filename = "AppendStreamTest";
            using (var tf = TeaFile<int>.Create(filename))
            {
                tf.Write(Enumerable.Range(1, 10));
            }
            using (var tf = TeaFile<int>.Append(filename))
            {
                tf.Write(Enumerable.Range(70, 5));
            }
            using (var tf = TeaFile<int>.OpenRead(filename))
            {
                tf.Count.Should().Be(15);
                tf.Items.Should().Have.SameSequenceAs(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 70, 71, 72, 73, 74);
            }
        }

        //[TestMethod]
        //public void AppendToPreallocatedFileFails()
        //{
        //    const string filename = "AppendToPreallocatedFileFails";
        //    var tf = TeaFile<int>.Create(filename);
        //    tf.Write(72);
        //    tf.Close(); // does actually nothing, because tf does not own the stream

        //    // this api does not offer preallocation, so we modify the stream manually
        //    stream.Position = 8 + 8;
        //    BinaryWriter bw = new BinaryWriter(stream);
        //    bw.Write(tf.ItemAreaEnd);
        //    100.Times(() => stream.WriteByte(0));

        //    stream.Position = 0;

        //    var tf2 = TeaFile<int>.OpenRead(stream);
        //    tf2.ItemAreaEnd.Should().Be.LessThan(stream.Length); // the file has preallocation

        //    stream.Position = 0;
        //    Executing.This(() => TeaFile<int>.Append(stream)).Should().Throw<IOException>();
        //}

        [TestMethod]
        public void DiscDriveFailureDuringCreate()
        {
            var stream = new TestStream();
            stream.FailAfterPosition = 17;
            Executing.This(() => TeaFile<int>.Create(stream)).Should().Throw<IOException>();
        }

        [TestMethod]
        public void DiscDriveFailureDuringHeaderReading()
        {
            var stream = new TestStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Write(111);
            }
            stream.Position = 0;
            stream.FailAfterPosition = 17;
            Executing.This(() => TeaFile<int>.OpenRead(stream)).Should().Throw<FileFormatException>()
                .Exception.InnerException.Should().Be.OfType<IOException>();
        }

        [TestMethod]
        public void DiscDriveFailureDuringOpenWrite()
        {
            var stream = new TestStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                tf.Write(111);
            }
            stream.Position = 0;
            stream.FailAfterPosition = 17;
            Executing.This(() => TeaFile<int>.OpenWrite(stream)).Should().Throw<FileFormatException>()
                .Exception.InnerException.Should().Be.OfType<IOException>();
        }

        [TestMethod]
        public void Truncate()
        {
            var filename = "TeaFileTTest_Truncate.tea";
            using (var tf = TeaFile<int>.Create(filename))
            {
                var fi = new FileInfo(filename);
                var emptyFileLength = fi.Length;
                tf.Count.Should().Be(0);
                tf.Write(7);
                tf.Write(8);
                tf.Write(9);
                tf.Flush();

                fi.Refresh();
                fi.Length.Should().Be(emptyFileLength + 3 * 4);
                tf.Count.Should().Be(3);

                tf.Truncate();
                fi.Refresh();
                fi.Length.Should().Be(emptyFileLength);
                tf.Count.Should().Be(0);
            }
        }

        [TestMethod]
        public void ItemCollection()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<int>.Create(stream))
            {
                for (int i = 0; i < 8; i++)
                {
                    tf.Write(i * 100);
                }
            }
            stream.Position = 0;
            using (var tf = TeaFile<int>.OpenRead(stream))
            {
                tf.Items[3].Should().Be(300);
                tf.Items[7].Should().Be(700);
                tf.Items[0].Should().Be(0);
                tf.Items[3].Should().Be(300);
                tf.Items[7].Should().Be(700);
                tf.Items[0].Should().Be(0);
                tf.Count.Should().Be(8);
            }
        }

        [TestMethod]
        public void EndOfStreamException()
        {
            var stream = new TestStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
                Time t = new Time(2000, 1, 1);
                tf.Write(Enumerable.Range(1, 5).Select(i => new Event<int> {Time = t.AddDays(i), Value = i}));
            }
            stream.Position = 0;
            stream.StopReadAfterPosition = stream.Length - 2;
            using (var tf = TeaFile<Event<int>>.OpenRead(stream))
            {
                tf.Count.Should().Be(5); // the count is still 5, the stream length has not changed
                tf.Read().Value.Should().Be(1);
                tf.Read().Value.Should().Be(2);
                tf.Read().Value.Should().Be(3);
                tf.Read().Value.Should().Be(4);
                Executing.This(() => tf.Read()).Should().Throw<EndOfStreamException>();
            }
        }

        [TestMethod]
        public void IOException()
        {
            var stream = new TestStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
                Time t = new Time(2000, 1, 1);
                tf.Write(Enumerable.Range(1, 5).Select(i => new Event<int> {Time = t.AddDays(i), Value = i}));
            }
            stream.Position = 0;
            stream.FailAfterPosition = stream.Length - 2;
            using (var tf = TeaFile<Event<int>>.OpenRead(stream))
            {
                tf.Count.Should().Be(5); // the count is still 5, the stream length has not changed
                tf.Read().Value.Should().Be(1);
                tf.Read().Value.Should().Be(2);
                tf.Read().Value.Should().Be(3);
                tf.Read().Value.Should().Be(4);
                Executing.This(() => tf.Read()).Should().Throw<IOException>();
            }
        }

        [TestMethod]
        public void FieldNames()
        {
            var filename = "TeaFileTTest_FieldNames.tea";
            using (var tf = TeaFile<Event<OHLCV>>.Create(filename))
            {
                var id = tf.Description.ItemDescription;
                id.Fields.Count.Should().Be(6);
                id.Fields.Select(f => f.Name).Should().Have.SameSequenceAs("Time", "Open", "High", "Low", "Close", "Volume");
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                var id = tf.Description.ItemDescription;
                id.Fields.Select(f => f.Name).Should().Have.SameSequenceAs("Time", "Open", "High", "Low", "Close", "Volume");
            }
        }

        [TestMethod]
        public void CreateZeroTeaFile()
        {
            var ms = new MemoryStream();
            var tf = TeaFile<Event<OHLCV>>.Create(ms, null, null, false);
            tf.Dispose();
            ms.Length.Should().Be(32);
            ms.Position = 0;
            var r = new BinaryReader(ms);
            var bom = r.ReadInt64();
            bom.Should().Be(0x0d0e0a0402080500);
            var itemstart = r.ReadInt64();
            itemstart.Should().Be(32);
            var eof = r.ReadInt64();
            eof.Should().Be(0);
            var sectionCount = r.ReadInt64();
            sectionCount.Should().Be(0);

            ms.Position = 0;
            var tf2 = TeaFile.OpenRead(ms);
            var d = tf2.Description;
            d.ItemDescription.Should().Be.Null();
            d.ContentDescription.Should().Be.Null();
            d.NameValues.Should().Be.Null();
        }

        [TestMethod]
        public void CreateFileWithContentAndNameValueSectionButNoItemDescription()
        {
            var ms = new MemoryStream();
            var nvs = new NameValueCollection();
            nvs.Add("name1", 11);
            nvs.Add("name2", "value2");
            var tf = TeaFile<Event<OHLCV>>.Create(ms, "this is the content", nvs, false);
            ms.Position = 0;
            var tf2 = TeaFile.OpenRead(ms);
            var d = tf2.Description;
            d.Should().Not.Be.Null();
            d.ItemDescription.Should().Be.Null();
            d.ContentDescription.Should().Be("this is the content");
            d.NameValues.Should().Not.Be.Null();
            d.NameValues.Should().Have.Count.EqualTo(2);
            d.NameValues.GetValue<int>("name1").Should().Be(11);
            d.NameValues.GetValue<string>("name2").Should().Be("value2");
        }
        
        [TestMethod]
        public void ItemThatHasNoTimeAttributeCanBeUsedForTeaFile()
        {
            var filename = "TeaFileTTest_ItemThatHasNoTimeAttributeCanBeUsedForTeaFile.tea";
            File.Delete(filename);
            using (var tf = TeaFile<Tick>.Create(filename))
            {
                for (int i = 0; i < 3; i++)
                {
                    tf.Write(new Tick {Time = DateTime.Now, Price = i, Volume = 10 * i});
                }
            }
            using (var tf = TeaFile.OpenRead(filename))
            {
                foreach (var item in tf.Items)
                {
                    var s = tf.Description.ItemDescription.GetValueString(item);
                    Console.WriteLine(s);
                }
            }
        }

        [TestMethod]
        public void TimeScaleIsSetInDescriptionOfCreatedFile()
        {
            var filename = "TeaFileTTest_TimeScaleIsSetInDescriptionOfCreatedFile.tea";
            File.Delete(filename);
            using (var tf = TeaFile<Tick>.Create(filename))
            {
                tf.Description.Timescale.HasValue.Should().Be.True();
                tf.Description.Timescale.Value.Epoch.Should().Be(Time.Scale.Epoch);
                tf.Description.Timescale.Value.TicksPerDay.Should().Be(Time.Scale.TicksPerDay);
            }
        }

        [TestMethod]
        public void AccessorTest()
        {
            var filename = "TeaFileTTest_AccessorTest.tea";
            using (TeaFile<Tick>.Create(filename))
            {
                //  an empty file holding the description is enough for this test
            }
            Executing.This(() => TeaFile<OHLCV>.OpenRead(filename)).Should().Throw<TypeMismatchException>();
        }

        [TestMethod]
        public void AccessorRawMemoryMappingTest()
        {
            var filename = "TeaFileTTest_AccessorRawMemoryMappingTest.tea";
            using (TeaFile<Tick>.Create(filename))
            {
                //  an empty file holding the description is enough for this test
            }
            Executing.This(() => TeaFile<OHLCV>.OpenRawMemoryMapping(filename)).Should().Throw<TypeMismatchException>();
        }
    }
}
