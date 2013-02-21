// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace TeaTime
{
    ///<summary>
    /// Access items in TeaFiles via safe memory mapping using <see cref="MemoryMappedFile"/>.
    ///</summary>
    /// <remarks>
    /// This safe way of memory mapping incurs significant overhead such that performance 
    /// gains are far behind using unsafe P/Invoke Memory Mapping.<br/><br/>
    /// 
    /// If pure managed code is required, this class might be prefered over <see cref="RawMemoryMapping{T}"/>. It also
    /// provides a type safe interface, while <see cref="RawMemoryMapping{T}"/> provides raw byte* only. For
    /// performance reasons however, MemoryMappedTeaFile is much faster.
    /// </remarks>
    public sealed class ManagedMemoryMapping<T> : IDisposable where T : struct
    {
        readonly int itemSize;
        readonly MemoryMappedFile memoryMappedFile;
        readonly MemoryMappedViewAccessor accessor;

        internal ManagedMemoryMapping(string path, long itemAreaStart, long itemAreaLength, int itemSize)
        {
            this.itemSize = itemSize;
            var fi = new FileInfo(path);
            this.memoryMappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open, path, fi.Length, MemoryMappedFileAccess.Read);
            this.accessor = this.memoryMappedFile.CreateViewAccessor(itemAreaStart, itemAreaLength, MemoryMappedFileAccess.Read);
        }

        /// <summary>Reads an item at a given position. </summary>
        /// <param name="itemIndex">The item index to read. </param>
        /// <returns>The item at index <paramref name="itemIndex"/>. </returns>
        public T Read(long itemIndex)
        {
            // hopefully the compiler removes some copy operations here
            T item;
            this.accessor.Read(itemIndex * this.itemSize, out item);
            return item;
        }

        /// <summary>Indexer to get items within this collection using array index syntax. </summary>
        /// <value>The indexed item. </value>
        public T this[int itemIndex]
        {
            get { return this.Read(itemIndex); }
        }

        #region Implementation of IDisposable

        /// <summary>Releases all resources. </summary>
        public void Dispose()
        {
            this.accessor.Dispose();
            this.memoryMappedFile.Dispose();
        }

        #endregion
    }
}
