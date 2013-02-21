// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Text;

namespace TeaTime.IO
{
    /// <summary>
    /// FormattedReader provides higher level file reading on top of <see cref="IFileIO"/>. In the sense of 
    /// a strict layer pattern, were direct calls to the underlying layer are not allowed, FormattedReader 
    /// also exposes reading of int32, int64 and double and Guid values.
    /// </summary>
    /// <remarks>Methods are explained in <see cref="IFormattedReader"/></remarks>
    class FormattedReader : IFormattedReader
    {
        readonly IFileIO fileIO;

        public FormattedReader(IFileIO fileIO)
        {
            this.fileIO = fileIO;
        }

        public int ReadInt32()
        {
            return this.fileIO.ReadInt32();
        }

        public long ReadInt64()
        {
            return this.fileIO.ReadInt64();
        }

        public double ReadDouble()
        {
            return this.fileIO.ReadDouble();
        }

        Guid ReadGuid()
        {
            var bytes = this.fileIO.ReadBytes(16);
            return new Guid(bytes);
        }

        public string ReadText()
        {
            var bytes = this.ReadLengthPrefixedBytes();
            var encoding = new UTF8Encoding(false, true);
            return encoding.GetString(bytes);
        }

        public NameValue ReadNameValue()
        {
            string name = this.ReadText();
            NameValue.ValueKind kind = (NameValue.ValueKind)this.ReadInt32();
            switch (kind)
            {
            case NameValue.ValueKind.Text:
                string s = this.ReadText();
                return new NameValue(name, s);
            case NameValue.ValueKind.Int32:
                int n = this.ReadInt32();
                return new NameValue(name, n);
            case NameValue.ValueKind.Double:
                double d = this.ReadDouble();
                return new NameValue(name, d);
            case NameValue.ValueKind.Guid:
                Guid g = this.ReadGuid();
                return new NameValue(name, g);
            default:
                throw new ArgumentOutOfRangeException("NameValueKind iss not supported".Formatted(kind));
            }
        }

        public void SkipBytes(int n)
        {
            this.fileIO.SkipBytes(n);
        }

        public long Position
        {
            get { return this.fileIO.Position; }
        }

        byte[] ReadLengthPrefixedBytes()
        {
            var count = this.fileIO.ReadInt32();
            if (count == 0) return new byte[0];
            return this.fileIO.ReadBytes(count);
        }
    }
}
