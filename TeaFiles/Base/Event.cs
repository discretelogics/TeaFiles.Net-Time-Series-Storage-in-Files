// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime
{
    /// <summary>
    /// Represents an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An event describes a value at a certain time. This value might be an observation
    /// like the temperature in Costa Rica, the price of a transaction or an assumed value like number of people waiting in a queue computed by a simulation.
    /// Such value might be represented by a scalar value, like a <see cref="double"/> for a temperature or require a 
    /// structured value like a "Tick" holding price and volume. While the type of such value differs, ab evene always associates it with 
    /// a time value. This class abstracts this time property.
    /// </para>
    /// <para>
    /// Instead of using<br></br>
    /// <code>
    /// struct Tick
    /// {
    ///     [EventTime]
    ///     public Time Time;
    ///     public double Price;
    ///     public int Volume;
    /// }
    /// </code>
    /// Event&lt;T&gt; allows to abbreviate
    /// <code>
    /// struct Tick
    /// {
    ///     public double Price;
    ///     public int Volume;
    /// }
    /// // and then use
    /// Event&lt;Tick&gt;
    /// </code>
    /// This has several advantages:
    /// <list>
    /// <item>Processing frameworks can isolate the value from <see cref="Event{T}"/> or construct timestamped instances from values.</item>
    /// <item>The fact that a type is an event is made explicit in code</item>
    /// <item>It is shorted to write, <see cref="EventTimeAttribute"/> is packed into <see cref="Event{T}"/>,</item>
    /// <item>The type used for time repressentation is encapsulated in a single class.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the event value.</typeparam>
    public struct Event<T> where T : struct
    {
        /// <summary>
        /// Constructor, this overload is often handy to create instances.
        /// </summary>
        /// <param name="time">The event time.</param>
        /// <param name="value">The event value.</param>
        public Event(Time time, T value)
        {
            this.Time = time;
            this.Value = value;
        }

        /// <summary>
        /// The time of the event.
        /// </summary>
        /// <remarks>
        /// The <see cref="Time"/> type supports flexible epoch and resolution values.<br/>
        /// </remarks>
        [EventTime]
        public Time Time;

        /// <summary>
        /// The event value.
        /// </summary>
        /// <remarks>Dependent on the application domain, this might be a scalar value or a struct.</remarks>
        public T Value;

        /// <summary>Returns the fully qualified type name of this instance. </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name. </returns>
        public override string ToString()
        {
            return "{0}\t{1}".Formatted(this.Time, this.Value);
        }
    }
}
