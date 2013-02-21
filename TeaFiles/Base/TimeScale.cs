// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;

namespace TeaTime
{
    /// <summary>Describes how raw tick counts are converted to date and time values.</summary>
    /// <remarks>
    /// <h2>Time representation in TeaFiles</h2>
    /// <para>
    /// TeaFiles shall be usable by various applications and operatings systems. Each of 
    /// these usually offer some system to deal with date, time or datetime values. TeaFiles 
    /// can hold any of the values used by these systems. This class holds the values required 
    /// to interpret raw time values.<br/><br/>
    /// </para>
    /// <example>
    /// A TeaFile might store items like
    /// <code>
    /// struct Observation
    /// {
    ///     public long     Time;
    ///     public double   Temperature;
    /// }
    /// </code>
    /// Lets say an observation holds a time value of 86410. To get the date and time of this integer value, 
    /// we need to know the interval between ticks and the date/time where the counting started. <see cref="Timescale"/>
    /// provides these values as <see cref="TicksPerDay"/> and <see cref="Epoch"/>.
    /// </example>
    /// The combination of a raw value, this might be an unsigned or signed integer or a floating point value and 
    /// a time scale stored once for a time series gives enough flexibility to store values used by any 
    /// time system.
    /// </remarks>
    public struct Timescale : IEquatable<Timescale>
    {
        #region Well known constants

        /// <summary>A Timescale instance with an epoch of 1.1.1970 and Millisecond resolution.</summary>
        /// <remarks>Since Java uses this epoch and resolution, we call it "Java" Timescale.</remarks>
        public static Timescale Java
        {
            get { return FromEpoch(1970, 1, 1, MillisecondsPerDay); }
        }

        /// <summary>A Timescale instance with an epoch of 1.1.0000 and 0.1 Microsecond resolution.</summary>
        /// <remarks>Since .Net uses this epoch and resolution, we call it "Net" Timescale.<br/>
        /// TeaFiles that shall be universally accessible should not use this time scale, as the value range
        /// causes problems on platforms that do not provide 64bit values, like the R application.
        /// </remarks>
        public static Timescale Net
        {
            get { return FromEpoch(1, 1, 1, 10 * MicrosecondsPerDay); }
        }

        #endregion

        #region Constants

        /// <summary>The seconds per day = 86400. </summary>
        public const long SecondsPerDay = 60 * 60 * 24; // = 86400

        /// <summary>The milliseconds per day = 86400000. </summary>
        public const long MillisecondsPerDay = SecondsPerDay * 1000;

        /// <summary>The microseconds per day = 86400000000. </summary>
        public const long MicrosecondsPerDay = MillisecondsPerDay * 1000;

        #endregion

        #region State

        /// <summary>The epoch specified as number of days between 1.1.0000 and the epoch. </summary>
        long epochOffsetInDays;

        /// <summary>The ticks per day. </summary>
        long ticksPerDay;

        /// <summary>The epoch offsetin net ticks. </summary>
        long netEpochIffsetInDays;

        /// <summary>The net ticks per scale tick. </summary>
        long netTicksPerDayTick;

        #endregion

        #region Factory

        /// <summary>Create a new Timescale instance from the epoch day/month/year and the number of ticks per day. </summary>
        /// <remarks>Static factory methods are provided since Timescale is a struct. </remarks>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values. </exception>
        /// <param name="epochYear">The epoch year. </param>
        /// <param name="epochMonth">The epoch month. </param>
        /// <param name="epochDay">The epoch day. </param>
        /// <param name="ticksPerDay">The ticks counted per day. </param>
        /// <returns>A new Timescale instance. </returns>
        public static Timescale FromEpoch(int epochYear, int epochMonth, int epochDay, long ticksPerDay)
        {
            if (ticksPerDay > TimeSpan.TicksPerDay) throw new ArgumentException("ticksPerDay must be less than 100ns / s resolution.");

            var epoch = new DateTime(epochYear, epochMonth, epochDay);
            var duration = epoch - DateTime.MinValue;

            return FromEpoch((long)duration.TotalDays, ticksPerDay);
        }

