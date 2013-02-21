// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Diagnostics;

namespace TeaTime
{
    ///<summary>
    /// Describes the behavior upon <see cref="Timescale"/> collision.
    ///</summary>
    /// <remarks>
    /// Each TeaFile holds a <see cref="Timescale"/> value that defines how to interpret tick counts int the file.
    /// For performance and simplicity reasons, this API a maintains a single process wide <see cref="Timescale"/> 
    /// value in <see cref="Time.Scale"/> that allows simple conversion of time values like <see cref="Time.Day"/>. 
    /// When a TeaFile is opened and it's Timescale conflicts with the default Timescale, then the <see cref="ScaleCollisionBehavior"/>
    /// defines how to behave.
    /// </remarks>
    public enum ScaleCollisionBehavior
    {
        /// <summary>Throw a <see cref="TimescaleException"/>. </summary>
        ThrowException,

        /// <summary>Ignore the collision. </summary>
        Ignore,

        /// <summary>Set the default scale to the new scale. </summary>
        UseNewScale
    }

    /// <summary>This class is designed after <see cref="DateTime"/> but allows configurable epoch and resolution via its 
    /// <see cref="Scale"/> property.
    /// </summary>
    [DebuggerDisplay("#{ticks} {Year}.{Month}.{Day} {Hour}:{Minute}:{Second}")]
    [Serializable]
    public struct Time : IEquatable<Time>
    {
        #region Static members - per class configuration

        static Timescale scale = Timescale.Java; // this is the default time scale

        /// <summary>Gets or sets the scale. </summary>
        /// <value>The <see cref="Timescale"/> used in computations of <see cref="Time"/>. 
        /// In the common case, all Time instances inside an application use the same scale. For 
        /// performance and simplicity reasons, this API therefore keeps a single process wide
        /// scale value in this property.	   
        /// </value>
        public static Timescale Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>The behavior upon <see cref="Timescale"/> collision . </summary>
        /// <remarks>Each TeaFile holds a <see cref="Timescale"/> value that defines how to interpret tick counts int the file.
        /// For performance and simplicity reasons, this API a maintains a single process wide <see cref="Timescale"/>
        /// value in <see cref="Time.Scale"/> that allows simple conversion of time values like <see cref="Time.Day"/>.
        /// When a TeaFile is opened and it's Timescale conflicts with the default Timescale, then the <see cref="ScaleCollisionBehavior"/>
        /// defines how to behave.<br/><br/>
        /// It is recommended to leave this value at its default of <see cref="TeaTime.ScaleCollisionBehavior.ThrowException"/> and 
        /// to keep a uniform Timescale across all files used.
        /// </remarks>
        public static ScaleCollisionBehavior ScaleCollisionBehavior { get; set; }

        #endregion

        #region State

        long ticks;

        #endregion

        /// <summary>Constructor. </summary>
        /// <param name="year"> The year. </param>
        /// <param name="month">The month. </param>
        /// <param name="day">  The day. </param>
        public Time(int year, int month, int day)
        {
            var netTime = new DateTime(year, month, day);
            this.ticks = Scale.NetToScale(netTime.Ticks);
        }

        /// <summary>Constructor. </summary>
        /// <param name="year">  The year. </param>
        /// <param name="month"> The month. </param>
        /// <param name="day">   The day. </param>
        /// <param name="hour">  The hour. </param>
        /// <param name="minute">The minute. </param>
        /// <param name="second">The second. </param>
        public Time(int year, int month, int day, int hour, int minute, int second)
        {
            var netTime = new DateTime(year, month, day, hour, minute, second);
            this.ticks = Scale.NetToScale(netTime.Ticks);
        }

        /// <summary>Constructor. </summary>
        /// <param name="year">       The year. </param>
        /// <param name="month">      The month. </param>
        /// <param name="day">        The day. </param>
        /// <param name="hour">       The hour. </param>
        /// <param name="minute">     The minute. </param>
        /// <param name="second">     The second. </param>
        /// <param name="millisecond">The millisecond. </param>
        public Time(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            var netTime = new DateTime(year, month, day, hour, minute, second, millisecond);
            this.ticks = Scale.NetToScale(netTime.Ticks);
        }

        /// <summary>Constructor. </summary>
        /// <param name="ticks">The ticks. </param>
        public Time(Int64 ticks)
        {
            this.ticks = ticks;
        }

        #region System.DateTime delegations

        /// <summary>
        /// Gets the day of the month represented by this instance.
        /// </summary>
        /// <returns>
        /// The day component, expressed as a value between 1 and 31.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Day
        {
            get { return this.NetTime.Day; }
        }

        /// <summary>
        /// Gets the day of the week represented by this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.DayOfWeek"/> enumerated constant that indicates the day of the week of this <see cref="T:System.DateTime"/> value. 
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public DayOfWeek DayOfWeek
        {
            get { return this.NetTime.DayOfWeek; }
        }

