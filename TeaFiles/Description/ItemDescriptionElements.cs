// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime
{
    /// <summary>Describes the elements inside an <see cref="ItemDescription"/>. </summary>
    /// <remarks>
    /// <para>
    /// A TeaFile holds a description of the item type it stores. This item description has several elements:
    /// <ul>
    /// <li>the name of the item type (e.g. "Tick", "OHLCV")</li>
    /// <li>the size of the item</li>
    /// <li>the number of fields and their</li>
    /// <li>name,</li>
    /// <li>offset and</li>
    /// <li>type.</li>
    /// </ul>
    /// A value of this type specifies which elements shall be compared when an existing file is opened with a specific item type.    
    /// </para>
    /// <para>
    /// When a typed <see cref="TeaFile{T}"/> is opened, the <see cref="ItemDescription"/> of {T} is compared against those in the file. If it does not match those
    /// stored in the file, an exception is thrown. This ensures that a TeaFile is read with the correct type. For instance,
    /// a file that was written using the item type
    /// <code>
    /// struct Tick
    /// {
    ///     public Time Time;
    ///     public double Price;
    /// }	  
    /// </code>
    /// is not accessible using 
    /// <code>
    /// struct A
    /// {
    ///     public Time Time;
    ///     public short Value1;
    ///     public float Value2;
    ///     public double Value3;
    /// }
    /// TeaFile&lt;A&gt;.OpenRead("lab.tea");
    /// </code>
    /// because A has a different type name ("A" vs "Tick"), a different size and field names, types and offsets.
    /// Sometimes however, such strong check is undesired. If just the name of the type or the name of a field is altered,
    /// the file should still be accessible with a partially different type than the one used for writing. The optional 
    /// argument elementsToValidate in the OpenRead, Write and Append methods of <see cref="TeaFile{T}"/> allows this:
    /// <example>
    /// <code>
    /// struct Tick
    /// {
    ///     public Time Time;
    ///     public double Price;
    /// }
    /// TeaFile&lt;Tick&gt;.Create("lab.tea");
    /// 
    /// // now we read this file using another type.
    /// struct NewTick
    /// {
    ///     public Time NewTime;
    ///     public double NewPrice;
    /// }
    /// TeaFile&lt;NewTick&gt;.OpenRead("lab.tea", ItemDescriptionElements.FieldTypes);
    /// </code>
    /// </example>
    /// Since we open the file with a relaxed check that compares field types (and offsets), the call will succeed and 
    /// the file will be readable with the new type.
    /// </para>
    /// </remarks>    
    /// <seealso cref="TeaFile{T}.OpenRead(string,TeaTime.ItemDescriptionElements)"/>
    /// <seealso cref="TeaFile{T}.OpenWrite(string,TeaTime.ItemDescriptionElements)"/>
    /// <seealso cref="TeaFile{T}.Append(string,TeaTime.ItemDescriptionElements)"/>
    [Flags]
    public enum ItemDescriptionElements
    {
        /// <summary>no item description part. </summary>
        None = 0,
        /// <summary>the item . </summary>
        ItemName = 1,
        /// <summary>the item size. </summary>
        ItemSize = 2,
        /// <summary>field offsets. </summary>
        FieldOffsets = 4,
        /// <summary>field names. </summary>
        FieldNames = 4 + 8,
        /// <summary>field types. </summary>
        FieldTypes = 4 + 16,

        /// <summary>all item description parts. </summary>
        All = 0xff
    }
}
