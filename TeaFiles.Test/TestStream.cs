// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using System.Linq;

namespace TeaTime
{
    class TestStream : MemoryStream
    {
        public bool FlushWasCalled { get; set; }
        public bool WasDisposed { get; set; }
        public long FailAfterPosition { get; set; }
        public long StopReadAfterPosition { get; set; }

        public TestStream()
        {
            this.FailAfterPosition = long.MaxValue;
            this.StopReadAfterPosition = long.MaxValue;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Console.WriteLine("[Write({0}]    o={1}, n={2})".Formatted(buffer.Take(count).Joined(","), offset, count));
            base.Write(buffer, offset, count);
            if (this.Position >= this.FailAfterPosition)
            {
                throw new IOException("lets say the hard disc failed.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.Position + count > this.StopReadAfterPosition)
            {
                count = (int)(this.StopReadAfterPosition - this.Position);
                if (count < 0) count = 0;
            }
            var n = base.Read(buffer, offset, count);
            if (this.Position >= this.FailAfterPosition)
            {
                throw new IOException("lets say the hard disc failed.");
            }
            return n;
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine("Dispose");
            this.WasDisposed = true;
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            Console.WriteLine("Flush");
            this.FlushWasCalled = true;
            base.Flush();
        }
    }
}
