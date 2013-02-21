// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace TeaTime
{
    /// <summary>Describes the type of items stored in a TeaFile.</summary>
    /// <remarks>
    /// <para>
    /// This description is infered from the generic argument of
    /// a <see cref="TeaFile{T}"/> and persisted into the file when a new file is created. This description allows the file to be read without 
    /// having knowledge about its content, thus making it <b>self contained</b>.
    /// </para>
    /// <h4>Creation of a TeaFile</h4>
    /// <para>
    /// When a file is written using <see cref="TeaFile{T}" />, an ItemDescription is created based on type reflection and additional analysis
    /// to get the field offsets. This description is persisted inside the file.
    /// </para>
    /// <h4>Typed Reading</h4>
    /// <para>
    /// When a file is read using <see cref="TeaFile{T}" />, the ItemDesciption is created from reflection and analysis exactly as during creation.
    /// The file however holds another description that was persisted during creation. These descriptions should match in order to return correct values.
    /// <see cref="TeaFile{T}"/>.OpenRead() will compare them to ensure that. If they do not match, a <see cref="TypeMismatchException"/> is thrown.
    /// </para>
    /// <h4>Untyped Reading</h4>
    /// <para>
    /// When a file is read with a <see cref="TeaFile"/>, the description is read from the file. The TeaFile's <see cref="ItemDescription"/> property will 
    /// return this description and its <see cref="ItemDescription.Source"/> property will indicate that it was read from the file header.
    /// </para>
    /// </remarks>
    public class ItemDescription
    {
        #region Construction

        internal ItemDescription(DescriptionSource source)
        {
            this.fields = new List<Field>();
            this.source = source;
        }

        /// <summary>
        /// Returns an ItemDescription based on reflection and analysis of <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="ItemException">
        /// If <typeparamref name="T"/> is an empty struct, an <see cref="ItemException"/> is thrown. This is a measure against 
        /// TeaFiles holding empty structs which makes no sense at first and introduces difficulties better to be avoided. Empty structs 
        /// are for instance forbidden by C syntax, or have the mysterious size 1 in C++ or C#.
        /// </exception>
        /// <typeparam name="T">The itemType to be analyzed and described.</typeparam>
        /// <returns>An ItemDescription for type <typeparamref name="T"/> </returns>
        internal static ItemDescription FromAnalysis<T>( /* ItemDescriptionDetails */) where T : struct
        {
            using (var la = new LayoutAnalyzer<T>())
            {
                List<AnalyzedField> analyzedFields = la.AnalyzeLayout();
                if (!analyzedFields.Any())
                {
                    throw new ItemException("The item '{0}' has no fields. Creation of TeaFiles with empty structs is not supported.".Formatted(typeof (T).FullName));
                }
                return FromAnalysis(analyzedFields, la.TypeSize, typeof (T).GetLanguageName());
            }
        }

        /// <summary>
        /// Creates an <see cref="ItemDescription"/> from the arguments provided.
        /// </summary>
        /// <param name="analyzedFields">A Dictionary holding the fields and their offsets.</param>
        /// <param name="size">The size of the item type.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <returns></returns>
        static ItemDescription FromAnalysis(IEnumerable<AnalyzedField> analyzedFields, Int32 size, string typeName)
        {
            ItemDescription id = new ItemDescription(DescriptionSource.ItemType);
            id.itemSize = size;
            id.itemTypeName = typeName;
            if (analyzedFields != null)
            {
                foreach (var af in analyzedFields.OrderBy(fo => fo.Offset))
                {
                    FieldInfo fi = af.FieldPath.Last;

                    var f = id.NewField();
                    f.FieldType = fi.FieldType.GetFieldType();
                    f.Offset = af.Offset;
                    f.Name = af.Name;
                    f.IsTime = fi.FieldType == typeof (Time);
                    f.IsEventTime = fi.IsDefined<EventTimeAttribute>();
                }
                // if the EventTimeAttribute has not been set on any field, use the first time field as event time
                if (id.EventTimeField == null)
                {
                    var f = id.Fields.FirstOrDefault(ff => ff.IsTime);
                    if (f != null)
                    {
                        f.IsEventTime = true;
                    }
                }
            }
            return id;
        }

        #endregion

        #region Core

        /// <summary>
        /// Gets the collection of fields describing the item's fields.
        /// </summary>
        /// <value>The fields.</value>
        public ReadOnlyCollection<Field> Fields
        {
            get { return this.fields.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the size of the items stored in the TeaFile.
        /// </summary>
        /// <value>The size of the item.</value>
        public Int32 ItemSize
        {
            get { return this.itemSize; }
            internal set { this.itemSize = value; }
        }

        /// <summary>
        /// Gets the name of the Item type.
        /// </summary>
        /// <remarks>
        /// Example: "Event&lt;Tick&gt;".
        /// </remarks>
        /// <value>The name of the item type.</value>
        public string ItemTypeName
        {
            get { return this.itemTypeName; }
            internal set { this.itemTypeName = value; }
        }

        /// <summary>
        /// The source of the ItemDescription.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An ItemDescription can be created by 
        /// <list>
        /// <item>analyzing the item type of a TeaFile or</item>
        /// <item>by reading it from a TeaFile.</item>
        /// </list>
        /// This property describes which of the above origins an ItemDescription instance has.
        /// </para>
        /// </remarks>
        /// <value><see cref="DescriptionSource.ItemType"/> indicates that the instance was created from analzing the itemm type.<br/>
        /// <see cref="DescriptionSource.File"/> indicates that the instance was read from file.
        /// </value>
        internal DescriptionSource Source
        {
            get { return this.source; }
        }

        /// <summary>Indicating whether the type has an event time. </summary>
        /// <value>true if the item has an event time, false if not. </value>
        public bool HasEventTime
        {
            get { return this.fields.Any(f => f.IsEventTime); }
        }

        /// <summary>
        /// Checks it the access description can access a time series based on this ItemDescription.
        /// </summary>
        /// <remarks>
        /// policies:<br></br>
        /// If <paramref name="accessorDescription"/> has no fields, the check will always succeed.<br></br>
        /// 
        /// </remarks>
        /// <param name="accessorDescription">ItemDescription that describes the type used to access the file.</param>
        /// <param name="elementsToConsider">The amount of details used for the test.</param>
        /// <exception cref="TypeMismatchException">If the accessor type is not suitable to access the file.</exception>
        public void IsAccessibleWith(ItemDescription accessorDescription, ItemDescriptionElements elementsToConsider)
        {
            if (elementsToConsider.HasFlag(ItemDescriptionElements.ItemName))
            {
                if (this.ItemTypeName != accessorDescription.itemTypeName)
                {
                    throw new TypeMismatchException("ItemNames do not match: '{0}' vs '{1}'".Formatted(this.ItemTypeName, accessorDescription.ItemTypeName), "ItemName Check");
                }
            }
            if (elementsToConsider.HasFlag(ItemDescriptionElements.ItemSize))
            {
                if (this.ItemSize != accessorDescription.ItemSize)
                {
                    throw new TypeMismatchException("ItemSizes do not match: {0} vs {1}".Formatted(this.ItemSize, accessorDescription.ItemSize), "ItemSize Check");
                }
            }

            if (elementsToConsider.HasFlag(ItemDescriptionElements.FieldOffsets))
            {
                if (accessorDescription == null) throw new ArgumentNullException("accessorDescription");
                if (!this.fields.Any()) throw new TypeMismatchException("No fields are available in {0} to check field offsets. Empty structs are not supported by this API.".Formatted(this.Source.ToString()), "No ItemFields");
                if (!accessorDescription.fields.Any()) throw new TypeMismatchException("No fields are available in {0} to check field offsets. Empty structs are not supported by this API.".Formatted(accessorDescription.Source.ToString()), "No ItemFields Accessor");

                var f = this.Fields.FirstOrDefault(ff => accessorDescription.FindFieldByOffset(ff.Offset) == null);
                if (f != null)
                {
                    throw new TypeMismatchException("Field has no field with matching byte offset:'{0}'".Formatted(f), "FieldOffsets Check");
                }
            }
            if (elementsToConsider.HasFlag(ItemDescriptionElements.FieldNames))
            {
                if (accessorDescription == null) throw new ArgumentNullException("accessorDescription");
                // ItemDescriptionDetails.FieldNames includes ItemDescriptionDetails.FieldOffsets by the definition of ItemDescriptionDetails values!
                var f = this.Fields.FirstOrDefault(ff => accessorDescription.FindFieldByOffset(ff.Offset).Name != ff.Name);
                if (f != null)
                {
                    throw new TypeMismatchException("Fields have different names: '{0}' vs '{1}'".Formatted(f, accessorDescription.FindFieldByOffset(f.Offset).Name), "FieldNames Check");
                }
            }
            if (elementsToConsider.HasFlag(ItemDescriptionElements.FieldTypes))
            {
                if (accessorDescription == null) throw new ArgumentNullException("accessorDescription");
                // ItemDescriptionDetails.FieldNames includes ItemDescriptionDetails.FieldOffsets by the definition of ItemDescriptionDetails values!
                var f = this.Fields.FirstOrDefault(ff => accessorDescription.FindFieldByOffset(ff.Offset).FieldType != ff.FieldType);
                if (f != null)
                {
                    throw new TypeMismatchException("Fields have different types: '{0}' vs '{1}'".Formatted(f, accessorDescription.FindFieldByOffset(f.Offset).Name), "FieldTypes Check");
                }
            }
        }

        internal Field NewField()
        {
            var f = new Field();
            f.Index = this.fields.Count;
            this.fields.Add(f);
            return f;
        }

        /// <summary>Searches a field by its offset. </summary>
        /// <param name="offset">The offset. </param>
        /// <returns>The field at <paramref name="offset"/>. Null if no field was found. </returns>
        public Field FindFieldByOffset(int offset)
        {
            return this.fields.FirstOrDefault(f => f.Offset == offset);
        }

        /// <summary>Gets a field by name. </summary>
        /// <exception cref="ArgumentNullException">    Thrown when name is null. </exception>
        /// <exception cref="InvalidOperationException">Thrown when no field with <paramref name="name"/> exists. </exception>
        /// <param name="name">The name. </param>
        /// <returns>The field with name <paramref name="name"/>. </returns>
        public Field GetFieldByName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            try
            {
                return this.fields.First(f => f.Name == name);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No field with {0} exists.".Formatted(name), ex);
            }
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(Environment.NewLine);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <remarks>This overload allows specificion of the delimiter between the string for each field.</remarks>
        /// <param name="delimiter">The delimeter.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string delimiter)
        {
            return "{0} {1} {2} fields:{3}{4}".Formatted(this.itemTypeName, this.itemSize, this.fields.Count,
                                                         delimiter,
                                                         this.fields.Select(f => string.Format("{0}, {1}, {2}", f.Name.SafeToString("unnamed"), f.Offset, f.FieldType)).Joined(delimiter));
        }

        #endregion

        #region Item Formatting

        /// <summary>
        /// When an arbitrary TeaFile is read, and the file holds a description, item's can be read but there is no type
        /// available that represents them. In this case, the item's fields are stored in the values collection
        /// of an <see cref="Item" /> instance. This Print method will print such item's values, separated by blanks.
        /// </summary>
        /// <param name="item">The untyped item.</param>
        public string GetValueString(Item item)
        {
            return this.GetValueString(item, " ");
        }

        /// <summary>
        /// When an arbitrary TeaFile is read, and the file holds a description, item's can be read but there is no type
        /// available that represents them. In this case, the item's fields are stored in the values collection
        /// of an <see cref="Item" /> instance. This Print method will print such item's values, seperated by <paramref name="separator"/>.
        /// </summary>
        /// <param name="item">The untyped item.</param>
        /// <param name="separator">The separator between the fields.</param>
        public string GetValueString(Item item, string separator)
        {
            if (item == null) throw new ArgumentNullException("item");
            return string.Join(separator, this.Fields.Select(f => item.Values[f.Index]).ToArray());
        }

        #endregion

        #region Item Access

        /// <summary>
        /// Returns the field that holds the event time.
        /// </summary>
        /// <remarks>The implementation searches through available fields upon the first call and then stores it internally (Lazy evaluation).</remarks>
        public Field EventTimeField
        {
            get
            {
                if (this.eventTimeField == null)
                {
                    this.eventTimeField = this.fields.FirstOrDefault(f => f.IsEventTime);
                }
                return this.eventTimeField;
            }
        }

        #endregion

        #region State

        DescriptionSource source;
        Int32 itemSize;
        string itemTypeName;
        List<Field> fields;
        Field eventTimeField; // for efficiency purpose this field is fetched once

        #endregion
    }
}
