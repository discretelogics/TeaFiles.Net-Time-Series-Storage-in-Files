// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Collections;
using System.Collections.Generic;

namespace TeaTime
{
    /// <summary>Provides access to items using typed <see cref="TeaFile{T}"/>. </summary>
    /// <remarks>
    /// This interface adds direct access and count to <see cref="IEnumerable{T}"/>.
    /// </remarks>
    /// <typeparam name="T">The item type.</typeparam>
    public interface IItemCollection<out T> : IEnumerable<T> where T : struct
    {
        /// <summary>
        /// The number of items in the file.
        /// </summary>
        /// <value>The number of items in the file.</value>
        long Count { get; }

        /// <summary>Indexer to get items within this collection using array index syntax. </summary>
        /// <value>The indexed item. </value>
        T this[long index] { get; }
    }

    /// <summary>
    /// A TeaFile holds a collection of items. In instance of this class exposes access to this collection.
    /// </summary>
    /// <remarks>
    /// Access is provided by an enumerator. In addition, the number of items in the file 
    /// is exposed via its <see cref="Count"/> property.<br> </br>
    /// Usage:<br></br>
    /// <code>
    /// using(var teaFile = TeaFile&lt;OHLCV&gt;.OpenRead("ohlcv.tea"))
    /// {
    ///		foreach(OHLCV item in teaFile.Items)
    ///		{
    ///			Console.WriteLine(item.Time);
    ///			Console.WriteLine(item.Open);
    ///		}
    /// }
    /// </code>
    /// </remarks>
    class ItemCollection<T> : IItemCollection<T> where T : struct
    {
        #region State

        internal readonly TeaFile<T> teaFile;

        #endregion

        #region Construction & Initialization

        /// <summary>
        /// Contructs an instance, initializing it with a reference to the TeaFile it provides access to.
        /// </summary>
        /// <remarks>
        /// This class shall be instantiated by an instance of a TeaFile only, thus it is internal.
        /// </remarks>
        /// <param name="teaFile">The tea file.</param>
        internal ItemCollection(TeaFile<T> teaFile)
        {
            this.teaFile = teaFile;
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// The number of items in the file.
        /// </summary>
        /// <value>The number of items in the file.</value>
        public long Count
        {
            get { return this.teaFile.Count; }
        }

        /// <summary>Indexer to get items within this collection using array index syntax. </summary>
        /// <value>The indexed item. </value>
        public T this[long index]
        {
            get
            {
                this.teaFile.SetFilePointerToItem(index);
                return this.teaFile.Read();
            }
        }

        #endregion

        #region IEnumerable<T>

        /// <summary>Gets the enumerator. </summary>
        /// <returns>The enumerator. </returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.teaFile.SetFilePointerToItem(0);
            T value;
            while (this.TryRead(out value))
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Iterator implemented using the yield statement do not allow a yield return
        /// statement inside a try catch block, so we encapsulate the necessary try catch
        /// in this method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool TryRead(out T value)
        {
            return this.teaFile.TryRead(out value);
        }

        #endregion
    }
}
