// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Text;

namespace TeaTime.IO
{
    /// <summary>
    /// FormattedWriter provides higher level file writing on top of <see cref="IFileIO"/>. In the sense of 
    /// a strict layer pattern, were direct calls to the underlying layer are not allowed, FormattedWriter 
    /// also exposes writing of int32, int64 and double and Guid values.
    /// </summary>
    /// <remarks>Methods are explained in <see cref="IFormattedWriter"/></remarks>
    class FormattedWriter : IFormattedWriter
    {
        IFileIO fileIO;

        public FormattedWriter(IFileIO fileIO)
        {
            this.fileIO = fileIO;
        }

        public void WriteInt32(int value)
        {
            this.fileIO.WriteInt32(value);
        }

        public void WriteInt64(long value)
        {
            this.fileIO.WriteInt64(value);
        }

        public void WriteDouble(double value)
        {
            this.fileIO.WriteDouble(value);
        }

        /// <summary>
        /// Writes a 16 byte Guid into the file.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks></remarks>
        public void WriteGuid(Guid value)
        {
            this.fileIO.WriteBytes(value.ToByteArray());
        }

        public void WriteRaw(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            this.fileIO.WriteBytes(bytes);
        }

        public void WriteText(string text)
        {
            var encoding = new UTF8Encoding(false, true);
            byte[] bytes = encoding.GetBytes(text ?? "");
            this.WriteLengthPrefixedBytes(bytes);
        }

        public void WriteNameValue(NameValue nameValue)
        {
            this.WriteText(nameValue.Name);
            this.WriteInt32((int)nameValue.Kind);
            switch (nameValue.Kind)
            {
            case NameValue.ValueKind.Text:
                this.WriteText(nameValue.GetValue<string>());
                break;
            case NameValue.ValueKind.Int32:
                this.WriteInt32(nameValue.GetValue<int>());
                break;
            case NameValue.ValueKind.Double:
                this.WriteDouble(nameValue.GetValue<double>());
                break;
            case NameValue.ValueKind.Guid:
                this.WriteGuid(nameValue.GetValue<Guid>());
                break;
            default:
                throw new ArgumentOutOfRangeException("NameValueKind is not supported".Formatted(nameValue.Kind));
            }
        }

        void WriteLengthPrefixedBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                this.fileIO.WriteInt32(0);
            }
            else
            {
                // length
                this.fileIO.WriteInt32(bytes.Length);
                // bytes
                this.fileIO.WriteBytes(bytes);
            }
        }
    }
}
