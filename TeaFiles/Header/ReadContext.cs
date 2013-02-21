// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using TeaTime.IO;

namespace TeaTime.Header
{
    /// <summary>
    /// During header reading, the ReaContext provides section descriptions 
    /// </summary>
    class ReadContext
    {
        public ReadContext(IFormattedReader reader)
        {
            this.Description = new TeaFileDescription();
            this.Reader = reader;
        }

        public IFormattedReader Reader { get; private set; }
        public TeaFileDescription Description { get; private set; }

        public long ItemAreaStart { get; set; }
        public long ItemAreaEnd { get; set; }
        public long SectionCount { get; set; }
    }
}
