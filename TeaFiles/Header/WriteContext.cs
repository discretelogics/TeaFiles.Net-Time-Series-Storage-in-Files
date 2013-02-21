// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using TeaTime.IO;

namespace TeaTime.Header
{
    /// <summary>
    /// An instance of this class is passed to all section formatters's <see cref="ISectionFormatter.Write"/> method, providing the 
    /// formatter access to the <see cref="TeaFileDescription"/> and the writer.
    /// </summary>
    class WriteContext
    {
        public WriteContext(IFormattedWriter writer)
        {
            this.Writer = writer;
        }

        public IFormattedWriter Writer { get; set; }
        public TeaFileDescription Description { get; set; }
        public long ItemAreaStart { get; set; }
        public int SectionCount { get; set; }
    }
}
