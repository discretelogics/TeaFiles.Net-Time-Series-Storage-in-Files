// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Diagnostics;

// For guidelines regarding the creation of new exception types, see
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp

namespace TeaTime
{
    /// <summary>The item type violates constraints.</summary>
    /// <remarks>For instance, the item must expose fields that match any <see cref="FieldType"/>.</remarks>
    [Serializable]
    [DebuggerNonUserCode]
    public class ItemException : Exception
    {
        /// <summary>Constructor.</summary>
        public ItemException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message.</param>
        public ItemException(string message) : base(message)
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public ItemException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>The header of a TeaFile has an invalid format. </summary>
    [Serializable]
    [DebuggerNonUserCode]
    public sealed class FileFormatException : Exception
    {
        /// <summary>Constructor. </summary>
        public FileFormatException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        public FileFormatException(string message) : base(message)
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        /// <param name="inner">The inner exception. </param>
        public FileFormatException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>A TeaFile is read with a type that does not match the type it was written with. </summary>
    /// <remarks>
    /// A TeaFile holds a description of the item type that was used to create the file. Subsequent writes and reads
    /// should use exactly the same type.
    /// </remarks>
    [Serializable]
    [DebuggerNonUserCode]
    public sealed class TypeMismatchException : Exception
    {
        /// <summary>Constructor. </summary>
        public TypeMismatchException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        /// <param name="source"> Source for the. </param>
        public TypeMismatchException(string message, string source) : base(message)
        {
            base.Source = source;
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        /// <param name="inner">The inner exception. </param>
        public TypeMismatchException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// An internal error occured.
    /// </summary>
    /// <remarks>
    /// Such exception should indicates a programming error inside the TeaFile API. If 
    /// you encounter such exception please forward steps to reproduce it to the 
    /// api authors.
    /// </remarks>
    [Serializable]
    [DebuggerNonUserCode]
    public class InternalErrorException : Exception
    {
        /// <summary>Constructor. </summary>
        public InternalErrorException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        public InternalErrorException(string message) : base(message)
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        /// <param name="inner">The inner exception. </param>
        public InternalErrorException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>The current default Timescale differs from the Timescale used in a file.</summary>
    [Serializable]
    [DebuggerNonUserCode]
    public sealed class TimescaleException : Exception
    {
        /// <summary>Constructor. </summary>
        public TimescaleException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        public TimescaleException(string message) : base(message)
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        /// <param name="inner">The inner exception. </param>
        public TimescaleException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>The field of an item is of a type that can not be stored in a TeaFile. </summary>
    [Serializable]
    [DebuggerNonUserCode]
    public sealed class InvalidFieldTypeException : Exception
    {
        /// <summary>Constructor.</summary>
        public InvalidFieldTypeException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The message. </param>
        public InvalidFieldTypeException(string message) : base(message)
        {
        }
    }

    /// <summary>The object has an invalid state. </summary>
    /// <remarks>
    /// Classes hide internal state and ensure that this state adheres to class invariants. If an operation 
    /// encounters a violation of these invariants, it can raise this exception. Usually this operation will 
    /// not be able to report when the state was corrputed but only the fact that it has been corrupted.
    /// </remarks>
    [Serializable]
    [DebuggerNonUserCode]
    public class InvalidStateException : Exception
    {
        /// <summary>Constructor. </summary>
        public InvalidStateException()
        {
        }

        /// <summary>Constructor. </summary>
        /// <param name="message">The error message. </param>
        public InvalidStateException(string message) : base(message)
        {
        }

        /// <summary>Constructor.</summary>
        /// <param name="message">The error message. </param>
        /// <param name="inner">The inner exception. </param>
        public InvalidStateException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
