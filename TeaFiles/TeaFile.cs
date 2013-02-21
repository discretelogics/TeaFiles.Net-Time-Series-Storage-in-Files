// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using System.Linq;

namespace TeaTime
{
    /// <summary>
    /// Read TeaFiles without knowing anything about their content (untyped reading).
    /// </summary>
    /// <remarks>
    /// Access is limited to read, because we do not have any knowledge about the file's item type. Writing
    /// of file is provided by <see cref="TeaFile{T}"/> only.
    /// </remarks>
    public sealed class TeaFile : IDisposable, IItemReader
    {
        #region Construction

        /// <summary>
        /// Restrict construction to static factory methods.
        /// </summary>
        TeaFile()
        {
        }

        /// <summary>
        /// Open a TeaFile for reading (untyped).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null. </exception>
        /// <param name="path">The path of the file. </param>
        /// <returns>An instance of TeaFile. </returns>
        public static TeaFile OpenRead(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return OpenRead(stream, true);
        }

        /// <summary>
        /// Open a TeaFile for reading (untyped).
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null. </exception>
        /// <param name="stream">A stream of the file. This might be a file, or another implementation of <see cref="Stream"/>, for instance a <see cref="MemoryStream"/>. </param>
        /// <returns>An instance of TeaFile. </returns>
        public static TeaFile OpenRead(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return OpenRead(stream, false);
        }

        static TeaFile OpenRead(Stream stream, bool ownsStream)
        {
            var tf = new TeaFile();
            try
            {
                tf.core = new TeaFileCore(stream, ownsStream);
                tf.core.ReadHeader();

                //	protect against empty Item structs
                if (tf.Description.ItemDescription != null)
                {
                    if (!tf.Description.ItemDescription.Fields.Any())
                    {
                        throw new ItemException("Cannot read this file because the item has no fields according to its item description in the file.");
                    }
                }

                tf.reader = new BinaryReader(tf.core.Stream);

                if (tf.Description.ItemDescription != null)
                {
                    // A field might occupy equal or more bytes than the size of its type due to padding bytes
                    // inserted between the fields or between items in the ItemArray. The paddingBytes array
                    // holds the number of bytes that will be read after reading a field in order to set the
                    // file pointer to the next field.
                    tf.fieldSpacings = tf.GetFieldSpacings();
                }

                return tf;
            }
            catch
            {
                tf.Dispose(); // since we will not hand out the disposable TeaFile instance,
                // we must close the filestream if header reading failed!
                throw;
            }
        }

        #endregion

        #region Description

        /// <summary>
        /// The description of the file content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dependend of the description inside the file, the description will hold information 
        /// about the item type, its layout or further description.
        /// </para>
        /// </remarks>
        /// <value>The description.</value>
        public TeaFileDescription Description
        {
            get { return this.core.Description; }
        }

        /// <summary>The name of the file. </summary>
        /// <value>The name. </value>
        /// <remarks>
        /// The name equals the path of the file.		 
        /// </remarks>
        public string Name
        {
            get { return this.core.Name; }
        }

        #endregion

        #region Items

        /// <summary>
        /// The items in the file.
        /// </summary>
        /// <remarks>
        /// The <see cref="Items"/> instance returned exposes an enumerator over the items in the file,
        /// where each item is exposed as <see cref="Item"/>.
        /// </remarks>
        public IItemCollection Items
        {
            get
            {
                if (this.itemCollection == null)
                {
                    this.core.ItemDescriptionExists();
                    this.itemCollection = new ItemCollection(this);
                }
                return this.itemCollection;
            }
        }

        /// <summary>Reads the next item from the file. </summary>
        /// <returns>The item. </returns>
        /// <remarks>The file pointer defines which item will be read. Use </remarks>
        public Item Read()
        {
            this.core.ItemDescriptionExists();

            var item = this.CreateItem();
            int i = 0;
            foreach (Field f in this.Description.ItemDescription.Fields)
            {
                this.fieldSpacings[i].Times(() => this.reader.ReadByte());
                var value = this.reader.Read(f.FieldType);

                // convert to Time if field is a time
                if (f.IsTime && this.core.Description.Timescale.HasValue)
                {
                    value = new Time((Int64)value);
                }

                item.Values[i] = value;
                i++;
            }
            this.fieldSpacings[i].Times(() => this.reader.ReadByte());
            return item;
        }

