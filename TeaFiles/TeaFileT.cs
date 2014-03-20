// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TeaTime
{
    /// <summary>Create, write and read TeaFiles using their item type (typed reading).</summary>
    /// <remarks>This class is the core of this assembly. Use it to <see cref="Create(string,string,TeaTime.NameValueCollection,bool)"/>, 
    /// <see cref="Write(T)"/> and <see cref="Read"/> items to and from TeaFiles.
    /// </remarks>
    ///<typeparam name="T">The item type.</typeparam>
    public sealed class TeaFile<T> : IDisposable where T : struct
    {
        #region Construction

        TeaFile()
        {
        }

        /// <summary>Create a new TeaFile for type <typeparamref name="T"/>. </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="path">path of the file. </param>
        /// <param name="contentDescription">A string describing the content. Defaults to null.</param>
        /// <param name="nameValues">A collectiion of name values describing the content. Defaults to null.</param>
        /// <param name="includeItemDescription">Specifies if the item description is written into the file.</param>
        /// <returns>A new instance of <see cref="TeaFile{T}"/>. </returns>
        /// <remarks>
        /// Calling this methods creates a new file and writes the file header holding a description of <typeparamref name="T"/>.
        /// The returned TeaFile{T} instance is in open state, read for writing.
        /// </remarks>
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
        /// </code>		 
        /// </example>
        public static TeaFile<T> Create(string path, string contentDescription = null, NameValueCollection nameValues = null, bool includeItemDescription = true)
        {
            if (path == null) throw new ArgumentNullException("path");
            Stream stream = new FileStream(path, FileMode.CreateNew);
            try
            {
                return Create(stream, true, contentDescription, nameValues, includeItemDescription);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a TeaFile, using the specified stream as the underlying storage media.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="contentDescription">The content description.</param>
        /// <param name="nameValues">The name values.</param>
        /// <param name="includeItemDescription">if set to <c>true</c> [include item description].</param>
        /// <returns></returns>
        /// <remarks>
        /// Instead of creating a new <see cref="FileStream"/>, this method takes the <see cref="Stream"/> passed. This provides 
        /// more control over the stream, like setting specific <see cref="FileShare"/> attributes. It also allows usage of 
        /// alternative storage medias like <see cref="MemoryStream"/>.
        /// 
        /// Note that this method does not transfer ownership of the stream to the returned instance of TeaFileT and that disposing 
        /// the TeaFileT instance does not dispose <param name="stream"></param>. So the management of the passed stream resource remains the duty of the caller.
        /// <see cref="Create(string,string,TeaTime.NameValueCollection,bool)"/>
        /// </remarks>
        public static TeaFile<T> Create(Stream stream, string contentDescription = null, NameValueCollection nameValues = null, bool includeItemDescription = true)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Create(stream, false, contentDescription, nameValues, includeItemDescription);
        }

        static TeaFile<T> Create(Stream stream, bool ownsStream, string contentDescription = null, NameValueCollection nameValues = null, bool includeItemDescription = true)
        {
            var tf = new TeaFile<T>();
            try
            {
                tf.core = new TeaFileCore(stream, ownsStream);
                tf.buffer = new SafeBuffer<T>(stream);
                if (includeItemDescription)
                {
                    tf.Description.ItemDescription = ItemDescription.FromAnalysis<T>();
                }
                tf.Description.ContentDescription = contentDescription;
                tf.Description.NameValues = nameValues;
                tf.Description.Timescale = Time.Scale; // The 
                tf.core.WriteHeader();
                tf.Flush();
                return tf;
            }
            catch (Exception)
            {
                tf.Dispose();
                throw;
            }
        }

        /// <summary>Opens a TeaFile in read only mode. </summary>
        /// <exception cref="ArgumentNullException">Thrown when path is null. </exception>
        /// <param name="path">The path of the file. </param>
        /// <param name="elementsToValidate">Elements of the ItemDescription of <typeparamref name="T"/> compared against hose inside thefile.
        /// <see cref="ItemDescriptionElements"/> for details. </param>
        /// <returns>The file, open for reading. </returns>
        public static TeaFile<T> OpenRead(string path, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            if (path == null) throw new ArgumentNullException("path");
            Stream stream = new FileStream(path, FileMode.Open);
            return OpenRead(stream, true, elementsToValidate);
        }

        /// <summary>Opens a TeaFile for read. </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="stream">            The underlying stream. </param>
        /// <param name="elementsToValidate">Elements of the ItemDescription to be checked.
        /// <see cref="ItemDescriptionElements"/> for details. </param>
        /// <returns>The file open for reading.</returns>
        /// <remarks>This overload allows a <see cref="MemoryStream"/> or any Stream implmementation to be used as the underlying
        /// persistence media or specifically opened streams, for instance with specific <see cref="FileShare"/> flags set.</remarks>
        public static TeaFile<T> OpenRead(Stream stream, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return OpenRead(stream, false, elementsToValidate);
        }

        static TeaFile<T> OpenRead(Stream stream, bool ownsStream, ItemDescriptionElements elementsToValidate)
        {
            var tf = new TeaFile<T>();
            try
            {
                tf.core = new TeaFileCore(stream, ownsStream);
                tf.buffer = new SafeBuffer<T>(stream);
                tf.core.ReadHeader();

                //  reflecting the current type incurs some cost, so we do it only if we are asked to consider 
                //  current item layout to check it against to layout described in the file
                ItemDescription accessorDescription = null;
                if (elementsToValidate != ItemDescriptionElements.None)
                {
                    accessorDescription = ItemDescription.FromAnalysis<T>();
                }
                tf.core.IsAccessibleWith(accessorDescription, elementsToValidate);
                return tf;
            }
            catch (Exception)
            {
                tf.Dispose();
                throw;
            }
        }

        /// <summary>Opens a TeFile in write mode. </summary>
        /// <exception cref="ArgumentNullException">Path is null. </exception>
        /// <param name="path">The path of the file. </param>
        /// <param name="elementsToValidate">Elements of the ItemDescription to be checked.
        /// <see cref="ItemDescriptionElements"/> for details. </param>
        /// <returns>A TeaFile in write mode. </returns>
        /// <remarks><see cref="OpenRead(string,TeaTime.ItemDescriptionElements)"/> about the <paramref name="elementsToValidate"/> parameter.</remarks>
        public static TeaFile<T> OpenWrite(string path, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            if (path == null) throw new ArgumentNullException("path");
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            try
            {
                return OpenWrite(stream, true, elementsToValidate);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        /// <summary>Opens a TeFile in write mode. </summary>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null. </exception>
        /// <param name="stream">The underlying storage media. </param>
        /// <param name="elementsToValidate">Elements of the ItemDescription to be checked.
        /// <see cref="ItemDescriptionElements"/> for details. </param>
        /// <returns>A TeaFile, open in write mode. </returns>
        public static TeaFile<T> OpenWrite(Stream stream, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return OpenWrite(stream, false, elementsToValidate);
        }

        static TeaFile<T> OpenWrite(Stream stream, bool ownsStream, ItemDescriptionElements elementsToValidate)
        {
            var tf = new TeaFile<T>();
            try
            {
                tf.buffer = new SafeBuffer<T>(stream);
                tf.core = new TeaFileCore(stream, ownsStream);
                tf.core.ReadHeader();
                tf.core.IsAccessibleWith(ItemDescription.FromAnalysis<T>(), elementsToValidate);
                // filepointer is at begin of item area. finally, the user might want to read.
                // using filepointer is essential here.
                // check the item api to allow this.
                return tf;
            }
            catch (Exception)
            {
                tf.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Opens the file in write mode and allows appending items to it. The filepointer is set to the end of the file.
        /// </summary>
        /// <param name="path">The path of the file. </param>
        /// <param name="elementsToValidate">The elements of the ItemDescription to validate.
        /// <see cref="ItemDescriptionElements"/> for details. </param>
        /// <returns>. </returns>
        /// <exception cref="ArgumentException">If path is null.</exception>
        public static TeaFile<T> Append(string path, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            if (path == null) throw new ArgumentNullException("path");

            Stream headerStream = null;
            Stream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            try
            {
                headerStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return Append(stream, true, headerStream, elementsToValidate);
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
            finally
            {
                if (headerStream != null)
                {
                    headerStream.Dispose();
                }
            }
        }

        static TeaFile<T> Append(Stream stream, bool ownsStream, Stream headerReadStream, ItemDescriptionElements elementsToValidate = ItemDescriptionElements.All)
        {
            var tf = new TeaFile<T>();
            try
            {
                TeaFileDescription description;
                tf.core = new TeaFileCore(stream, ownsStream);

                // A file opened for appending, cannot be read. Therefore we create a second file, read it and assign its description to the current TeaFile<> instance.
                using (var tfheader = OpenRead(headerReadStream, false, elementsToValidate)) // we pass headerStream ownership to tfheader, so it will be closed after header reaeding.
                {
                    description = tfheader.Description;
                    if (!tfheader.core.CanAppend)
                    {
                        throw new IOException("Cannot append to file because it has preallocated space between the end of the item area and the end of the file.");
                    }
                    tf.core.Assign(tfheader.ItemAreaStart, tfheader.core.ItemAreaEndMarker);
                }

                // if the stream is a filestream that was opened in FileMode.Append, this call is redundant.
                // this line is here for the allowed case were the stream and headerstream point to the same stream.
                tf.SetFilePointerToEnd();

                tf.core.Description = description;
                tf.buffer = new SafeBuffer<T>(stream);
                return tf;
            }
            catch (Exception)
            {
                tf.Dispose();
                throw;
            }
        }

        #endregion

        #region Public Interface

        /// <summary>Reads the next item from the file </summary>
        /// <returns>The item at the filepointer. </returns>
        public T Read()
        {
            return this.buffer.Read();
        }

        /// <summary>Writes an item in to the file at the position of the file pointer. </summary>
        /// <param name="item">The item to write. </param>
        public void Write(T item)
        {
            this.buffer.Write(item);
        }

        /// <summary>Writes several items in to the file. </summary>
        /// <param name="values">The items to write. </param>
        public void Write(IEnumerable<T> values)
        {
            values.ForEach(Write);
        }

        /// <summary>
        /// Set file pointer based on item index.
        /// </summary>
        /// <param name="itemIndex">Zero based index of the item tthe filpointer shall point to after the call.</param>
        public void SetFilePointerToItem(long itemIndex)
        {
            this.core.SetFilePointerToItem(itemIndex);
        }

        /// <summary>Sets the file pointer to the physical end of the file. </summary>
        public void SetFilePointerToEnd()
        {
            this.core.SetFilePointerToEnd();
        }

        /// <summary>
        /// Removes all items from the file.
        /// </summary>
        /// <remarks>
        /// All items are removed and the file size 		 
        /// </remarks>
        public void Truncate()
        {
            this.core.Truncate();
        }

        /// <summary>Flushes the file. </summary>
        public void Flush()
        {
            this.core.Stream.Flush();
        }

        /// <summary>Closes this file. </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ItemAreaStart:" + this.core.ItemAreaStart);
            sb.AppendLine("ItemAreaEnd:" + this.core.ItemAreaEnd);
            sb.AppendLine("HasPreAallocatedSpace:" + this.core.HasPreallocatedSpace);
            sb.AppendLine("ItemAreaSize:" + this.core.ItemsAreaSize);
            return sb.ToString();
        }

        #endregion

        #region Internals

        /// <summary>
        /// TryRead is more performant as it avoids throwing an EndOfStreamException when the end of the file is reached.
        /// </summary>
        /// <remarks>This method cannot be used when header tracing is intended.</remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool TryRead(out T value)
        {
            return this.buffer.TryRead(out value);
        }

        internal Stream Stream
        {
            get { return this.core.Stream; }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The description of the file's content. The returned <see cref="Description"/> instance in turn holds descriptions 
        /// of the items in the file, a <see cref="NameValueCollection"/> with arbitrary values and a simple ContentDescription string.
        /// 
        /// TeaFiles may or may not hold any description of their content. If not any description is available, this property will be null.
        /// </summary>
        public TeaFileDescription Description
        {
            get { return this.core.Description; }
        }

        /// <summary>Provides access to the items in the file. </summary>
        public IItemCollection<T> Items
        {
            get { return new ItemCollection<T>(this); }
        }

        /// <summary>The name of the file. </summary>
        /// <value>This is the path passed when the file was opened.</value>
        public string Name
        {
            get { return this.core.Name; }
        }

        /// <summary>Gets the item area start. </summary>
        /// <value>This is the byte offset from the start of the file.</value>
        public long ItemAreaStart
        {
            get { return this.core.ItemAreaStart; }
        }

        /// <summary>Gets the item area end. </summary>
        /// <value>This is the byte offset from the start of the file.</value>
        public long ItemAreaEnd
        {
            get { return this.core.ItemAreaEnd; }
        }

        /// <summary>Gets the size of the item area in bytes. </summary>
        public long ItemAreaSize
        {
            get { return this.core.ItemsAreaSize; }
        }

        /// <summary>Gets the number of items in the file.  </summary>
        public long Count
        {
            get { return this.core.Count; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>Finaliser. </summary>
        ~TeaFile()
        {
            this.core.Destruct();
        }

        /// <summary>Releases all resources acquired by the instance. </summary>
        public void Dispose()
        {
            this.core.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region State

        TeaFileCore core;
        SafeBuffer<T> buffer;

        #endregion

        /// <summary>Opens a memory mapping of the file using managed memory mapping. </summary>
        /// <param name="path">path of the file. </param>
        /// <returns>An instance of <see cref="ManagedMemoryMapping{T}"/> providing access to the items in the file. </returns>
        public static ManagedMemoryMapping<T> OpenMemoryMapping(string path)
        {
            long itemAreaStart;
            long itemAreaEnd;
            int itemSize;
            using (var tf = OpenRead(path))
            {
                itemAreaStart = tf.ItemAreaStart;
                itemAreaEnd = tf.ItemAreaEnd;
                itemSize = tf.Description.ItemDescription.ItemSize;
            }
            return new ManagedMemoryMapping<T>(path, itemAreaStart, itemAreaEnd - itemAreaStart, itemSize);
        }

        /// <summary>Opens a memory mapping of the file using unsafe memory mapping. </summary>
        /// <param name="path">path of the file. </param>
        /// <returns>An instance of <see cref="RawMemoryMapping{T}"/> providing access to the items in the file. </returns>
        /// <remarks>
        /// Compared to <see cref="OpenRawMemoryMapping"/>, this raw access performs much better.
        /// </remarks>
        public static RawMemoryMapping<T> OpenRawMemoryMapping(string path)
        {
            return RawMemoryMapping<T>.OpenRead(path);
        }
    }
}