        /// <summary>Create a new Timescale instance from the epoch in days and the number of ticks per day. </summary>
        /// <remarks>Static factory methods are provided since Timescale is a struct. </remarks>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values. </exception>
        /// <param name="epochOffsetInDays">The epoch specified as number of days between 1.1.0000 and the epoch. </param>
        /// <param name="ticksPerDay">The ticks counted per day. </param>
        /// <returns>A new Timescale instance. </returns>
        public static Timescale FromEpoch(long epochOffsetInDays, long ticksPerDay)
        {
            if (ticksPerDay > TimeSpan.TicksPerDay) throw new ArgumentException("ticksPerDay must be below 100ns / s resolution.");

            var scale = new Timescale();

            scale.epochOffsetInDays = epochOffsetInDays;
            scale.ticksPerDay = ticksPerDay;

            scale.netTicksPerDayTick = TimeSpan.TicksPerDay / scale.ticksPerDay;
            scale.netEpochIffsetInDays = scale.epochOffsetInDays * TimeSpan.TicksPerDay;

            return scale;
        }

        #endregion

        #region Core

        /// <summary>The epoch as the number of days from 1.1.0000. </summary>
        public long Epoch
        {
            get { return this.epochOffsetInDays; }
        }

        /// <summary>The number of ticks counted per day. </summary>
        public long TicksPerDay
        {
            get { return this.ticksPerDay; }
        }

        /// <summary>Returns "Java" or "Net" if this instance has the epoch and ticks per day of these well known scales. "Custom" otherwise.</summary>
        public string WellKnownName
        {
            get
            {
                if (this == Net) return "Net";
                if (this == Java) return "Java";
                return "Custom";
            }
        }

        /// <summary>Converts Net scaled ticks to ticks in the scale of this instance. </summary>
        /// <param name="netTicks">The number of ticks at .Net DateTime scale. </param>
        /// <returns>The number of ticks in the scale of this instance. </returns>
        internal long NetToScale(long netTicks)
        {
            var epochAdjusted = netTicks - this.netEpochIffsetInDays;
            return epochAdjusted / this.netTicksPerDayTick;
        }

        /// <summary>Converts ticks scaled in the scale of this instance to .Net DateTime ticks. </summary>
        /// <param name="scaledTicks">The number ticks in this instance's scale. </param>
        /// <returns>.Net based ticks. </returns>
        public long ScaleToNet(long scaledTicks)
        {
            var ticks = this.netTicksPerDayTick * scaledTicks;
            ticks += this.netEpochIffsetInDays;
            return ticks;
        }

        #endregion

        #region Implementation of IEquatable<TimeScale>

        /// <summary>Tests if this Timescale is considered equal to another. </summary>
        /// <param name="other">The timescale to compare to this object. </param>
        /// <returns>true if the objects are considered equal, false if they are not. </returns>
        public bool Equals(Timescale other)
        {
            return this.ticksPerDay == other.ticksPerDay &&
                   this.epochOffsetInDays == other.epochOffsetInDays;
        }

        #endregion

        #region Equality

        /// <summary>Equality operator. </summary>
        /// <param name="scale1">The first instance to compare. </param>
        /// <param name="scale2">The second instance to compare. </param>
        /// <returns>true if the parameters are considered equivalent. </returns>
        public static bool operator ==(Timescale scale1, Timescale scale2)
        {
            return scale1.Equals(scale2);
        }

        /// <summary>Inequality operator. </summary>
        /// <param name="scale1">The first instance to compare. </param>
        /// <param name="scale2">The second instance to compare. </param>
        /// <returns>true if the parameters are not considered equivalent. </returns>
        public static bool operator !=(Timescale scale1, Timescale scale2)
        {
            return !(scale1 == scale2);
        }

        /// <summary>Indicates whether this instance and a specified object are equal. </summary>
        /// <param name="obj">Another object to compare to. </param>
        /// <returns>
        /// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Timescale) && this.Equals((Timescale)obj);
        }

        /// <summary>Returns the hash code for this instance. </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance. </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.epochOffsetInDays.GetHashCode() * 397) ^ this.ticksPerDay.GetHashCode();
            }
        }

        #endregion

        /// <summary>Returns the fully qualified type name of this instance. </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name. </returns>
        public override string ToString()
        {
            if (this == Net) return "Net";
            if (this == Java) return "Java";
            return "{0},{1}".Formatted(this.Epoch, this.ticksPerDay);
        }
    }
}