        /// <summary>
        /// Gets the day of the year represented by this instance.
        /// </summary>
        /// <returns>
        /// The day of the year, expressed as a value between 1 and 366.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int DayOfYear
        {
            get { return this.NetTime.DayOfYear; }
        }

        /// <summary>
        /// Gets the hour component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The hour component, expressed as a value between 0 and 23.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Hour
        {
            get { return this.NetTime.Hour; }
        }

        /// <summary>
        /// Gets the milliseconds component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The milliseconds component, expressed as a value between 0 and 999.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Millisecond
        {
            get { return this.NetTime.Millisecond; }
        }

        /// <summary>
        /// Gets the minute component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The minute component, expressed as a value between 0 and 59.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Minute
        {
            get { return this.NetTime.Minute; }
        }

        /// <summary>
        /// Gets the month component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The month component, expressed as a value between 1 and 12.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Month
        {
            get { return this.NetTime.Month; }
        }

        /// <summary>
        /// Gets the seconds component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The seconds, between 0 and 59.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Second
        {
            get { return this.NetTime.Second; }
        }

        /// <summary>
        /// Gets the year component of the date represented by this instance.
        /// </summary>
        /// <returns>
        /// The year, between 1 and 9999.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int Year
        {
            get { return this.NetTime.Year; }
        }

        /// <summary>
        /// The underlying number of Ticks.
        /// </summary>
        /// <remarks>
        /// This number is the only value persisted by an instance of this class. The <see cref="Scale"/> property 
        /// determines the relation between ticks and the time and date they represent.
        /// </remarks>
        public long Ticks
        {
            get { return this.ticks; }
        }

