// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;

namespace TeaTime
{
    partial class Extensions
    {
        public static object Read(this BinaryReader reader, FieldType fieldType)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            switch (fieldType)
            {
            case FieldType.UInt8:
                return reader.ReadByte();
            case FieldType.UInt16:
                return reader.ReadUInt16();
            case FieldType.UInt32:
                return reader.ReadUInt32();
            case FieldType.UInt64:
                return reader.ReadUInt64();

            case FieldType.Int8:
                return reader.ReadSByte();
            case FieldType.Int16:
                return reader.ReadInt16();
            case FieldType.Int32:
                return reader.ReadInt32();
            case FieldType.Int64:
                return reader.ReadInt64();

            case FieldType.Float:
                return reader.ReadSingle();
            case FieldType.Double:
                return reader.ReadDouble();

            case FieldType.NetDecimal:
                return reader.ReadDecimal();

            default:
                throw new ArgumentOutOfRangeException("Reading FieldType '{0}' from the stream failed, the type is not supported.".Formatted(fieldType));
            }
        }
    }
}
