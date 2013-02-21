// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using TeaTime.Header;
using TeaTime.IO;

namespace TeaTime
{
    /// <summary>
    /// This class holds methods common required for untyped and typed reading.
    /// </summary>
    /// <remarks>
    /// This API provides 2 ways to access a TeaFile: <br></br>
    /// <list>
    /// <item><see cref = "TeaFile" /> for untyped reading of files whose content is unknown and</item>
    /// <item><see cref = "TeaFile{T}" /> for typed reading and writing of files.</item>
    /// </list>
    /// <br></br>
    /// <see cref="TeaFile{T}"/> does not derive from TeaFile as being the more powerful class, but they are 
    /// both plain root classes. The functionality reuiqred by both classes is embodied in <see cref="TeaFileCore"/> 
    /// that is instantiated as a member in <see cref="TeaFile"/> and <see cref="TeaFile{T}"/>.
    /// <see cref = "TeaFile" />
    /// </remarks>
    class TeaFileCore : IDisposable
    {
        #region Properties

        public TeaFileDescription Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets the number of items in the file.
        /// </summary>
        /// <remarks>
        /// This property is implemented internally, since access to items is 
        /// exposed by the Items property solely, to prived a cleaner API.
        /// </remarks>
        /// <value>The number of items.</value>
        public long Count
        {
            get { return this.ItemsAreaSize / this.description.ItemDescription.ItemSize; }
        }

        public Stream Stream
        {
            get { return this.stream; }
        }

        public long ItemAreaStart
        {
            get { return this.itemAreaStart; }
        }

        /// <summary>
        ///     Returns the file position that marks the end of the file. This will be either the end of the
        ///     file if the file is fully packed with items, or some position before the physical end, if 
        ///     disc space was preallocated.
        /// </summary>
        /// <remarks>
        ///     A TeaFile might be filled with items until the end of the file. In this case, 
        ///     the logical end of the file is equal to the physical file end. In order to allow
        ///     defragmentation of the disc, the file can allocate more physical space that will be
        ///     filled by later additions of items. In this case, the file header will hold the 
        ///     end of the area of the file that is already filled with items.
        /// </remarks>
        public long ItemAreaEnd
        {
            get
            {
                if (this.itemAreaEnd == 0) return this.stream.Length;
                return this.itemAreaEnd;
            }
        }

        public long ItemAreaEndMarker
        {
            get { return this.itemAreaEnd; }
        }

        /// <summary>
        /// ItemAreaEndMarker is set and there is space between the marker and the end of the file.
        /// </summary>
        public bool HasPreallocatedSpace
        {
            get { return this.itemAreaEnd != 0 && this.itemAreaEnd < this.stream.Length; }
        }

        public long ItemsAreaSize
        {
            get { return this.ItemAreaEnd - this.itemAreaStart; }
        }

        public bool CanAppend
        {
            get { return !this.HasPreallocatedSpace; }
        }

        public string Name
        {
            get
            {
                if (this.stream is FileStream)
                {
                    return ((FileStream)this.stream).Name;
                }
                throw new InvalidOperationException("TeaFile was not created from a FileStream instance or filename. Name is therefore not available.");
            }
        }

        #endregion

        #region public Methods

        public TeaFileCore(Stream stream, bool ownsStream)
        {
            this.stream = stream;
            this.ownsStream = ownsStream;
            this.description = new TeaFileDescription();
        }

        public void Dispose()
        {
            this.Destruct();
            GC.SuppressFinalize(this);
        }

        public void IsAccessibleWith(ItemDescription accessorDescription, ItemDescriptionElements elements)
        {
            // without descriptions in the file we cannot check anything. No warning will be issued in such case, 
            // the user is responsible to check that files have descriptions if desired.
            if (this.Description == null) return;
            if (this.Description.ItemDescription == null) return;

            var fileDescription = this.Description.ItemDescription;
            fileDescription.IsAccessibleWith(accessorDescription, elements);
        }

        public void ReadHeader()
        {
            FileIO fio = new FileIO(this.stream);
            FormattedReader r = new FormattedReader(fio);
            var rc = HeaderManager.Instance.ReadHeader(r);
            this.description = rc.Description;
            this.itemAreaStart = rc.ItemAreaStart;
            this.itemAreaEnd = rc.ItemAreaEnd;
            if (this.stream.Position != this.itemAreaStart)
            {
                throw new InternalErrorException("file position is not set to begin of item area after reading header.");
            }
            if (this.Description != null && this.Description.Timescale.HasValue)
            {
                var fileTimeScale = this.Description.Timescale.Value;
                if (!fileTimeScale.Equals(Time.Scale))
                {
                    switch (Time.ScaleCollisionBehavior)
                    {
                    case ScaleCollisionBehavior.ThrowException:
                        throw new TimescaleException();

                    case ScaleCollisionBehavior.Ignore:
                        break;

                    case ScaleCollisionBehavior.UseNewScale:
                        Time.Scale = fileTimeScale;
                        break;
                    }
                }
            }
        }

        public void WriteHeader()
        {
            var wc = HeaderManager.Instance.WriteHeader(new FormattedWriter(new FileIO(this.stream)), this.description);
            this.itemAreaStart = wc.ItemAreaStart;
        }

        public void Assign(long itemAreaStart, long itemAreaEnd)
        {
            this.itemAreaStart = itemAreaStart;
            this.itemAreaEnd = itemAreaEnd;
        }

        /// <summary>
        /// Most operations of this class require the file to hold a description of it's items. To ensure this,
        /// the methods can call this method which will throw <see cref = "NotSupportedException" /> if no such 
        /// description is available.
        /// </summary>
        public void ItemDescriptionExists()
        {
            if (this.description == null)
                throw new InvalidOperationException("Cannot access items because this file has no description");
            if (this.description.ItemDescription == null)
                throw new InvalidOperationException("Cannot access items because this file has no description of the Items contained");
        }

        public void SetFilePointerToItem(long item)
        {
            long position = this.ItemAreaStart + this.description.ItemDescription.ItemSize * item;
            this.stream.Seek(position, SeekOrigin.Begin);
        }

        public void SetFilePointerToEnd()
        {
            this.stream.Seek(0, SeekOrigin.End);
        }

        /// <summary>
        /// Truncates the file, logically removing all items, physically downsizing it to the header's size.
        /// </summary>
        public void Truncate()
        {
            this.stream.SetLength(this.itemAreaStart);
            this.itemAreaEnd = 0;
            this.stream.Position = 8;
            var bw = new BinaryWriter(this.stream);
            bw.Write(this.itemAreaEnd);
            this.stream.Position = this.itemAreaStart;
        }

        public void Destruct()
        {
            if (this.ownsStream && this.stream != null)
            {
                this.stream.Dispose();
            }
        }

        #endregion

        #region State

        Stream stream;
        bool ownsStream;
        long itemAreaStart;
        long itemAreaEnd;
        TeaFileDescription description;

        #endregion
    }
}
