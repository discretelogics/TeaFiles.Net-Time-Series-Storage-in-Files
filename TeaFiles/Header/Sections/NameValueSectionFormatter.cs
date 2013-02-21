// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Linq;

namespace TeaTime.Header
{
    /// <summary>
    /// The NameValue section holds a collection of Name / Value pairs. See <see cref="NameValue"/> or details of the value types.
    /// </summary>
    class NameValueSectionFormatter : ISectionFormatter
    {
        public Int32 Id
        {
            get { return 0x81; }
        }

        public void Write(WriteContext c)
        {
            if (c == null) throw new ArgumentNullException("c");
            if (c.Description == null) return;
            if (c.Description.NameValues == null) return;
            if (c.Description.NameValues.Count == 0) return;

            var nvs = c.Description.NameValues;
            c.Writer.WriteInt32(nvs.Count());
            nvs.ForEach(nv => c.Writer.WriteNameValue(nv));
        }

        public void Read(ReadContext c)
        {
            if (c == null) throw new ArgumentNullException("c");
            int count = c.Reader.ReadInt32();
            c.Description.NameValues = new NameValueCollection();
            count.Times(() => c.Description.NameValues.Add(c.Reader.ReadNameValue()));
        }
    }
}
