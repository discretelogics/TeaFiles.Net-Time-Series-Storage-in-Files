using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeaTime
{
    /// <summary>
    /// Describes the time format applicable to the ticks stored in the time series items.<br></br>
    /// This API allows Time Series with timestamps that have different resolutions and epoch starts. 
    /// Commonly used time representations count the ticks from some point in time. Usually the ticks 
    /// count the number of 
    /// <list>
    /// <item>Seconds (e.g. Ruby's Time class before 1.9)</item>
    /// <item>Milliseconds (e.g. java.util.Date)</item>
    /// <item>0.1 Microseconds = 100 Nanoseconds (e.g. .Net System.DateTime)</item>
    /// </list>
    /// <see cref="Time"/> can map handle all these different formats by setting it's static property 
    /// <see cref="Time.Scale"/> to an instance of <see cref="Timescale"/> that holds the epoch offset and 
    /// tick resolution to be used.
    /// 
    /// This class describes a scale while class <see cref="Timescale"/> is used for computation.
    /// </summary>
    public class TimeFormat
    {
        /// <summary>
        /// This is the number of days since 1.1.1 at which the epoch starts.
        /// </summary>
        public long Epoch { get; set; }

        /// <summary>
        /// The number of ticks per day. If time resolution is required only at the day level this can be set to 1.
        /// </summary>
        public long TicksPerDay { get; set; }

        public Timescale Scale
        {
            get { return Timescale.FromEpoch(this.Epoch, this.TicksPerDay); }
        }

        public bool Equals(Timescale scale)
        {
            return this.Epoch == scale.Epoch &&
                   this.TicksPerDay == scale.TicksPerDay;
        }
    }
}
