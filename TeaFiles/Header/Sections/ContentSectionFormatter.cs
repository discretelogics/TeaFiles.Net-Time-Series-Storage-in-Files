// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime.Header
{
    /// <summary>
    /// The ContentDescription holds a plain string desribing what the file stores.<br></br>
    /// Example: "EUR/USD" or "Temperature New York City"
    /// </summary>
    class ContentSectionFormatter : ISectionFormatter
    {
        public Int32 Id
        {
            get { return 0x80; }
        }

        public void Write(WriteContext c)
        {
            if (c == null) throw new ArgumentNullException("c");
            if (c.Description == null) return;
            if (!c.Description.ContentDescription.IsSet()) return;
            c.Writer.WriteText(c.Description.ContentDescription);
        }

        public void Read(ReadContext c)
        {
            if (c == null) throw new ArgumentNullException("c");
            c.Description.ContentDescription = c.Reader.ReadText();
        }
    }
}
