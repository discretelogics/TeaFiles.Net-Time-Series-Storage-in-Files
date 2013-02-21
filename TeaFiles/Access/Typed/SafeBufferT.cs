// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TeaTime
{
    unsafe class SafeBuffer<T> : SafeBuffer where T : struct
    {
        public SafeBuffer(Stream stream) : base(true)
        {
            this.stream = stream;

            this.Initialize<T>(1);
            this.byteLength = (int)base.ByteLength;
            this.byteBuffer = new byte[this.byteLength];
        }

        public T Read()
        {
            int read = this.stream.Read(this.byteBuffer, 0, this.byteLength);
            if (read < this.byteLength)
            {
                throw new EndOfStreamException();
            }

            fixed (byte* p = this.byteBuffer)
            {
                base.SetHandle((IntPtr)p);
                return base.Read<T>(0);
            }
        }

        public bool TryRead(out T value)
        {
            int read = this.stream.Read(this.byteBuffer, 0, this.byteLength);
            if (read < this.byteLength)
            {
                value = default(T);
                return false;
            }
            fixed (byte* p = this.byteBuffer)
            {
                base.SetHandle((IntPtr)p);
                value = base.Read<T>(0);
                return true;
            }
        }

        public void Write(T value)
        {
            fixed (byte* p = this.byteBuffer)
            {
                base.SetHandle((IntPtr)p);
                base.Write(0, value);
            }
            this.stream.Write(this.byteBuffer, 0, this.byteLength);
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        Stream stream;
        byte[] byteBuffer;
        int byteLength;
    }
}
