// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime
{
    /// <summary>
    /// Describes the types available for structs inside TeaFiles.
    /// </summary>
    /// <remarks>
    /// <para>In order to be platform compatible, only the first 10 types should be used (signed and unsigned integers, float and double).</para>
    /// <para>Some applications have a very limited type set, so even tighter restriction is sometimes favorable.
    /// To exchange data with R for instance, usage should be restricted to signed 32 bit integers and double.
    /// </para>
    /// </remarks>
    public enum FieldType
    {
        /// <summary>Invalid value. </summary>
        None = 0,

        // platform agnostic
        /// <summary>signed 1 byte value. </summary>
        Int8 = 1,
        /// <summary>signed 2 byte value. </summary>
        Int16 = 2,
        /// <summary>signed 4 byte value. </summary>
        Int32 = 3,
        /// <summary>signed 8 byte value. </summary>
        Int64 = 4,

        /// <summary>unsigned 1 byte value. </summary>
        UInt8 = 5,
        /// <summary>unsigned 2 byte value. </summary>
        UInt16 = 6,
        /// <summary>unsigned 4 byte value. </summary>
        UInt32 = 7,
        /// <summary>unsigned 8 byte value. </summary>
        UInt64 = 8,

        /// <summary>4 byte IEEE 754 floating point value. </summary>
        Float = 9,
        /// <summary>8 byte IEEE 754 floating point value. </summary>
        Double = 10,

        // platform specific
        /// <summary>.Net specific <see cref="decimal"/> type. </summary>
        NetDecimal = 0x200,

        // private extensions must have integer identifiers above 0x1000.
        /// <summary>Custom Field Types should have identifiers above 0x1000. </summary>
        Custom = 0x1000
    }

    static partial class Extensions
    {
        public static FieldType GetFieldType(this Type t)
        {
            if (t == typeof (sbyte)) return FieldType.Int8;
            if (t == typeof (Int16)) return FieldType.Int16;
            if (t == typeof (Int32)) return FieldType.Int32;
            if (t == typeof (Int64)) return FieldType.Int64;

            if (t == typeof (byte)) return FieldType.UInt8;
            if (t == typeof (UInt16)) return FieldType.UInt16;
            if (t == typeof (UInt32)) return FieldType.UInt32;
            if (t == typeof (UInt64)) return FieldType.UInt64;

            if (t == typeof (float)) return FieldType.Float;
            if (t == typeof (double)) return FieldType.Double;

            if (t == typeof (DateTime))
            {
                throw new InvalidFieldTypeException("DateTime is not supported as a field type. Use TeaTime.Time instead.");
            }
            if (t == typeof (Time)) return FieldType.Int64; // in doubt we use signed integers to avoid computational issues with signed values

            if (t == typeof (decimal)) return FieldType.NetDecimal;

            throw new ArgumentOutOfRangeException(
                "Incompatible field: The type {0} cannot be used inside the item of a TeaFile. " +
                "Change the type of the field or exclude it.".Formatted(t.FullName));
        }

        public static int GetSize(this FieldType fieldType)
        {
            switch (fieldType)
            {
            case FieldType.Int8:
                return 1;
            case FieldType.Int16:
                return 2;
            case FieldType.Int32:
                return 4;
            case FieldType.Int64:
                return 8;

            case FieldType.UInt8:
                return 1;
            case FieldType.UInt16:
                return 2;
            case FieldType.UInt32:
                return 4;
            case FieldType.UInt64:
                return 8;

            case FieldType.Float:
                return 4;
            case FieldType.Double:
                return 8;

            case FieldType.NetDecimal:
                return 16;

            case FieldType.None:
                throw new InvalidOperationException("FieldType was not set to a valid type.");

            default:
                throw new ArgumentOutOfRangeException("The FieldType '{0}' is not supported by this API.".Formatted(fieldType));
            }
        }
    }
}