        bool IItemReader.CanRead
        {
            get { return this.core.Stream.Position < this.core.ItemAreaEnd; }
        }

        /// <summary>Gets the value of <paramref name="field"/> of the item at index <paramref name="itemIndex"/>. </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="itemIndex">Zero-based index of the item. </param>
        /// <param name="field">The field. </param>
        /// <returns>The field value. </returns>
        public object GetFieldValue(int itemIndex, Field field)
        {
            if (field == null) throw new ArgumentNullException("field");

            var item = this.itemCollection[itemIndex];
            return this.GetFieldValue(item, field);
        }

        /// <summary>Gets the value of <paramref name="field"/> of <paramref name="item"/>. </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="item">The item. </param>
        /// <param name="field">The field. </param>
        /// <returns>The field value. </returns>
        public object GetFieldValue(Item item, Field field)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (field == null) throw new ArgumentNullException("field");

            return item.Values[field.Index];
        }

        /// <summary>
        /// Opens the file, fetches the description, closes the file and returns the description.
        /// </summary>
        /// <remarks>
        /// The file must be readable, so if it is open by some other process, it must have shareable read state.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static TeaFileDescription GetDescription(string fileName)
        {
            using (var tf = OpenRead(fileName))
            {
                return tf.Description;
            }
        }

        /// <summary>
        /// Return the Event Time if the <paramref name="item"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The file holds no ItemDescription.</exception>
        /// <exception cref="InvalidOperationException">The ItemDescription does not identify a field as event time.</exception>
        /// <param name="item">The item</param>
        /// <returns>The value of the item's time field.</returns>
        public Time GetEventTime(Item item)
        {
            if (this.Description.ItemDescription == null) throw new InvalidOperationException("Cannot read event time, because the file contains no item description.");
            var eventField = this.Description.ItemDescription.EventTimeField;
            if(eventField == null) throw new InvalidOperationException("No field is known as Event Time.");
            return (Time)this.GetFieldValue(item, eventField);
        }

        /// <summary>Gets the item area start, as byte offset from the begin of the file. </summary>
        public long ItemAreaStart
        {
            get { return this.core.ItemAreaStart; }
        }

        /// <summary>Gets the item area end, as byte offset from the begin of the file. </summary>
        /// <remarks>This points to the position in the file past the last item, which is the end of the file if no preallocation is used.</remarks>
        public long ItemAreaEnd
        {
            get { return this.core.ItemAreaEnd; }
        }

        /// <summary>Gets the size of the item area in bytes. </summary>
        public long ItemAreaSize
        {
            get { return this.core.ItemsAreaSize; }
        }

        #endregion

        #region IDisposable & Finalizer

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.core.Dispose();
            if (this.reader != null)
            {
                this.reader.Dispose(); // actually a noop, closes its stream, which we did ourselves in the line above
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="TeaFile"/> is reclaimed by garbage collection.
        /// </summary>
        ~TeaFile()
        {
            this.core.Destruct();
        }

        #endregion

        #region Internals

        long IItemReader.Count
        {
            get { return this.core.Count; }
        }

        //internal Stream Stream
        //{
        //    get { return this.core.Stream; }
        //}

        void IItemReader.SetFilePointerToItem(int i)
        {
            this.core.SetFilePointerToItem(i);
        }

        internal Item CreateItem()
        {
            return new Item(this.Description.ItemDescription.Fields.Count);
        }

        /// <summary>
        /// computes the byte spaces between 
        /// </summary>
        internal int[] GetFieldSpacings()
        {
            var fields = this.Description.ItemDescription.Fields;
            var spaceBeforeField = new int[fields.Count + 1];
            spaceBeforeField[0] = fields[0].Offset;
            int i = 0;
            while (i <= fields.Count - 2) //  iterate over fields from first to second last
            {
                Field field = fields[i];
                Field nextField = fields[i + 1];
                var totalFieldSpace = nextField.Offset - field.Offset;
                spaceBeforeField[i + 1] = totalFieldSpace - field.FieldType.GetSize();
                i++;
            }

            //  the padding of the last field is specified by its (total)offset and the size of the item
            spaceBeforeField[fields.Count] = this.Description.ItemDescription.ItemSize -
                                             (fields[i].Offset + fields[i].FieldType.GetSize());
            return spaceBeforeField;
        }

        #endregion

        #region State

        TeaFileCore core;
        IItemCollection itemCollection;
        BinaryReader reader;
        int[] fieldSpacings;

        #endregion
    }
}