        /// <summary>
        /// Converts a <see cref="Time"/> value to a <see cref="DateTime"/> value.
        /// </summary>
        /// <remarks>
        /// This property returns the same value as an implicit conversion to <see cref="DateTime"/>.
        /// </remarks>
        public DateTime NetTime
        {
            get
            {
                long netTicks = Scale.ScaleToNet(this.ticks);
                return new DateTime(netTicks);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        /// <remarks>Internally, the string is converted into a System.DateTime instance whose ToString() method is then called.</remarks>
        public override string ToString()
        {
            try
            {
                return this.NetTime.ToString();
            }
            catch (Exception ex)
            {
                return "invalid: " + ex.Message;
            }
        }

        /// <summary>
        /// Describes the instance as a string.
        /// </summary>
        /// <returns>
        /// A string representation of value of the current <see cref="T:System.DateTime"/> object as specified by <paramref name="format"/>.
        /// If the date time value is invalid, the text "invalid" is returned.
        /// </returns>
        /// <remarks>
        /// Time holds an underlying <see cref="Int64"/> value ticks that is converted to a <see cref="DateTime"/> value and then formatted as a <see cref="string"/>.
        /// This conversion might fail if the tick value is out of range.
        /// </remarks>
        /// <param name="format">A standard or custom date and time format string. </param><exception cref="T:System.FormatException">The length of <paramref name="format"/> is 1, and it is not one of the format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo"/>.-or- <paramref name="format"/> does not contain a valid custom format pattern. </exception><filterpriority>1</filterpriority>
        public string ToString(string format)
        {
            try
            {
                return this.NetTime.ToString(format);
            }
            catch (Exception ex)
            {
                return "invalid: " + ex.Message;
            }
        }

        #endregion

        #region Implementation of IEquatable<Time>

        /// <summary>Tests if this Time is considered equal to another. </summary>
        /// <param name="other">The time to compare to this object. </param>
        /// <returns>true if the objects are considered equal, false if they are not. </returns>
        public bool Equals(Time other)
        {
            return other.ticks == this.ticks;
        }

        #endregion

        #region Operators

        /// <summary>Equality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are considered equivalent. </returns>
        public static bool operator ==(Time left, Time right)
        {
            return left.Ticks == right.Ticks;
        }

        /// <summary>Inequality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are not considered equivalent. </returns>
        public static bool operator !=(Time left, Time right)
        {
            return left.Ticks != right.Ticks;
        }

        /// <summary>Less-than comparison operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the first parameter is less than the second. </returns>
        public static bool operator <(Time left, Time right)
        {
            return left.Ticks < right.Ticks;
        }

        /// <summary>Greater-than comparison operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the first parameter is greater than to the second. </returns>
        public static bool operator >(Time left, Time right)
        {
            return left.Ticks > right.Ticks;
        }

        /// <summary>Less-than-or-equal comparison operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the first parameter is less than or equal to the second. </returns>
        public static bool operator <=(Time left, Time right)
        {
            return left.Ticks <= right.Ticks;
        }

        /// <summary>Greater-than-or-equal comparison operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the first parameter is greater than or equal to the second. </returns>
        public static bool operator >=(Time left, Time right)
        {
            return left.Ticks >= right.Ticks;
        }

        #endregion

        #region DateTime interop

        /// <summary>DateTime casting operator. </summary>
        /// <param name="time">Allow to cast a <see cref="Time"/> value to <see cref="DateTime"/>. </param>
        public static implicit operator DateTime(Time time)
        {
            return time.NetTime;
        }

        /// <summary>Time casting operator. </summary>
        /// <param name="dateTime">Allow to create a <see cref="Time"/> value from a <see cref="DateTime"/> value. </param>
        public static implicit operator Time(DateTime dateTime)
        {
            var ticks = Scale.NetToScale(dateTime.Ticks);
            return new Time(ticks);
        }

        /// <summary>Equality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are considered equivalent. </returns>
        public static bool operator ==(Time left, DateTime right)
        {
            return left.NetTime.Ticks == right.Ticks;
        }

        /// <summary>Inequality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are not considered equivalent. </returns>
        public static bool operator !=(Time left, DateTime right)
        {
            return !(left == right);
        }

        /// <summary>Equality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are considered equivalent. </returns>
        public static bool operator ==(DateTime left, Time right)
        {
            return left.Ticks == right.NetTime.Ticks;
        }

        /// <summary>Inequality operator. </summary>
        /// <param name="left"> The first instance to compare. </param>
        /// <param name="right">The second instance to compare. </param>
        /// <returns>true if the parameters are not considered equivalent. </returns>
        public static bool operator !=(DateTime left, Time right)
        {
            return !(left == right);
        }

        #endregion

        #region Elementary Methods

        /// <summary>Indicates whether this instance and a specified object are equal. </summary>
        /// <param name="obj">Another object to compare to. </param>
        /// <returns>
        /// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((obj is Time) && this.Equals((Time)obj)) ||
                   ((obj is DateTime) && this.NetTime == (DateTime)obj);
        }

        /// <summary>Returns the hash code for this instance. </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance. </returns>
        public override int GetHashCode()
        {
            return this.ticks.GetHashCode();
        }

        /// <summary>Compares this Time object to another to determine their relative ordering. </summary>
        /// <param name="other">Another instance to compare. </param>
        /// <returns>Negative if 'other' is less than '', 0 if they are equal, or positive if it is greater. </returns>
        public int Compare(Time other)
        {
            return this.ticks.CompareTo(other);
        }

        #endregion

        #region Time Domain

        /// <summary>Returns a new DateTime that adds the specified number of days to the value of this instance.</summary>
        /// <param name="numberOfDays">A number of whole and fractional days. The parameter can be negative or positive.</param>
        /// <returns>A new Time instance whose value is the sum of the date and time represented by this instance and 
        /// <see paramref="numberOfDays"/>. </returns>
        public Time AddDays(double numberOfDays)
        {
            return new Time(this.Ticks + (long)(numberOfDays * scale.TicksPerDay));
        }

        /// <summary>Returns a new DateTime that adds the specified number of hours to the value of this instance.</summary>
        /// <param name="hours">A number of whole and fractional hours. The parameter can be negative or positive.</param>
        /// <returns>A new Time instance whose value is the sum of the date and time represented by this instance and 
        /// <see paramref="hours"/>. </returns>
        public Time AddHours(double hours)
        {
            return new Time((long)(this.Ticks + hours * scale.TicksPerDay / 24));
        }

        /// <summary>Returns a new DateTime that adds the specified number of minutes to the value of this instance.</summary>
        /// <param name="minutes">A number of whole and fractional minutes. The parameter can be negative or positive.</param>
        /// <returns>A new Time instance whose value is the sum of the date and time represented by this instance and 
        /// <see paramref="minutes"/>. </returns>
        public Time AddMinutes(double minutes)
        {
            return new Time((long)(this.Ticks + (minutes * scale.TicksPerDay / (24 * 60))));
        }

        /// <summary>Returns a new DateTime that adds the specified number of seconds to the value of this instance.</summary>
        /// <param name="seconds">A number of whole and fractional seconds. The parameter can be negative or positive.</param>
        /// <returns>A new Time instance whose value is the sum of the date and time represented by this instance and 
        /// <see paramref="seconds"/>. </returns>
        public Time AddSeconds(double seconds)
        {
            return new Time((long)(this.Ticks + seconds * scale.TicksPerDay / (24 * 60 * 60)));
        }

        /// <summary>Gets the date of this instance. </summary>
        /// <value>A new instance of <see cref="Time"/> holding the date part of this instance, stripping the time of the day.</value>
        public Time Date
        {
            get { return new Time(this.Ticks - (this.Ticks % Scale.TicksPerDay)); }
        }

        #endregion
    }
}
