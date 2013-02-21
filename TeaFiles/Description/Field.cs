// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime
{
    /// <summary>
    /// Defines the field of a struct holding the event time.
    /// </summary>
    /// <remarks>
    /// An event describes a value at a certain time. This can be represented by a struct holding the properties of the value
    /// and an additional time value. The prefered way to add such time value is to use <see cref="Event{T}"/>. Alternatively,
    /// a struct can hold the value and the time value together.
    /// 
    /// <b>Preferred: use Event{T}</b>
    /// <code>
    /// struct Tick
    /// {
    ///     public double Price;
    ///     public int Volume;
    /// }
    /// Event&lt;Tick&gt;<br></br>
    /// </code>
    /// <b>Alternative:</b>
    /// <code>
    /// struct Tick2
    /// {
    ///     public Time Time;
    ///     public double Price;
    ///     public int Volume;
    /// }
    /// </code>
    /// In the latter case, Tick2 holds a field of type <see cref="Time"/>. This API <b>assumes that the first <see cref="Time"/>
    /// value in a type used for TeaFiles is the event time.</b> Due to this implicit assumption, the <see cref="EventTimeAttribute"/>
    /// is not necessary in this case. If however more than a single field holds a <see cref="Time"/> field and not the first field 
    /// shall be the event time, then putting this attribute on the event time field allows to express that.
    /// <example>
    /// <code>
    /// struct FeedMeasurement
    /// {
    ///     public Time Time;
    ///     public double Price;
    ///     public int Volume;
    ///     [EventTime]
    ///     public Time ArrivalTime;
    /// }
    /// </code>
    /// </example>
    /// In this example, the arrival time it the time at which an event, already holding the time stamp of a distant clock, arrives 
    /// on the local machine, where it gets the time stamp of the local clock into its field ArrivalTime. The latter shall be the event 
    /// time. To declare this, the <see cref="EventTimeAttribute"/> must be applied to the field.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EventTimeAttribute : Attribute
    {
    }

    /// <summary>
    /// Describes the field of an item.
    /// </summary>
    /// <remarks>
    /// TeaFiles store homogenous collections of items. Each item (.Net technically a struct) is a structured value 
    /// composed from fields. This class describes a single field.
    /// </remarks>
    public class Field
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The field's byte offset inside the struct.
        /// </summary>
        public Int32 Offset { get; set; }

        /// <summary>
        /// The type of the field. The struct must hold only types of the TeaType enumeration to be accessible from 
        /// other APIs. If one or more fields have the TeaType = <see cref="FieldType" />.Custom, the TeaFile will not be accessible
        /// universally.
        /// </summary>
        public FieldType FieldType { get; set; }

        /// <summary>
        /// The field represents a time.
        /// </summary>
        /// <remarks>
        /// A field holding a time value will have a scalar type, like <see cref="ulong"/>. So from this data type it can not 
        /// be deferred that the field shall be interpreted as time. TeaFiles therefore describe those fields to be interpreted 
        /// as time in their Time Section. When a TeaFile is read, this property receives it's value from the Time Section and 
        /// signals that the field value is a time. When reading untyped using <see cref="TeaFile"/>, the field value is 
        /// automatically converted into a <see cref="Time"/> value.
        /// </remarks>
        public bool IsTime { get; set; }

        /// <summary>
        /// The field is the event time of the event as part of the time series.
        /// </summary>
        /// <remarks>
        /// Not every field holding time represents the event time. Example:
        /// A data feed delivers trade prices together with a time at which the trades occured. A local subscriber 
        /// to this feed wants to measure the delay at which this data arrives. To do so, an event is defined as:
        /// <code>
        /// struct ArrivalEvent
        /// {
        ///     DateTime    TradeTime;
        ///     double      Price;
        ///     [EventTime]
        ///     DateTime    ArrivalTime;
        /// }
        /// </code>
        /// This struct holds 2 time fields but only one of them can hold the event time. The event time must have 
        /// non-decreassing values (each value is >= the previous value) which is certainly the case for the ArrivalTime field. 
        /// If the data feed guarantees delivery in temporal order, the TradeTime field could also be used 
        /// as the event time field. If a struct holds more than one DateTime fields, one field must be specified 
        /// as the event time, using <see cref="EventTimeAttribute"/>.
        /// </remarks>
        public bool IsEventTime { get; set; }

        /// <summary>
        /// Describes the order of this field inside the fields collection.
        /// </summary>
        /// <remarks>
        /// This value is used internally and made public because it might be useful.
        /// </remarks>
        public int Index { get; set; }

        /// <summary>Gets the value of this field from an item.</summary>
        /// <exception cref="ArgumentNullException"> The item is null.</exception>
        /// <param name="item">The item.</param>
        /// <returns>The value.</returns>
        public object GetValue(Item item)
        {
            if (item == null) throw new ArgumentNullException("item");

            return item.Values[this.Index];
        }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </returns>
        public override string ToString()
        {
            return this.FieldType + " " + this.Offset + " " + this.Name.SafeToString();
        }
    }
}
