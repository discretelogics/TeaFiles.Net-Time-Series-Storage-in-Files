// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeaTime
{
    class FieldPath
    {
        readonly List<FieldInfo> fields;

        public FieldPath()
        {
            this.fields = new List<FieldInfo>();
        }

        FieldPath(IEnumerable<FieldInfo> fieldInfos)
        {
            this.fields = new List<FieldInfo>(fieldInfos);
        }

        public List<FieldInfo> Fields
        {
            get { return this.fields; }
        }

        public FieldInfo Last
        {
            get { return this.fields.Last(); }
        }

        public string DotPath
        {
            get
            {
                // filter Value field from Event<T>
                return string.Join(".", this.fields.Where(f => !IsTeaTimeEventDataToSkip(f)).Select(f => f.Name));
            }
        }

        /// <summary>
        /// The value part of <see cref="Event{T}"/> is skipped if its type is a struct.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns><c>true</c> if the event shall be skipped; otherwise, <c>false</c>.</returns>
        /// <remarks>This avoids creating fieldnames "Time, Value.Price, Value.Volume" for Event&lt;Trade&gt;.<br></br>
        /// If however the type is a primitive, like a double, then this double shall appear as field with name "Value".
        /// </remarks>
        static bool IsTeaTimeEventDataToSkip(FieldInfo f)
        {
            if (f.Name != "Value") return false;
            if (f.FieldType.IsPrimitive) return false;
            Type t = f.DeclaringType;
            return t.IsGenericType && (t.GetGenericTypeDefinition() == typeof (Event<>));
        }

        public FieldPath AppendChild(FieldInfo fi)
        {
            FieldPath childPath = new FieldPath(this.fields);
            childPath.fields.Add(fi);
            return childPath;
        }

        public override string ToString()
        {
            return this.DotPath;
        }
    }
}
