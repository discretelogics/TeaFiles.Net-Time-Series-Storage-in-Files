// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TeaTime
{
    /// <summary>
    /// The layout analyzer analyses the layout of the struct used for a TeaFile.<br></br>
    /// This analysis is done by creating a sample instance of the struct and assigning a 
    /// "magic" value to each field that is then searched for.
    /// </summary>
    /// <typeparam name="T">The struct type used inside the TeaFile. While this type is constrained 
    /// to a struct by the C# language, it is also disallowed that this struct references class by any of its fields.
    /// For instance, the struct passed must not hold a string as field.</typeparam>
    class LayoutAnalyzer<T> : SafeBuffer where T : struct
    {
        public LayoutAnalyzer() : base(true)
        {
            this.Initialize<T>(1);
            this.byteBuffer = new byte[this.ByteLength];
        }

        #region core

        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        /// Returns the aligned size, which is >= the size returned by sizeof(T);
        /// </summary>
        public int TypeSize
        {
            get { return this.byteBuffer.Length; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public unsafe List<AnalyzedField> AnalyzeLayout()
        {
            try
            {
                var analyzedFields = new List<AnalyzedField>();
                bool dotSyntaxOccured = false;

                fixed (byte* p = this.byteBuffer)
                {
                    base.SetHandle((IntPtr)p);
                    foreach (FieldPath fieldPath in this.GetPrimitiveFields())
                    {
                        int fieldOffset = this.GetOffset(p, fieldPath);
                        var af = new AnalyzedField(fieldPath, fieldOffset);
                        dotSyntaxOccured = af.Name.Contains(".");
                        analyzedFields.Add(af);
                    }
                }

                // try to remove path syntax from field names if they are not necessary
                if (dotSyntaxOccured)
                {
                    if (analyzedFields.Select(o => o.FieldPath.Last.Name).Distinct().Count() == analyzedFields.Count)
                    {
                        analyzedFields.ForEach(af => af.Name = af.FieldPath.Last.Name);
                    }
                }

                return analyzedFields;
            }
            catch (ItemException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Analyzing layout of type failed: {0}".Formatted(ex.Message), ex);
            }
        }

        /// <summary>
        /// Creates primitive fields for the current type T.
        /// </summary>
        internal IEnumerable<FieldPath> GetPrimitiveFields()
        {
            var rootPath = new FieldPath();
            return typeof (T).GetFields(bindingFlags).Select(rootPath.AppendChild).SelectMany(this.GetPrimitiveFields);
        }

        /// <summary>
        /// Returns child fields from the field specified by <paramref name="path"/>.
        /// </summary>
        IEnumerable<FieldPath> GetPrimitiveFields(FieldPath path)
        {
            FieldInfo f = path.Last;
            if (IsPrimitive(f.FieldType))
            {
                yield return path;
            }
            else
            {
                foreach (FieldPath primitiveField in f.FieldType.GetFields(bindingFlags).Select(path.AppendChild).SelectMany(this.GetPrimitiveFields))
                {
                    yield return primitiveField;
                }
            }
        }

        static bool IsPrimitive(Type t)
        {
            return t == typeof (SByte) ||
                   t == typeof (Int16) ||
                   t == typeof (Int32) ||
                   t == typeof (Int64) ||
                   t == typeof (Byte) ||
                   t == typeof (UInt16) ||
                   t == typeof (UInt32) ||
                   t == typeof (UInt64) ||
                   t == typeof (double) ||
                   t == typeof (float) ||
                   t == typeof (Decimal) ||
                   t == typeof (DateTime) ||
                   t == typeof (Time) ||
                   t == typeof (char) ||
                   t == typeof (bool);
        }

        internal unsafe int GetOffset(byte* p, FieldPath fieldPath)
        {
            if (fieldPath == null) throw new ArgumentNullException("fieldPath");

            var field = fieldPath.Last;

            if (field.FieldType == typeof (sbyte))
            {
                sbyte magic = 17;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (sbyte));
            }
            if (field.FieldType == typeof (Int16))
            {
                Int16 magic = 1702;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (Int16));
            }
            if (field.FieldType == typeof (Int32))
            {
                Int32 magic = 4122;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (Int32));
            }
            if (field.FieldType == typeof (Int64))
            {
                Int64 magic = 0x1111222233334444;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (Int64));
            }
            if (field.FieldType == typeof (byte))
            {
                byte magic = 125;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, &magic, this.byteBuffer.Length, sizeof (byte));
            }
            if (field.FieldType == typeof (UInt16))
            {
                UInt16 magic = 125;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (UInt16));
            }
            if (field.FieldType == typeof (UInt32))
            {
                UInt32 magic = 433322211;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (UInt32));
            }
            if (field.FieldType == typeof (UInt64))
            {
                UInt64 magic = 99887766554433;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (UInt64));
            }
            if (field.FieldType == typeof (double))
            {
                double magic = 1234.5678;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (double));
            }
            if (field.FieldType == typeof (float))
            {
                float magic = 12.34f;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (float));
            }
            if (field.FieldType == typeof (DateTime))
            {
                DateTime magic = new DateTime(1967, 3, 4, 11, 7, 12);
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (Int64));
            }
            if (field.FieldType == typeof (Time))
            {
                Time magic = new Time(1972, 3, 4, 11, 7, 12);
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (Int64));
            }
            if (field.FieldType == typeof (decimal))
            {
                decimal magic = 1234.5678m;
                var testInstance = this.GetStructWithOneFieldSet(fieldPath.Fields, magic);
                this.Write(0, (T)testInstance);
                return ByteSearcher.GetPosition(p, (byte*)&magic, this.byteBuffer.Length, sizeof (decimal));
            }

            throw new ItemException("The FieldType '{0}' of field '{1}' is not supported.'".Formatted(field.FieldType, field.Name));
        }

        internal static object CreateAndSet(FieldInfo f, object fieldValue)
        {
            object instance = Activator.CreateInstance(f.DeclaringType);
            f.SetValueDirect(__makeref(instance), fieldValue);
            return instance;
        }

        internal object GetStructWithOneFieldSet(List<FieldInfo> fields, object fieldValue)
        {
            int i = fields.Count;
            object instance = CreateAndSet(fields[--i], fieldValue);
            while (--i >= 0)
            {
                instance = CreateAndSet(fields[i], instance);
            }
            return instance;
        }

        #endregion

        #region internals

        /// <summary>
        /// Required Override from abstract bases SafeHandle and SafeBuffer.
        /// </summary>
        /// <returns>True signal everything went fine while false signals catastrophic failure. So we return true.</returns>
        protected override bool ReleaseHandle()
        {
            return true;
        }

        internal bool ReleaseHandleTestAccessor()
        {
            return this.ReleaseHandle();
        }

        #endregion

        #region state

        readonly byte[] byteBuffer;

        #endregion
    }
}
