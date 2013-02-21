// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TeaTime.IO;

namespace TeaTime.Header
{
    class HeaderManager
    {
        public const Int64 Signature = 0x0d0e0a0402080500; // t e a 4 2 at 5 0 0 

        ISectionFormatter[] sectionFormatters;

        #region Configuration

        public HeaderManager()
        {
            this.sectionFormatters = new ISectionFormatter[]
                {
                    new ItemSectionFormatter(),
                    new ContentSectionFormatter(),
                    new NameValueSectionFormatter(),
                    new TimeSectionFormatter(),
                };
        }

        public void AddSectionFormatter(ISectionFormatter formatter)
        {
            this.sectionFormatters = this.sectionFormatters.Union(formatter.ToEnumerable()).ToArray();
        }

        #endregion

        #region Core

        public ReadContext ReadHeader(IFormattedReader r)
        {
            var rc = new ReadContext(r);
            try
            {
                var bom = r.ReadInt64();
                if (bom != 0x0d0e0a0402080500) throw new FileFormatException("Expected Signature not found. Either this file is not a TeaFile or the byte order (endianness) differs between the machine were the file was written and the local machine. Expected:'{0:x}'\nFound:   '{1:x}'\n".Formatted(0x0d0e0a0402080500, bom));
                rc.ItemAreaStart = r.ReadInt64();
                rc.ItemAreaEnd = r.ReadInt64();
                rc.SectionCount = r.ReadInt64();

                for (int i = 0; i < rc.SectionCount; i++)
                {
                    int sectionId = r.ReadInt32();
                    int nextSectionOffset = r.ReadInt32();
                    int nextSectionStart = (int)r.Position + nextSectionOffset;
                    if (nextSectionStart > rc.ItemAreaStart) throw new FileFormatException("NextSectionOffset of section number {0} is wrong: Next Section Start would be beyond ItemStart.".Formatted(i));

                    ISectionFormatter sectionFormatter = this.sectionFormatters.FirstOrDefault(f => f.Id == sectionId);
                    if (sectionFormatter != null)
                    {
                        sectionFormatter.Read(rc);
                        if (r.Position > nextSectionStart) throw new FileFormatException("Section read too many bytes from the stream. SectionId:{0} SectionName:{1}".Formatted(sectionId, sectionFormatter.GetType().Name.Replace("Formatter", "")));
                    }

                    int bytesToSkip = nextSectionStart - (int)r.Position;
                    if (bytesToSkip < 0) throw new FileFormatException("Reading sections from the file header failed. Section with id {0} reads more bytes than reserved for that section".Formatted(sectionId));
                    r.SkipBytes(bytesToSkip);
                }

                if (r.Position > rc.ItemAreaStart) throw new FileFormatException("Stream position is behind start of item area.");
                r.SkipBytes((int)rc.ItemAreaStart - (int)r.Position);

                if (rc.ItemAreaStart != r.Position) throw new FileFormatException("Stream Position could not be set to start of item area.");
                return rc;
            }
            catch (FileFormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // communicate any unexpected Header Read Error (e.g. an error while reading from disc)
                throw new FileFormatException("Error reading TeaFile Header: {0}".Formatted(ex.Message), ex);
            }
        }

        public WriteContext WriteHeader(IFormattedWriter w, TeaFileDescription description)
        {
            var wc = new WriteContext(w);
            wc.ItemAreaStart = 32; //  if no sections are created, ItemArea will start at 32. This value will change is sections are created

            wc.Description = description;
            byte[] sections = this.CreateSections(wc);

            w.WriteInt64(0x0d0e0a0402080500);
            w.WriteInt64(wc.ItemAreaStart);
            w.WriteInt64(0);
            w.WriteInt64(wc.SectionCount);
            wc.Writer.WriteRaw(sections);

            //var sectionPadding = wc.ItemAreaStart - w.Position;
            //if (sectionPadding < 0) throw new FileFormatException("Stream position (potentially after writing sections) is behind item start. After opening a TeaFile and reading the header, the file pointer must be exactly at the begin of the item area.");
            //w.WriteZeroBytes((int) sectionPadding);

            return wc;
        }

        #endregion

        #region Internal

        /// <summary>
        /// Creates the header's sections from the descriptions found in the context. Computes their positions 
        /// and sets the FirstItemPosition property in the context.
        /// </summary>
        internal byte[] CreateSections(WriteContext wc)
        {
            var saved = wc.Writer;

            using (var sectionStream = new MemoryStream()) // see noop-Dispose comment below
            {
                var sectionWriter = new FormattedWriter(new FileIO(sectionStream));
                int pos = 32; // sections start at byte position 32
                foreach (var formatter in this.sectionFormatters)
                {
                    // payload
                    using (var payloadStream = new MemoryStream()) // actually MemoryStream.Dispose() is a noop here, but for the code analysers pleasure we wrap these usings around
                    {
                        wc.Writer = new FormattedWriter(new FileIO(payloadStream));
                        formatter.Write(wc);
                        var size = (int)payloadStream.Length;
                        if (size > 0)
                        {
                            // section id
                            sectionWriter.WriteInt32(formatter.Id);
                            pos += 4;

                            // nextSectionOffset
                            sectionWriter.WriteInt32(size);
                            pos += 4;

                            // payload
                            sectionWriter.WriteRaw(payloadStream.ToArray());
                            pos += size; // no padding or spacing done here

                            wc.SectionCount++;
                        }
                    }
                }

                // padding
                int paddingBytes = (8 - pos % 8);
                if (paddingBytes == 8) paddingBytes = 0;
                sectionWriter.WriteRaw(new byte[paddingBytes]);
                wc.ItemAreaStart = pos + paddingBytes; // first item starts padded on 8 byte boundary.

                wc.Writer = saved;
                return sectionStream.ToArray();
            }
        }

        #endregion

        #region singleton

        [DebuggerNonUserCode]
        public static HeaderManager Instance
        {
            get { return Singleton.instance; }
        }

        // ReSharper disable ClassNeverInstantiated.Local
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        class Singleton
            // ReSharper restore ClassNeverInstantiated.Local
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            // ReSharper disable EmptyConstructor

            internal static readonly HeaderManager instance = new HeaderManager();

            // ReSharper restore EmptyConstructor
        }

        #endregion
    }
}
