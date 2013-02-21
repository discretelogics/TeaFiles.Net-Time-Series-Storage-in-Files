// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TeaTime
{
    /// <summary>
    /// Access items in TeaFiles via unsafe raw memory mapping.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides access to the item area via memory mapping. <see cref="ItemAreaStart"/> holds a raw byte pointer to the 
    /// first item in the file, <see cref="ItemAreaEnd"/> a pointer past the last item. Such mapped access often provides 
    /// best performance if large (many MB) or very large files (several GB or more) are accessed.
    /// </para>
    /// <para>    
    /// While this class can boost performance it comes with some drawbacks:<br/>
    /// 1. Usage requires unsafe context and bugs might be much harder to detect in unsafe terrain.
    /// 2. The raw byte pointers must be casted to the item type in order to access the items.
    /// 3. The TeaTime environment encourages usage of the <seealso cref="Event{T}"/> template as item type. Pointers to this 
    /// type cannot be created however. Usage of this class is therefore restricted to plain structs.
    /// </para>    
    /// </remarks>
    public sealed unsafe class RawMemoryMapping<T> : IDisposable where T : struct
    {
        #region Construction

        /// <summary>
        /// Factory method to create a file mapping for read only access.
        /// </summary>
        /// <param name="fileName">The name of the file to open memory mapped.</param>
        /// <returns></returns>
        public static RawMemoryMapping<T> OpenRead(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            var mapping = new RawMemoryMapping<T>();
            mapping.teaFile = TeaFile<T>.OpenRead(fileName);

            try
            {
                FileStream fs = mapping.teaFile.Stream as FileStream;
                if (fs == null) throw new InvalidOperationException("TeaFile used for MemoryMapping is not a file stream but memory mapping requires a file.");

                mapping.mappingHandle = UnsafeNativeMethods.CreateFileMapping(
                    fs.SafeFileHandle,
                    IntPtr.Zero,
                    MapProtection.PageReadOnly,
                    0,
                    0,
                    null);
                if (mapping.mappingHandle.IsInvalid)
                {
                    throw new Win32Exception();
                }

                try
                {
                    mapping.mappingStart = UnsafeNativeMethods.MapViewOfFile(
                        mapping.mappingHandle,
                        MapAccess.FileMapRead,
                        0,
                        0,
                        IntPtr.Zero);
                    if (mapping.mappingStart == (byte*)0)
                    {
                        throw new Win32Exception();
                    }
                    mapping.itemAreaStart = mapping.mappingStart + mapping.teaFile.ItemAreaStart;
                    mapping.itemAreaEnd = mapping.itemAreaStart + mapping.teaFile.ItemAreaSize;
                }
                catch
                {
                    if (UnsafeNativeMethods.CloseHandle(mapping.mappingHandle) == 0)
                    {
                        // our view mapping failed above, but we still had a good mapping handle, so if 
                        // closing failed, we should check why
                        throw new Win32Exception();
                    }
                    mapping.mappingHandle = null;
                    throw;
                }
                return mapping;
            }
            catch
            {
                mapping.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Restrict instantiation to factory method.
        /// </summary>
        RawMemoryMapping()
        {
        }

        /// <summary>Releases all resources held by this instance.</summary>
        public void Dispose()
        {
            //  inside Dispose, exception are not allowed, so we trace errors
            try
            {
                if (this.mappingStart != null)
                {
                    if (UnsafeNativeMethods.UnmapViewOfFile((IntPtr)this.mappingStart) == 0)
                    {
                        Trace.WriteLine(UnsafeNativeMethods.GetLastWindows32ErrorMessage());
                    }
                    this.mappingStart = null;
                }
                if (this.mappingHandle != null)
                {
                    if (!this.mappingHandle.IsClosed)
                    {
                        if (UnsafeNativeMethods.CloseHandle(this.mappingHandle) == 0)
                        {
                            Trace.WriteLine(UnsafeNativeMethods.GetLastWindows32ErrorMessage());
                        }
                    }
                    this.mappingHandle.SetHandleAsInvalid();
                }
            }
            finally
            {
                this.teaFile.Dispose();
            }
        }

        #endregion

        #region Core

        /// <summary>Gets the item area start. </summary>
        /// <value>The start of the item area. If the file is not empty, this points to the first item in the file. </value>
        public byte* ItemAreaStart
        {
            get { return this.itemAreaStart; }
        }

        /// <summary>Gets the item area end. </summary>
        /// <value>The end of the item area. If the file is not empty, this points to the position where the next 
        /// item to append would be placed.</value>
        public byte* ItemAreaEnd
        {
            get { return this.itemAreaEnd; }
        }

        #endregion

        #region State

        TeaFile<T> teaFile;
        SafeFileHandle mappingHandle;

        byte* mappingStart;
        byte* itemAreaStart;
        byte* itemAreaEnd;

        #endregion
    }

    enum MapAccess
    {
        FileMapCopy = 0x0001,
        FileMapWrite = 0x0002,
        FileMapRead = 0x0004,
        FileMapAllAccess = 0x001f,
    }

    [Flags]
    enum MapProtection
    {
        PageNone = 0x00000000,
        // protection
        PageReadOnly = 0x00000002,
        PageReadWrite = 0x00000004,
        PageWriteCopy = 0x00000008,
        // attributes
        SecImage = 0x01000000,
        SecReserve = 0x04000000,
        SecCommit = 0x08000000,
        SecNoCache = 0x10000000,
    }

    static unsafe class UnsafeNativeMethods
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, MapProtection flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);

        [DllImport("kernel32", SetLastError = true)]
        public static extern byte* MapViewOfFile(SafeFileHandle hFileMappingObject, MapAccess dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumBytesToMap);

        [DllImport("kernel32", SetLastError = true)]
        public static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32", SetLastError = true)]
        public static extern int CloseHandle(SafeFileHandle handle);

        public static string GetLastWindows32ErrorMessage()
        {
            return new Win32Exception().Message;
        }
    }
}
