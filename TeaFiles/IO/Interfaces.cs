// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime.IO
{
    /// <summary>
    /// This interface describes the low level file I/O methods required to read
    /// and write a TeaFile header.<br/>
    /// The TeaFile header are limited to the 5 types below, namely
    /// <list type="bullets">
    /// <item>int 32</item>
    /// <item>int 64</item>
    /// <item>double</item>
    /// <item>guid (a sequence of 16 bytes)</item>
    /// <item>byte arrays (used for strings)</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// This interface is internal instead of private to allow its mocking.
    /// </remarks>
    interface IFileIO
    {
        /// <summary>
        /// Reads a 32bit integer value from the file.
        /// </summary>
        /// <returns>The int32 value.</returns>
        Int32 ReadInt32();

        /// <summary>
        /// Reads a 64bit integer value from the file.
        /// </summary>
        /// <returns>The int64 value.</returns>
        Int64 ReadInt64();

        /// <summary>
        /// Reads a double value from the file.
        /// </summary>
        /// <returns>The double value.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads raw bytes from the file.
        /// </summary>
        /// <returns>The bytes.</returns>
        byte[] ReadBytes(int count);

        /// <summary>
        /// Writes a 32bit integer into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks></remarks>
        void WriteInt32(Int32 value);

        /// <summary>
        /// Writes a 64bit integer into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks></remarks>
        void WriteInt64(Int64 value);

        /// <summary>
        /// Writes a double value into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a byte array into the file.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        void WriteBytes(byte[] bytes);

        /// <summary>
        /// Writes a zero byte into the file.
        /// </summary>
        void WriteZeroByte();

        /// <summary>
        /// Forwards the filepointer without returning the values. The implementation is free
        /// to do this by modifying the filepointer or by reading bytes and ignoring them.<br></br>
        /// This operation makes sense only when the file is read.
        /// </summary>
        /// <param name="count"></param>
        void SkipBytes(int count);

        /// <summary>
        /// Returns the position of the Filepointer.
        /// </summary>
        long Position { get; }
    }

    /// <summary>
    /// On top of an IFileIO instance, IFormattedWriter adds the capability to write 
    /// <list>
    /// <item>Guids</item>
    /// <item>Text</item>
    /// <item>Name / Value pairs</item>
    /// </list>
    /// into files. The simple types int32, int64 and double are also provided here such that all write functions 
    /// are exposed here and direct calls to the underlying <see cref="IFileIO"/> are not required.
    /// </summary>
    /// <remarks>
    /// IFormattedWriter does not expose a formatting WriteBytes method, as it is not required in this API. If direct byte access
    /// is required in the future, this method might need to be added to this interface. The implementing FormattedWriter 
    /// class offers the method already as private method. In contrast to WriteBytes which writes a length number, 
    /// WriteRaw does not write such a length value.
    /// </remarks>
    interface IFormattedWriter
    {
        /// <summary>
        /// Writes an <see cref="int"/> into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteInt32(int value);

        /// <summary>
        /// Writes a <see cref="long"/> into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteInt64(long value);

        /// <summary>
        /// Writes a <see cref="double"/> into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a <see cref="Guid"/> into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteGuid(Guid value);

        /// <summary>
        /// Text is converted into its UTF8 byte sequence and then written with as a byte[] 
        /// which in turn is written as the bytes preceeded by an int32 determining the size.<br/>
        /// </summary>
        /// <param name="text">The text to write</param>
        void WriteText(string text);

        /// <summary>
        /// Writes a NameValue struct into the file.
        /// </summary>
        /// <param name="nv">the value to write.</param>
        void WriteNameValue(NameValue nv);

        /// <summary>
        /// Writes the bytes into the file <b>without any formatting</b>. This method merely exists here 
        /// to support the strict layer pattern, so that direct calls to the underlying IFileIO instance 
        /// can be avoided. 
        /// </summary>
        /// <param name="bytes">The bytes to write. This value must not be null.</param>
        void WriteRaw(byte[] bytes);
    }

    /// <summary>
    /// On top of an IFileIO instance, IFormatteReader adds the capability to read
    /// <list>
    /// <item>Guids</item>
    /// <item>byte arrays and</item>
    /// <item>text</item>
    /// </list>
    /// from files.
    /// </summary>
    interface IFormattedReader
    {
        /// <summary>
        /// Reads a 32bit integer from the file.
        /// </summary>
        /// <returns>The value read from the file.</returns>
        Int32 ReadInt32();

        /// <summary>
        /// Reads a 64bit integer from the file.
        /// </summary>
        /// <returns>The value read from the file.</returns>
        Int64 ReadInt64();

        /// <summary>
        /// Reads a double value from the file.
        /// </summary>
        /// <returns>The value read from the file.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads the text from the file.
        /// </summary>
        /// <returns>The text.</returns>
        /// <remarks>Text is stored as UTF8 byte array which in turn is stored as a length prefixed sequence of bytes.</remarks>
        string ReadText();

        /// <summary>
        /// Reads a Name / Value instance from the file.
        /// </summary>
        /// <returns></returns>
        NameValue ReadNameValue();

        long Position { get; }

        void SkipBytes(int bytesToSkip);
    }
}
