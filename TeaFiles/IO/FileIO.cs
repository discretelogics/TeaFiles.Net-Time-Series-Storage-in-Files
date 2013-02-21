// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;

namespace TeaTime.IO
{
    /// <summary>
    /// Exposes low level file IO methods. This class collects the whole functionality required from the file system api of an operating system or
    /// language API. Notably, modifications to the file position are not required.
    /// </summary>
    /// <remarks>This class does not own the stream passed in the ctor, so it does not implement IDisposable.<br></br></remarks>
    class FileIO : IFileIO
    {
        Stream stream;
        BinaryReader reader;
        BinaryWriter writer;

        public FileIO(Stream stream)
        {
            this.stream = stream;
        }

        BinaryReader Reader
        {
            get { return this.reader ?? (this.reader = new BinaryReader(this.stream)); }
        }

        BinaryWriter BinaryWriter
        {
            get { return this.writer ?? (this.writer = new BinaryWriter(this.stream)); }
        }

        public long Position
        {
            get { return this.stream.Position; }
        }

        /// <summary>
        /// Reads a 32bit integer value from the file.
        /// </summary>
        /// <returns>The int32 value.</returns>
        /// <remarks></remarks>
        public Int32 ReadInt32()
        {
            return this.Reader.ReadInt32();
        }

        /// <summary>
        /// Reads a 64bit integer value from the file.
        /// </summary>
        /// <returns>The int64 value.</returns>
        public Int64 ReadInt64()
        {
            return this.Reader.ReadInt64();
        }

        /// <summary>
        /// Reads a double value from the file.
        /// </summary>
        /// <returns>The double value.</returns>
        public double ReadDouble()
        {
            return this.Reader.ReadDouble();
        }

        public byte[] ReadBytes(int count)
        {
            return this.Reader.ReadBytes(count);
        }

        /// <summary>
        /// This implementation performance n times ReadByte().<br></br>
        /// Alternatively we could set the file position directly but we want to avoid usage of the 
        /// filepointer functions to demonstrate that they are not necessary. This might be important 
        /// when TeaFile APIs are written in languages that do not have a rich API is .Net. For 
        /// instance, R might not have any filepointer modification functions in its own language.
        /// </summary>
        /// <param name="n">The number of bytes to skip.</param>
        public void SkipBytes(int n)
        {
            n.Times(() => this.Reader.ReadByte());
        }

        public void WriteInt32(Int32 value)
        {
            this.BinaryWriter.Write(value);
        }

        public void WriteInt64(Int64 value)
        {
            this.BinaryWriter.Write(value);
        }

        public void WriteDouble(double value)
        {
            this.BinaryWriter.Write(value);
        }

        public void WriteBytes(byte[] bytes)
        {
            this.BinaryWriter.Write(bytes);
        }

        public void WriteZeroByte()
        {
            this.stream.WriteByte(0);
        }
    }
}
