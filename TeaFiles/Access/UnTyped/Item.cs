// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Linq;

namespace TeaTime
{
    /// <summary>
    /// Holds the values of an Item when reading a TeaFile untyped.
    /// </summary>
    /// <remarks>
    /// If the type inside a TeaFile is not knnown or not available in the program, the file can be read untyped using <see cref="TeaFile"/>. 
    /// In contrast to <see cref="TeaFile{T}"/> which exposes each item by it's known type like <c>Tick</c>, <see cref="TeaFile"/> exposes the 
    /// items as a collection of instances of this <see cref="Item"/> class. It wraps an array of objects, each holding the value of a field of the 
    /// item. This item can be printed by calling its <see cref="ToString"/> method, or using <see cref="ItemDescription.GetValueString(TeaTime.Item)"/> 
    /// from <see cref="ItemDescription"/>.
    /// <example>
    /// <code>
    /// struct Tick
    /// {
    ///     public Time Time;
    ///     public double Price;
    ///     public long Volume;
    /// }
    ///
    /// ...
    ///    
    /// // write typed
    /// using (var tf = TeaFile&lt;Tick&gt;.Create("acme.tea"))
    /// {
    ///     tf.Write(new Tick { Time = new Time(2000, 3, 4), Price = 12.34, Volume = 7200 });
    /// }
    ///
    /// // 1. read typed
    /// using (var tf = TeaFile&lt;Tick&gt;.OpenRead("acme.tea"))
    /// {
    ///     Tick item = tf.Read();  // typed read is convenient: we get a tpyed Tick back,
    ///     Time t = item.Time;     // so access to its fields simply means acessing the fields of a Tick struct.
    ///     double p = item.Price;
    ///     long v = item.Volume;
    /// }
    ///
    /// // 2. read untyped
    /// // if we do not have the type available or do not know what is inside the file,
    /// // we can still read it untyped:
    /// using (var tf = TeaFile.OpenRead("acme.tea"))
    /// {
    ///     Item item = tf.Read();      // Here an item of type Item is returned, which holds the field values as an object[].
    ///     object t = item.Values[0];  // The field values can be accessed by their field index
    ///     object p = item.Values[1];  // The values inside this array still have the types Time, double and long, but
    ///     object v = item.Values[2];  // we might not always know this at compile time. If we know it,
    ///                                 // a casts can be added:
    ///     Time tt = (Time) item.Values[0];
    ///     Console.WriteLine(item);    // The implicit call of ToString() here will cause the output:
    ///                                 // 4.3.2000, 12.34, 7200
    /// }
    /// </code>
    /// </example>
    /// 
    /// Some GUI controls (like grids) accept an object[] as the value for a row, which allows very simple binding of items.
    /// </remarks>
    public class Item
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// Items are constructed by the TeaFile class and exposed via its <see cref="TeaFile.Items"/> collection. This constructor is therefore not public.
        /// </remarks>
        /// <param name="fieldCount">The number of fields to be stored.</param>
        internal Item(int fieldCount)
        {
            this.Values = new object[fieldCount];
        }

        /// <summary>
        /// Gets the field values.
        /// </summary>
        /// <value>The array holding the item's field values.</value>
        public object[] Values { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> composed by the item's values.
        /// </summary>
        /// <remarks>
        /// This method allows easy printing of an item. The returned string is the concatenation of all values inside the item, separated by a blank.<br/>
        /// Times will be represented by their underlying value, for instance as an Int64 value holding the number of ticks.
        /// </remarks>
        /// <returns>A <see cref="System.String"/> that represents the item.</returns>
        public override string ToString()
        {
            return string.Join(" ", this.Values.Select(v => v.ToString()).ToArray());
        }
    }
}
