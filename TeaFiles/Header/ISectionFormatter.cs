// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime.Header
{
    /// <summary>
    /// Each section in a TeaFile's header is identifed by an Id and can be written and read.<br></br>
    /// Passing context instances to the Read and Write methods exposes the description object to be read 
    /// or populated.
    /// </summary>
    interface ISectionFormatter
    {
        /// <summary>
        /// The section ID.<br></br>
        /// Section IDs are defined in the TeaFile format definition. Custom sections can be added whose IDs
        /// must be in the range as specified in the TeaFile definition.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Writes the section into the file taking the description values from the context's Description property.
        /// </summary>
        /// <param name="c"></param>
        void Write(WriteContext c);

        /// <summary>
        /// Reads the section and adds the values extracted from the file to the Description in the ReadContext argument.
        /// </summary>
        /// <param name="c"></param>
        void Read(ReadContext c);
    }
}
