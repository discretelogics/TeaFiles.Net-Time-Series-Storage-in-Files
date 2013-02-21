// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Collections;
using System.Collections.Generic;

namespace TeaTime
{
    /// <summary>Provides access to items of untyped <see cref="TeaFile"/>. </summary>
    public interface IItemCollection : IEnumerable<Item>
    {
        /// <summary>Gets the number of items in the collectiion.  </summary>
        long Count { get; }

        /// <summary>Indexer to get items within this collection using array index syntax. </summary>
        /// <value>The indexed item. </value>
        Item this[int index] { get; }

        /// <summary>Returns an enumerator that iterates through the items. </summary>
        /// <param name="startIndex">the index of the first item the enumerator shall yield. </param>
        /// <returns>The enumerator. </returns>
        IEnumerator<Item> GetEnumerator(int startIndex);
    }

    sealed class ItemCollection : IItemCollection
    {
        #region State

        readonly IItemReader reader;

        #endregion

        #region Construction & Initialization

        /// <summary>
        /// This class can be instantiated by a TeaFile only, thus it is internal.
        /// </summary>
        internal ItemCollection(IItemReader reader)
        {
            this.reader = reader;
        }

        #endregion

        #region Public Interface

        /// <summary>Gets the number of items in the collectiion.  </summary>
        public long Count
        {
            get { return this.reader.Count; }
        }

        /// <summary>Indexer to get items within this collection using array index syntax. </summary>
        /// <value>The indexed item. </value>
        public Item this[int index]
        {
            get
            {
                this.reader.SetFilePointerToItem(index);
                var item = this.reader.Read();
                return item;
            }
        }

        /// <summary>Gets the enumerator over all item. </summary>
        /// <returns>The enumerator. </returns>
        public IEnumerator<Item> GetEnumerator()
        {
            return this.GetEnumerator(0);
        }

        /// <summary>Gets an enumerator of a range starting at <paramref name="startIndex"/>. </summary>
        /// <param name="startIndex">The start index. </param>
        /// <returns>The enumerator. </returns>
        public IEnumerator<Item> GetEnumerator(int startIndex)
        {
            this.reader.SetFilePointerToItem(startIndex);
            while (this.reader.CanRead)
            {
                yield return this.reader.Read();
            }
        }

        #endregion

        #region IEnumerable<Item> Members

        IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
        {
            return this.GetEnumerator(0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Item>)this).GetEnumerator();
        }

        #endregion
    }
}
